import test from 'node:test';import assert from 'node:assert/strict';import {spawn} from 'node:child_process';import {mkdtempSync,rmSync} from 'node:fs';import {tmpdir} from 'node:os';import path from 'node:path';
import {DatabaseSync} from 'node:sqlite';import {createHash} from 'node:crypto';
test('zero-key optional backend and removed billing routes',async()=>{
 const dir=mkdtempSync(path.join(tmpdir(),'celestial-free-')),port=46319;
 const fixture=new DatabaseSync(path.join(dir,'test.db'));fixture.exec('CREATE TABLE sessions(token TEXT PRIMARY KEY,user_id TEXT NOT NULL,expires INTEGER NOT NULL); CREATE TABLE transactions(user_id TEXT,product TEXT,status TEXT)');fixture.prepare('INSERT INTO sessions VALUES(?,?,?)').run(createHash('sha256').update('local-fixture').digest('hex'),'fixture-owner',Date.now()+60000);for(const product of ['themes','supporter','boost'])fixture.prepare('INSERT INTO transactions VALUES(?,?,?)').run('fixture-owner',product,'paid');fixture.prepare('INSERT INTO transactions VALUES(?,?,?)').run('fixture-owner','boost','refunded');fixture.close();
 const child=spawn(process.execPath,['server.ts'],{cwd:new URL('..',import.meta.url),windowsHide:true,env:{...process.env,PORT:String(port),DATABASE_PATH:path.join(dir,'test.db'),OIDC_ISSUER:'',OIDC_CLIENT_ID:''},stdio:['ignore','pipe','pipe']});
 try{
  await new Promise<void>((resolve,reject)=>{const timer=setTimeout(()=>reject(Error('startup timed out')),10000);child.stdout.on('data',()=>{clearTimeout(timer);resolve();});child.on('exit',code=>{clearTimeout(timer);reject(Error('server exit '+code));});});
  const base='http://127.0.0.1:'+port,health=await(await fetch(base+'/health')).json();assert.equal(health.freeGame,true);assert.equal(health.accounts,false);
  for(const route of ['/checkout','/boost/activate','/stripe/webhook','/google/verify','/google/notifications','/entitlements'])for(const method of ['GET','POST'])assert.equal((await fetch(base+route,{method,headers:{Origin:'http://localhost:4310'}})).status,404,method+' '+route);
  assert.equal((await fetch(base+'/profile')).status,400);
  const profile=await(await fetch(base+'/profile',{headers:{Authorization:'Bearer local-fixture'}})).json();assert.deepEqual(profile.cosmetics,['theme-1','theme-2']);assert.equal(profile.historicSupporter,true);assert.equal(profile.legacyRecordCount,3);assert.equal('boostMultiplier' in profile,false);const retained=new DatabaseSync(path.join(dir,'test.db'),{readOnly:true});assert.equal(retained.prepare('SELECT COUNT(*) AS n FROM transactions').get()!.n,4);retained.close();
 }finally{child.kill();await new Promise(resolve=>child.once('exit',resolve));rmSync(dir,{recursive:true,force:true});}
});
