import fs from 'node:fs';import path from 'node:path';import {fileURLToPath} from 'node:url';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..'),data=JSON.parse(fs.readFileSync(path.join(root,'Game/Assets/_CelestialMaker/Resources/content.json'),'utf8'));
const categories=['generators','upgrades','research','talents','challenges','achievements','expeditions','stories'],all=categories.flatMap(k=>data[k]),ids=new Set(all.map(d=>d.id));
if(ids.size!==all.length)throw Error('Duplicate content ID');
for(const category of ['research','talents']){const defs=new Map(data[category].map(d=>[d.id,d]));for(const item of defs.values()){const seen=new Set();let next=item;while(next?.requires){if(seen.has(next.id))throw Error('Cyclic prerequisite');seen.add(next.id);next=defs.get(next.requires);if(!next)throw Error('Missing prerequisite');}}}
for(const g of data.generators)if(g.input>=g.output)throw Error('Resource cycle');
const report={utc:new Date().toISOString(),counts:Object.fromEntries(categories.map(k=>[k,data[k].length])),stages:Array.from({length:6},(_,stage)=>({stage,processes:data.generators.filter(g=>g.stage===stage).length})),uniqueIDs:ids.size,acyclicResourceGraph:true,acyclicResearchAndTalents:true,emptyNames:all.filter(d=>!d.name).length,emptyStories:data.stories.filter(d=>!d.text.trim()).length};
fs.writeFileSync(path.join(root,'Logs/content-audit.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report));
