using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace CelestialMaker.EditorTools {
 public static class BuildTools {
  [Serializable]class Configuration {public string target,backend,quality,container,architecture,compression;public bool development,requiresProductionSigning;}
  [Serializable]class Profiles {public Configuration WindowsDevelopment,WindowsRelease,AndroidAPKDevelopment,AndroidAPKSideload,AndroidAABRelease,AndroidAABTest,WebDevelopment,WebRelease;}
  public static void Windows(){bool release=Args.Contains("-cmRelease");Build(BuildTarget.StandaloneWindows64,release?"Windows/Release/CelestialMaker.exe":"Windows/Development/CelestialMaker.exe");}
  static string[] Args=>Environment.GetCommandLineArgs();
  public static void Web(){PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=false;Build(BuildTarget.WebGL,"Web");}
  public static void Android(){
#if UNITY_ANDROID
UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath=Environment.GetEnvironmentVariable("CM_ANDROID_SDK");UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath=Environment.GetEnvironmentVariable("CM_ANDROID_NDK");UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath=Environment.GetEnvironmentVariable("CM_ANDROID_JDK");
#endif
bool test=Args.Contains("-cmTestAAB"),sideload=Args.Contains("-cmSideload");PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)37;PlayerSettings.Android.bundleVersionCode=27001;PlayerSettings.bundleVersion=GameUI.ObservatoryVersion;PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.Android.renderOutsideSafeArea=true;PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;EditorUserBuildSettings.buildAppBundle=Args.Contains("-cmRelease")||test;Build(BuildTarget.Android,sideload?"Android/CelestialMaker-2.7.0-Android-arm64.apk":test?"Android/CelestialMaker-test.aab":EditorUserBuildSettings.buildAppBundle?"Android/CelestialMaker.aab":"Android/CelestialMaker.apk");}
  public static string GameSourceHash(){
   string game=Directory.GetParent(Application.dataPath).FullName;
   var files=Directory.GetFiles(Application.dataPath,"*",SearchOption.AllDirectories).Concat(Directory.GetFiles(Path.Combine(game,"ProjectSettings"),"*",SearchOption.AllDirectories)).Concat(new[]{Path.Combine(game,"Packages/manifest.json"),Path.Combine(game,"Packages/packages-lock.json")}).OrderBy(x=>x,StringComparer.Ordinal);
   var list=new StringBuilder();using(var sha=SHA256.Create()){foreach(string file in files)list.Append(file.Substring(game.Length).Replace('\\','/')).Append(':').Append(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(file))).Replace("-","")).Append('\n');return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(list.ToString()))).Replace("-","").ToLowerInvariant();}
  }
  static void Build(BuildTarget target,string relative){
   if(!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target),target))throw new Exception("BLOCKED: build module missing for "+target);
   PlayerSettings.enableFrameTimingStats=true;bool release=Args.Contains("-cmRelease"),test=Args.Contains("-cmTestAAB"),sideload=Args.Contains("-cmSideload");string name=target==BuildTarget.StandaloneWindows64?(release?"WindowsRelease":"WindowsDevelopment"):target==BuildTarget.WebGL?(release?"WebRelease":"WebDevelopment"):sideload?"AndroidAPKSideload":test?"AndroidAABTest":release?"AndroidAABRelease":"AndroidAPKDevelopment";
   var profiles=JsonUtility.FromJson<Profiles>(File.ReadAllText(Path.Combine(ProjectBootstrap.Root,"Tools/build-profiles.json")));
   var profile=target==BuildTarget.StandaloneWindows64?(release?profiles.WindowsRelease:profiles.WindowsDevelopment):target==BuildTarget.WebGL?(release?profiles.WebRelease:profiles.WebDevelopment):sideload?profiles.AndroidAPKSideload:test?profiles.AndroidAABTest:release?profiles.AndroidAABRelease:profiles.AndroidAPKDevelopment;
   PlayerSettings.SetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(target)),profile.backend=="IL2CPP"?ScriptingImplementation.IL2CPP:ScriptingImplementation.Mono2x);QualitySettings.SetQualityLevel(profile.quality=="Mobile"?0:1);
   if(target==BuildTarget.Android){
    if(release){string key=Environment.GetEnvironmentVariable("CM_ANDROID_KEYSTORE"),alias=Environment.GetEnvironmentVariable("CM_ANDROID_KEYALIAS"),keypass=Environment.GetEnvironmentVariable("CM_ANDROID_KEYSTORE_PASS"),aliaspass=Environment.GetEnvironmentVariable("CM_ANDROID_KEYALIAS_PASS");if(string.IsNullOrEmpty(key)||!File.Exists(key)||string.IsNullOrEmpty(alias)||string.IsNullOrEmpty(keypass)||string.IsNullOrEmpty(aliaspass))throw new Exception("BLOCKED: production signing environment is incomplete");PlayerSettings.Android.useCustomKeystore=true;PlayerSettings.Android.keystoreName=key;PlayerSettings.Android.keyaliasName=alias;PlayerSettings.Android.keystorePass=keypass;PlayerSettings.Android.keyaliasPass=aliaspass;}
    else PlayerSettings.Android.useCustomKeystore=false;
   }
   string config=Path.Combine(Application.dataPath,"_CelestialMaker/Resources/platform-config.json"),previous=File.ReadAllText(config);
   string backend=Environment.GetEnvironmentVariable("CM_BACKEND_URL")??(release||sideload?"":"http://localhost:4311");
   if(backend!=""&&(!Uri.TryCreate(backend,UriKind.Absolute,out var uri)||uri.Scheme!="https"&&!(profile.development&&uri.Scheme=="http")))throw new Exception("Invalid backend URL for this build profile");
   string dest=Path.Combine(ProjectBootstrap.Root,"Builds",relative);Directory.CreateDirectory(Path.GetDirectoryName(dest));
   try{
    File.WriteAllText(config,JsonUtility.ToJson(new Platform{backendUrl=backend}));AssetDatabase.ImportAsset("Assets/_CelestialMaker/Resources/platform-config.json");string hash=GameSourceHash();
    var options=new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>x.path).ToArray(),locationPathName=dest,target=target,options=profile.development?BuildOptions.Development:BuildOptions.None};var report=BuildPipeline.BuildPlayer(options);
    File.WriteAllText(Path.Combine(ProjectBootstrap.Root,"Logs/build-"+target+(sideload?"-Sideload":test?"-TestBundle":release?"-Release":"-Development")+".json"),JsonUtility.ToJson(new Report{result=report.summary.result.ToString(),errors=report.summary.totalErrors,size=report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,profile=name,target=target.ToString(),unity=Application.unityVersion,utc=DateTime.UtcNow.ToString("o"),sourceHash=hash,path=dest,sourceState="local uncommitted work; not a clean commit"},true));
    if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);Debug.Log("CELESTIAL BUILD PASS "+dest);
   }finally{File.WriteAllText(config,previous);AssetDatabase.ImportAsset("Assets/_CelestialMaker/Resources/platform-config.json");}
  }
  [Serializable]class Platform{public string backendUrl;}
  [Serializable]class Report {public string result,profile,target,unity,utc,sourceHash,path,sourceState;public int errors;public ulong size;public double seconds;}
 }
}
