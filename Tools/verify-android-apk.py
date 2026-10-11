"""Inspect a built sideload APK with the SDK tools; never install or launch it."""
from pathlib import Path
import argparse, hashlib, json, re, struct, subprocess, zipfile

def run(command):
    result=subprocess.run([str(x) for x in command],capture_output=True,text=True,encoding='utf-8',errors='replace')
    if result.returncode: raise RuntimeError(result.stdout+'\n'+result.stderr)
    return result.stdout+result.stderr

def inspect(apk,sdk,jdk,output):
    checks=[]
    def require(condition,description):
        if not condition: raise RuntimeError(description)
        checks.append(description)
    tools=sdk/'build-tools/36.0.0'
    badging=run([tools/'aapt2.exe','dump','badging',apk])
    manifest=run([tools/'aapt2.exe','dump','xmltree',apk,'--file','AndroidManifest.xml'])
    resources=run([tools/'aapt2.exe','dump','resources',apk])
    certs=run([jdk/'bin/java.exe','-jar',tools/'lib/apksigner.jar','verify','--verbose','--print-certs',apk])
    alignment=run([tools/'zipalign.exe','-c','-P','16','-v','4',apk])
    require("package: name='com.celestialmaker.game'" in badging,'Expected application identifier')
    require("versionName='2.8.2'" in badging and "versionCode='28201'" in badging,'Version 2.8.2 / 28201')
    require(re.search(r"^(?:minSdkVersion|sdkVersion):'26'$",badging,re.M) is not None,'Minimum Android API 26')
    require("targetSdkVersion:'37'" in badging,'Target Android API 37')
    require("native-code: 'arm64-v8a'" in badging,'ARM64 application')
    require('application-debuggable' not in badging,'Development instrumentation disabled')
    require('Verified using v2 scheme (APK Signature Scheme v2): true' in certs,'APK v2 signature verified')
    require('Verification successful' in alignment,'APK 16 KB ZIP alignment verified')
    permissions=re.findall(r"uses-permission(?:-sdk-\d+)?: name='([^']+)'",badging)
    forbidden={'com.android.vending.BILLING','android.permission.MANAGE_EXTERNAL_STORAGE','android.permission.READ_CONTACTS','android.permission.ACCESS_FINE_LOCATION','android.permission.RECORD_AUDIO','android.permission.CAMERA'}
    require(not forbidden.intersection(permissions),'No billing or unrelated sensitive permissions')
    native=[]
    with zipfile.ZipFile(apk) as archive:
        require(archive.testzip() is None,'All ZIP payload CRCs valid')
        entries=archive.namelist()
        require(not any(re.search(r'(?i)(stripe|play-billing|billingclient|\.keystore$|\.jks$|\.env$|private[_-]?key)',entry) for entry in entries),'No payment SDK or signing secrets packaged')
        libraries=[entry for entry in entries if entry.startswith('lib/') and entry.endswith('.so')]
        require(bool(libraries),'Native libraries present')
        require(all(entry.startswith('lib/arm64-v8a/') for entry in libraries),'No non-ARM64 native payload')
        for entry in libraries:
            data=archive.read(entry)
            require(data[:5]==b'\x7fELF\x02' and data[5]==1 and struct.unpack_from('<H',data,18)[0]==183,'AArch64 ELF: '+entry)
            phoff=struct.unpack_from('<Q',data,32)[0]
            phsize,phcount=struct.unpack_from('<HH',data,54)
            loads=[]
            for i in range(phcount):
                p_type,flags,offset,vaddr,paddr,filesz,memsz,align=struct.unpack_from('<IIQQQQQQ',data,phoff+i*phsize)
                if p_type==1: loads.append({'offset':offset,'virtualAddress':vaddr,'alignment':align})
            require(bool(loads) and all(p['alignment']>=16384 and (p['offset']-p['virtualAddress'])%16384==0 for p in loads),'16 KB ELF LOAD alignment: '+entry)
            info=archive.getinfo(entry)
            native.append({'path':entry,'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest(),'storedUncompressed':info.compress_type==zipfile.ZIP_STORED,'loadSegments':loads})
        icon_paths=re.findall(r"application-icon-\d+:'([^']+)'",badging)
        require(bool(icon_paths) and all(p in entries for p in icon_paths),'Launcher icons packaged')
        icon_blocks=re.findall(r'(?m)^\s*resource [^\n]+ mipmap/(app_icon|ic_launcher_foreground)\n((?:[ \t]+\([^\n]+\n)+)',resources)
        rasters=[]
        for name,block in icon_blocks:
            rasters.extend((name,p) for p in re.findall(r'\(file\) (\S+) type=PNG',block))
        require(bool(rasters),'Launcher raster resources resolve through the resource table')
        for name,p in rasters:
            data=archive.read(p);require(data.startswith(b'\x89PNG\r\n\x1a\n'),'Valid launcher PNG: '+name)
            (output/('launcher-icon.png' if name=='app_icon' else 'launcher-foreground.png')).write_bytes(data)
    digest=hashlib.sha256(apk.read_bytes()).hexdigest().upper()
    for name,text in [('badging.txt',badging),('manifest.txt',manifest),('resources.txt',resources),('signature.txt',certs),('zipalign.txt',alignment)]:
        (output/name).write_text(text,encoding='utf-8')
    result={'pass':True,'checks':checks,'version':'2.8.2','versionCode':28201,'package':'com.celestialmaker.game','minSdk':26,'targetSdk':37,'abi':['arm64-v8a'],'bytes':apk.stat().st_size,'sha256':digest,'permissions':permissions,'nativeLibraries':native,'signature':'Unity development signing identity; not production-signed','physicalDevice':'UNVERIFIED','desktopControlled':False,'visibleGameWindow':False}
    (output/'apk-verification.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'pass':True,'checks':len(checks),'bytes':result['bytes'],'sha256':digest,'permissions':permissions}))

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--apk',required=True,type=Path);parser.add_argument('--sdk',required=True,type=Path);parser.add_argument('--jdk',required=True,type=Path);parser.add_argument('--output',required=True,type=Path)
    args=parser.parse_args();args.output.mkdir(parents=True,exist_ok=True)
    inspect(args.apk.resolve(),args.sdk.resolve(),args.jdk.resolve(),args.output.resolve())
