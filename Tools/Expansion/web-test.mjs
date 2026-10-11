import {spawn} from 'node:child_process';
import assert from 'node:assert/strict';
const port=46320,child=spawn(process.execPath,['Website/server.mjs'],{windowsHide:true,env:{...process.env,PORT:String(port)},stdio:['ignore','pipe','pipe']});
try{
 await new Promise((resolve,reject)=>{const timer=setTimeout(()=>reject(Error('startup timeout')),10000);child.stdout.once('data',()=>{clearTimeout(timer);resolve();});child.once('exit',code=>{clearTimeout(timer);reject(Error('server exit '+code));});});
 const base='http://127.0.0.1:'+port,html=await(await fetch(base)).text();assert.match(html,/site-releases/);const app=await(await fetch(base+'/app.js')).text();assert.doesNotMatch(app,/\/checkout|\/boost\/activate|stripe|googleapis/i);const web=await(await fetch(base+'/game-manifest.json')).json();assert.equal(typeof web.unavailable,'string');const downloads=await(await fetch(base+'/downloads.json')).json();assert.match(downloads.Windows,/Expansion-Windows\.zip$/);const pack=await fetch(base+downloads.Windows,{method:'HEAD'});assert.equal(pack.status,200);assert.ok(Number(pack.headers.get('content-length'))>1000000);console.log('PASS free site: HTML, no billing code, honest Web status, current PC download');
}finally{child.kill();await new Promise(resolve=>child.once('exit',resolve));}
