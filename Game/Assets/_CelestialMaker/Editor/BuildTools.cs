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
PrepareAndroidIcons();
#if UNITY_ANDROID
UnityEditor.Android.AndroidExternalToolsSettings.sdkRootPath=Environment.GetEnvironmentVariable("CM_ANDROID_SDK");UnityEditor.Android.AndroidExternalToolsSettings.ndkRootPath=Environment.GetEnvironmentVariable("CM_ANDROID_NDK");UnityEditor.Android.AndroidExternalToolsSettings.jdkRootPath=Environment.GetEnvironmentVariable("CM_ANDROID_JDK");
#endif
bool test=Args.Contains("-cmTestAAB"),sideload=Args.Contains("-cmSideload");PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)37;PlayerSettings.Android.bundleVersionCode=28101;PlayerSettings.bundleVersion=GameUI.ObservatoryVersion;PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToPortrait=true;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.Android.renderOutsideSafeArea=true;PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;EditorUserBuildSettings.buildAppBundle=Args.Contains("-cmRelease")||test;Build(BuildTarget.Android,sideload?"Android/CelestialMaker-2.8.1-Android-arm64.apk":test?"Android/CelestialMaker-test.aab":EditorUserBuildSettings.buildAppBundle?"Android/CelestialMaker.aab":"Android/CelestialMaker.apk");}
  static void PrepareAndroidIcons(){
   const string sourcePath="Assets/_CelestialMaker/Resources/UI/PC/CelestialIcon.png",folder="Assets/_CelestialMaker/Art/AndroidIcons";
   if(!AssetDatabase.IsValidFolder("Assets/_CelestialMaker/Art"))AssetDatabase.CreateFolder("Assets/_CelestialMaker","Art");
   if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/_CelestialMaker/Art","AndroidIcons");
   var source=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!source.LoadImage(File.ReadAllBytes(sourcePath)))throw new Exception("Celestial icon missing");
   var foreground=new Texture2D(512,512,TextureFormat.RGBA32,false);var pixels=new Color[512*512];
   for(int y=0;y<512;y++)for(int x=0;x<512;x++){float u=((x+.5f)/512-.5f)/.66f+.5f,v=((y+.5f)/512-.5f)/.66f+.5f;pixels[y*512+x]=u>=0&&u<=1&&v>=0&&v<=1?source.GetPixelBilinear(u,v):Color.clear;}
   foreground.SetPixels(pixels);foreground.Apply();var background=new Texture2D(64,64,TextureFormat.RGBA32,false);background.SetPixels(Enumerable.Repeat(new Color(13/255f,20/255f,34/255f),64*64).ToArray());background.Apply();
   foreach(var pair in new[]{(name:"Foreground",texture:foreground),(name:"Background",texture:background)}){string path=folder+"/"+pair.name+".png";File.WriteAllBytes(path,pair.texture.EncodeToPNG());AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.alphaIsTransparency=pair.name=="Foreground";importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
   UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(foreground);UnityEngine.Object.DestroyImmediate(background);
   var legacy=AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);var front=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/Foreground.png");var back=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/Background.png");
   PlayerSettings.SetIcons(NamedBuildTarget.Android,Enumerable.Repeat(legacy,PlayerSettings.GetIconSizes(NamedBuildTarget.Android,IconKind.Application).Length).ToArray(),IconKind.Application);
   var kinds=PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android);if(kinds.Length==0)throw new Exception("Android icon API requires active Android target");
   foreach(var kind in kinds){var icons=PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android,kind);foreach(var icon in icons){if(icon.minLayerCount==1)icon.SetTexture(legacy);else{var labels=(string[])typeof(PlatformIconKind).GetProperty("customLayerLabels",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(kind);if(labels==null||labels.Length<icon.minLayerCount)throw new Exception("Unknown Android icon layer schema");var layers=new Texture2D[icon.minLayerCount];for(int layer=0;layer<layers.Length;layer++){string label=labels[layer].ToLowerInvariant();layers[layer]=label.Contains("foreground")?front:label.Contains("background")?back:throw new Exception("Unknown Android icon layer: "+label);}icon.SetTextures(layers);}}PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android,kind,icons);}
   AssetDatabase.SaveAssets();Debug.Log("CELESTIAL ANDROID ICONS ASSIGNED / "+kinds.Length+" kinds");
  }
#if UNITY_ANDROID
  public sealed class AndroidLegacyIconPostprocessor:UnityEditor.Android.IPostGenerateGradleAndroidProject {
   public int callbackOrder=>100;
   public void OnPostGenerateGradleAndroidProject(string path){
    var source=new Texture2D(2,2,TextureFormat.RGBA32,false);source.LoadImage(File.ReadAllBytes("Assets/_CelestialMaker/Resources/UI/PC/CelestialIcon.png"));int replaced=0;
    try{foreach(string directory in new[]{Path.Combine(path,"src/main/res"),Path.Combine(Path.GetDirectoryName(path),"launcher/src/main/res")}){if(!Directory.Exists(directory))continue;foreach(string file in Directory.GetFiles(directory,"app_icon.png",SearchOption.AllDirectories)){var original=new Texture2D(2,2,TextureFormat.RGBA32,false);original.LoadImage(File.ReadAllBytes(file));int width=original.width,height=original.height;UnityEngine.Object.DestroyImmediate(original);var target=new Texture2D(width,height,TextureFormat.RGBA32,false);var pixels=new Color[width*height];for(int y=0;y<height;y++)for(int x=0;x<width;x++)pixels[y*width+x]=source.GetPixelBilinear((x+.5f)/width,(y+.5f)/height);target.SetPixels(pixels);target.Apply();File.WriteAllBytes(file,target.EncodeToPNG());UnityEngine.Object.DestroyImmediate(target);replaced++;}}}finally{UnityEngine.Object.DestroyImmediate(source);}
    if(replaced==0)throw new Exception("Generated Android legacy icon resource missing");Debug.Log("CELESTIAL ANDROID LEGACY ICON RESOURCES / "+replaced);
   }
  }
#endif
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
