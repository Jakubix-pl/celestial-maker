import fs from 'node:fs';import path from 'node:path';import {execFileSync} from 'node:child_process';import {fileURLToPath} from 'node:url';import {createHash} from 'node:crypto';
const dir=path.dirname(fileURLToPath(import.meta.url)),dist=path.join(dir,'dist-free');fs.mkdirSync(dist,{recursive:true});
execFileSync(process.execPath,[path.join(dir,'node_modules/typescript/bin/tsc'),'--target','ES2022','--module','ES2022','--lib','ES2022,DOM','--strict','--outDir',dist,path.join(dir,'app.ts')],{stdio:'inherit'});
for(const file of ['index.html','style.css','favicon.svg','manifest.webmanifest'])fs.copyFileSync(path.join(dir,file),path.join(dist,file));
if(fs.existsSync(path.join(dir,'public/media')))fs.cpSync(path.join(dir,'public/media'),path.join(dist,'media'),{recursive:true});
const freeWeb=path.join(dir,'public/game/expansion-version.txt');if(fs.existsSync(freeWeb)&&fs.readFileSync(freeWeb,'utf8').trim()==='2.0.0')fs.cpSync(path.join(dir,'public/game'),path.join(dist,'game'),{recursive:true});
const build=path.join(dist,'game/Build');let manifest;
if(fs.existsSync(build)){
 const files=fs.readdirSync(build),names={loader:files.find(f=>f.endsWith('.loader.js')),data:files.find(f=>f.endsWith('.data')),framework:files.find(f=>f.endsWith('.framework.js')),code:files.find(f=>f.endsWith('.wasm'))};
 if(Object.values(names).some(f=>!f))throw Error('Incomplete Unity Web build');
 const digest=createHash('sha256');for(const name of Object.values(names))digest.update(fs.readFileSync(path.join(build,name)));
 const version=digest.digest('hex').slice(0,16),release='/game/releases/'+version,target=path.join(dist,release);
 fs.mkdirSync(path.join(target,'Build'),{recursive:true});for(const name of Object.values(names))fs.copyFileSync(path.join(build,name),path.join(target,'Build',name));
 const streaming=path.join(dist,'game/StreamingAssets');if(fs.existsSync(streaming))fs.cpSync(streaming,path.join(target,'StreamingAssets'),{recursive:true});
 manifest=Object.fromEntries(Object.entries(names).map(([key,name])=>[key,release+'/Build/'+name]));manifest.streaming=release+'/StreamingAssets';
 fs.writeFileSync(path.join(dist,'game-manifest.json'),JSON.stringify(manifest));
}
if(!manifest)fs.writeFileSync(path.join(dist,'game-manifest.json'),JSON.stringify({unavailable:'The free PC expansion is ready for download. The updated Web player has not been built in this PC-only pass.'}));
const downloads={};for(const [name,file] of [['Windows','CelestialMaker-Expansion-Windows.zip'],['Android','Android/CelestialMaker.apk']]){const src=path.join(dir,'../Builds',file);if(fs.existsSync(src)){fs.mkdirSync(path.join(dist,'downloads'),{recursive:true});const dest=path.basename(file);fs.copyFileSync(src,path.join(dist,'downloads',dest));downloads[name]='/downloads/'+dest;}}fs.writeFileSync(path.join(dist,'downloads.json'),JSON.stringify(downloads));
const appBytes=fs.readFileSync(path.join(dist,'app.js')),cssBytes=fs.readFileSync(path.join(dist,'style.css')),rawIndex=fs.readFileSync(path.join(dist,'index.html'),'utf8');
const siteVersion=createHash('sha256').update(appBytes).update(cssBytes).update(rawIndex).update(JSON.stringify(manifest||{})).digest('hex').slice(0,16),site='/site-releases/'+siteVersion;
fs.mkdirSync(path.join(dist,site),{recursive:true});fs.writeFileSync(path.join(dist,site,'app.js'),appBytes);fs.writeFileSync(path.join(dist,site,'style.css'),cssBytes);
const index=rawIndex.replace('href="/style.css"','href="'+site+'/style.css"').replace('src="/app.js"','src="'+site+'/app.js"');fs.writeFileSync(path.join(dist,'index.html'),index);
const precache=[site+'/app.js',site+'/style.css','/favicon.svg','/manifest.webmanifest','/media/cosmos-desktop.png',...(manifest?Object.entries(manifest).filter(([key])=>key!=='streaming').map(([,value])=>value):[])];
// Cache the manifest and entry document from this exact build, not a mutable URL
// that could change while a worker is installing during another deployment.
fs.writeFileSync(path.join(dist,'sw.js'),`const CACHE='celestial-${siteVersion}',FILES=${JSON.stringify(precache)},ENTRY=${JSON.stringify(index)},GAME=${JSON.stringify(JSON.stringify(manifest||{}))};self.addEventListener('install',e=>e.waitUntil(caches.open(CACHE).then(async c=>{await c.addAll(FILES);await c.put('/',new Response(ENTRY,{headers:{'Content-Type':'text/html'}}));await c.put('/game-manifest.json',new Response(GAME,{headers:{'Content-Type':'application/json'}}));})));self.addEventListener('activate',e=>e.waitUntil(caches.keys().then(keys=>Promise.all(keys.filter(k=>k.startsWith('celestial-')&&k!==CACHE).map(k=>caches.delete(k))))));self.addEventListener('fetch',e=>{const u=new URL(e.request.url);if(e.request.method!=='GET'||u.origin!==location.origin||u.pathname.startsWith('/api/')||u.pathname==='/account'||u.pathname.startsWith('/downloads'))return;e.respondWith(fetch(e.request).catch(()=>caches.open(CACHE).then(async c=>(await c.match(e.request))||(e.request.mode==='navigate'?c.match('/'):Response.error()))));});`);

console.log('Website built:',dist,'Unity game:',!!manifest);


