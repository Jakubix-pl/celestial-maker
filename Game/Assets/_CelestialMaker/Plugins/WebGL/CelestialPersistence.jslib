mergeInto(LibraryManager.library, {
 CM_Load:function(){
  window.cmSaveSlot=new URL(location.href).searchParams.get("qaProfile")||"profile";
  window.cmDB= new Promise((resolve,reject)=>{const r=indexedDB.open('celestial-maker',1);r.onupgradeneeded=()=>r.result.createObjectStore('saves');r.onsuccess=()=>resolve(r.result);r.onerror=()=>reject(r.error);});
  const load=()=>window.cmDB.then(db=>new Promise((resolve,reject)=>{const r=db.transaction('saves').objectStore('saves').get(window.cmSaveSlot);r.onsuccess=()=>resolve(r.result||'');r.onerror=()=>reject(r.error);})).then(s=>SendMessage('GameSession','WebLoadComplete',s)).catch(e=>{SendMessage('GameSession','WebReadOnly','');SendMessage('GameSession','WebLoadComplete','');SendMessage('GameSession','WebSaveComplete','Storage unavailable: '+String(e));});
  if(navigator.locks){navigator.locks.request(window.cmSaveSlot==='profile'?'celestial-save-writer':'celestial-save-writer-'+window.cmSaveSlot,{ifAvailable:true},lock=>{if(!lock){SendMessage('GameSession','WebReadOnly','');return load();}return new Promise(resolve=>{window.cmRelease=resolve;load();});});}else{SendMessage('GameSession','WebReadOnly','');load();}
  document.addEventListener('visibilitychange',()=>SendMessage('GameSession','WebVisibility',document.hidden?'hidden':'visible'));
 },
 CM_Save:function(ptr){const text=UTF8ToString(ptr);window.cmDB.then(db=>new Promise((resolve,reject)=>{const tx=db.transaction('saves','readwrite');tx.objectStore('saves').put(text,window.cmSaveSlot);tx.oncomplete=()=>resolve();tx.onerror=()=>reject(tx.error);tx.onabort=()=>reject(tx.error);})).then(()=>SendMessage('GameSession','WebSaveComplete','ok')).catch(e=>SendMessage('GameSession','WebSaveComplete',String(e)));},
 CM_Download:function(ptr){const b=new Blob([UTF8ToString(ptr)],{type:'application/json'}),a=document.createElement('a');a.href=URL.createObjectURL(b);a.download='celestial-save.json';a.click();setTimeout(()=>URL.revokeObjectURL(a.href),1000);},
 CM_RequestImport:function(){
  if(document.getElementById('cm-import-dialog'))return;
  const cover=document.createElement('div');cover.id='cm-import-dialog';cover.setAttribute('role','dialog');cover.setAttribute('aria-label','Import save');cover.style.cssText='position:fixed;inset:0;z-index:9999;background:#020715ee;display:flex;align-items:center;justify-content:center;padding:12px;';
  const form=document.createElement('form');form.style.cssText='background:#111b2d;color:#eef5ff;padding:20px;width:min(600px,100%);max-height:90vh;overflow:auto;';
  const title=document.createElement('h2');title.textContent='Import a save';
  const help=document.createElement('p');help.textContent='Choose an exported JSON file or paste its contents. Paid products are verified separately. Your current save is replaced only after validation.';
  const file=document.createElement('input');file.type='file';file.accept='.json,application/json';file.setAttribute('aria-label','Save file');
  const input=document.createElement('textarea');input.setAttribute('aria-label','Save JSON');input.placeholder='Paste an exported JSON save';input.style.cssText='display:block;width:100%;height:200px;box-sizing:border-box;margin:16px 0;color:#eef5ff;background:#07101f;';
  const status=document.createElement('p');status.setAttribute('role','status');
  file.onchange=async()=>{const f=file.files[0];if(!f)return;if(f.size>2097152){status.textContent='Save exceeds 2 MB.';return;}try{input.value=await f.text();status.textContent='File loaded. Choose Import validated save to continue.';}catch(e){status.textContent='Could not read file.';}};
  const submit=document.createElement('button');submit.type='submit';submit.textContent='Import validated save';
  const cancel=document.createElement('button');cancel.type='button';cancel.textContent='Cancel';cancel.style.marginLeft='12px';
  const close=()=>cover.remove();cancel.onclick=()=>{close();SendMessage('GameSession','WebImportCancelled','');};
  form.onsubmit=e=>{e.preventDefault();if(!input.value.trim()||input.value.length>2097152){status.textContent='Paste a nonempty save, up to 2 MB.';return;}const text=input.value;close();SendMessage('GameSession','WebImportComplete',text);};
  form.append(title,help,file,input,status,submit,cancel);cover.append(form);document.body.append(cover);input.focus();
 }
});
