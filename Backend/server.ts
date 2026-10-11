import http from 'node:http';
import fs from 'node:fs';
import path from 'node:path';
import {randomBytes,createHash} from 'node:crypto';
import {DatabaseSync} from 'node:sqlite';
import * as oidc from 'openid-client';
const env=process.env,origin=env.PUBLIC_ORIGIN||'http://localhost:4310',backendOrigin=env.BACKEND_ORIGIN||'http://localhost:4311';
const database=env.DATABASE_PATH||'./data/accounts.db';fs.mkdirSync(path.dirname(database),{recursive:true});const db=new DatabaseSync(database);
db.exec('CREATE TABLE IF NOT EXISTS sessions(token TEXT PRIMARY KEY,user_id TEXT NOT NULL,expires INTEGER NOT NULL); CREATE TABLE IF NOT EXISTS login_flows(token TEXT PRIMARY KEY,verifier TEXT NOT NULL,state TEXT NOT NULL,expires INTEGER NOT NULL)');
const accountsReady=!!env.OIDC_ISSUER&&!!env.OIDC_CLIENT_ID;
let configPromise:Promise<oidc.Configuration>|undefined;
const config=()=>{if(!env.OIDC_ISSUER||!env.OIDC_CLIENT_ID)throw Error('Account provider is not configured');return configPromise??=oidc.discovery(new URL(env.OIDC_ISSUER),env.OIDC_CLIENT_ID,env.OIDC_CLIENT_SECRET).then(c=>{oidc.enableNonRepudiationChecks(c);return c;});};
const token=()=>randomBytes(32).toString('base64url'),hash=(s:string)=>createHash('sha256').update(s).digest('hex');
function respond(res:http.ServerResponse,status:number,value:unknown){res.writeHead(status,{'Content-Type':'application/json','Cache-Control':'no-store'});res.end(JSON.stringify(value));}
function redirect(res:http.ServerResponse,url:string,cookie?:string){res.writeHead(302,{'Location':url,...(cookie?{'Set-Cookie':cookie}:{})});res.end();}
function cookie(name:string,value:string,age=86400){return `${name}=${value}; HttpOnly; SameSite=Lax; Path=/; Max-Age=${age}${backendOrigin.startsWith('https:')?'; Secure':''}`;}
function cookies(req:http.IncomingMessage){return Object.fromEntries((req.headers.cookie||'').split(';').map(s=>s.trim().split('=')));}
function user(req:http.IncomingMessage):string{const raw=req.headers.authorization?.replace(/^Bearer /,'')||cookies(req).cm_session;const row=raw?db.prepare('SELECT user_id FROM sessions WHERE token=? AND expires>?').get(hash(raw),Date.now()):null;if(!row)throw Error('Sign in to a recoverable account first');return String(row.user_id);}
async function body(req:http.IncomingMessage){let total=0;const chunks:Buffer[]=[];for await(const chunk of req){total+=chunk.length;if(total>1024*1024)throw Error('Request exceeds 1 MB');chunks.push(chunk);}return Buffer.concat(chunks);}
function id(value:unknown){if(typeof value!=='string'||! /^[a-zA-Z0-9_-]{16,100}$/.test(value))throw Error('Invalid idempotency identifier');return value;}
function account(claims:Record<string,unknown>){if(!claims||typeof claims.sub!=='string'||claims.email_verified!==true)throw Error('A verified recoverable email account is required');return `${env.OIDC_ISSUER}|${claims.sub}`;}
function session(uid:string){const raw=token();db.prepare('INSERT INTO sessions VALUES(?,?,?)').run(hash(raw),uid,Date.now()+86400000);return raw;}
const device=new Map<string,{expires:number,status:string,session?:string,error?:string}>();
const server=http.createServer(async(req,res)=>{try{
 res.setHeader('Access-Control-Allow-Origin',origin);res.setHeader('Access-Control-Allow-Credentials','true');res.setHeader('Vary','Origin');res.setHeader('X-Content-Type-Options','nosniff');
 if(req.method==='OPTIONS'){res.writeHead(204,{'Access-Control-Allow-Headers':'Content-Type,Authorization','Access-Control-Allow-Methods':'GET,POST,OPTIONS'});res.end();return;}
 const url=new URL(req.url||'/',backendOrigin);
 if(req.method==='POST'&&url.pathname!=='/auth/device'&&req.headers.origin!==origin&& !req.headers.authorization)throw Error('Origin validation failed');
 if(url.pathname==='/health'){respond(res,200,{status:'ok',accounts:accountsReady,freeGame:true,version:'2.0.0'});return;}
 if(url.pathname==='/auth/login'){const c=await config(),flow=token(),verifier=oidc.randomPKCECodeVerifier(),state=oidc.randomState();db.prepare('INSERT INTO login_flows VALUES(?,?,?,?)').run(hash(flow),verifier,state,Date.now()+600000);const redirectUrl=oidc.buildAuthorizationUrl(c,{redirect_uri:backendOrigin+'/auth/callback',scope:'openid email profile',state,code_challenge:await oidc.calculatePKCECodeChallenge(verifier),code_challenge_method:'S256'});redirect(res,redirectUrl.href,cookie('cm_flow',flow,600));return;}
 if(url.pathname==='/auth/callback'){const flow=cookies(req).cm_flow;const row=flow?db.prepare('SELECT * FROM login_flows WHERE token=? AND expires>?').get(hash(flow),Date.now()):null;if(!row)throw Error('Login session expired');db.prepare('DELETE FROM login_flows WHERE token=?').run(hash(flow));const callback=new URL(backendOrigin+'/auth/callback');callback.search=url.search;const tokens=await oidc.authorizationCodeGrant(await config(),callback,{pkceCodeVerifier:String(row.verifier),expectedState:String(row.state),idTokenExpected:true});const uid=account(tokens.claims() as Record<string,unknown>);redirect(res,origin+'/account',cookie('cm_session',session(uid)));return;}
 if(url.pathname==='/auth/device'&&req.method==='POST'){const c=await config(),code=token(),auth=await oidc.initiateDeviceAuthorization(c,{scope:'openid email profile'});const state={expires:Date.now()+Number(auth.expires_in)*1000,status:'pending'} as {expires:number,status:string,session?:string,error?:string};device.set(code,state);oidc.pollDeviceAuthorizationGrant(c,auth).then(tokens=>{state.session=session(account(tokens.claims() as Record<string,unknown>));state.status='complete';}).catch(()=>{state.status='failed';state.error='Login cancelled or expired';});respond(res,200,{requestId:code,userCode:auth.user_code,verificationUrl:auth.verification_uri_complete||auth.verification_uri});return;}
 if(url.pathname==='/auth/device/poll'){const key=url.searchParams.get('id')||'',entry=device.get(key);if(!entry||entry.expires<Date.now())throw Error('Device login expired');respond(res,200,entry);if(entry.status!=='pending')device.delete(key);return;}
 if(url.pathname==='/auth/logout'&&req.method==='POST'){const raw=cookies(req).cm_session;if(raw)db.prepare('DELETE FROM sessions WHERE token=?').run(hash(raw));res.setHeader('Set-Cookie',cookie('cm_session','',0));respond(res,200,{ok:true});return;}
 if(url.pathname==='/profile'){
  const uid=user(req),exists=db.prepare("SELECT name FROM sqlite_master WHERE type='table' AND name='transactions'").get();
  const records=exists?db.prepare("SELECT product,COUNT(*) AS count FROM transactions WHERE user_id=? AND status='paid' GROUP BY product").all(uid):[];
  const themes=records.some(r=>r.product==='themes'||r.product==='supporter');
  respond(res,200,{cosmetics:themes?['theme-1','theme-2']:[],historicSupporter:records.some(r=>r.product==='supporter'),legacyRecordCount:records.reduce((n,r)=>n+Number(r.count),0)});return;
 }
 respond(res,404,{error:'Endpoint not found'});
 }catch(e){respond(res,400,{error:e instanceof Error?e.message:'Request failed'});}});
server.listen(Number(env.PORT||4311),env.HOST||'127.0.0.1',()=>console.log('Optional account service ready'));




