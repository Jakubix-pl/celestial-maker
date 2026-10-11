using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using TMPro;

namespace CelestialMaker.EditorTools {
 public static class PcUiAssets {
  const string Base="Assets/_CelestialMaker/";
  const string Dest=Base+"Resources/UI/PC/";
  [MenuItem("Tools/Celestial Maker/Prepare PC Visuals")]
  public static void Prepare(){
   if(!AssetDatabase.IsValidFolder(Dest.TrimEnd('/')))AssetDatabase.CreateFolder(Base+"Resources/UI","PC");
   string art=Path.Combine(ProjectBootstrap.Root,"Design/generated.png");
   if(!File.Exists(Dest+"CosmicBackdrop.png"))File.Copy(art,Dest+"CosmicBackdrop.png");
   AssetDatabase.Refresh();var importer=(TextureImporter)AssetImporter.GetAtPath(Dest+"CosmicBackdrop.png");importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
   CreateFont("Rajdhani-Bold","DisplayFont");CreateFont("IBMPlexSans-Regular","BodyFont");CreateFont("Rajdhani-Bold","DisplayFontHD",96,2048);CreateFont("IBMPlexSans-Regular","BodyFontHD",96,2048);
   foreach(string name in new[]{"ObservatoryNebula","ObservatoryPortrait","CelestialIcon"}){var image=(TextureImporter)AssetImporter.GetAtPath(Dest+name+".png");image.textureType=TextureImporterType.Default;image.mipmapEnabled=true;image.maxTextureSize=4096;image.npotScale=TextureImporterNPOTScale.None;image.alphaIsTransparency=name=="CelestialIcon";image.textureCompression=TextureImporterCompression.Uncompressed;image.filterMode=FilterMode.Trilinear;image.SaveAndReimport();}
   var icon=AssetDatabase.LoadAssetAtPath<Texture2D>(Dest+"CelestialIcon.png");PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone,Enumerable.Repeat(icon,PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Standalone).Length).ToArray());
   var go=new GameObject("Cosmic Panel",typeof(RectTransform),typeof(CosmicPanel));go.GetComponent<CosmicPanel>().color=CosmicStyle.Panel;PrefabUtility.SaveAsPrefabAsset(go,Dest+"Panel.prefab");UnityEngine.Object.DestroyImmediate(go);
   PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
   AssetDatabase.SaveAssets();File.WriteAllText(Path.Combine(ProjectBootstrap.Root,"Logs/UIOverhaul/assets.json"),"{\"status\":\"PASS\",\"unity\":\""+Application.unityVersion+"\",\"fonts\":2,\"art\":\"Pencil generated.png\",\"prefab\":\"UI/PC/Panel\"}");
  }
  static void CreateFont(string source,string name,int pointSize=48,int atlasSize=1024){
   if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Dest+name+".asset"))return;
   var font=AssetDatabase.LoadAssetAtPath<Font>(Base+"Art/Fonts/"+source+".ttf");if(!font)throw new Exception("Missing font "+source);
   var asset=TMP_FontAsset.CreateFontAsset(font,pointSize,10,GlyphRenderMode.SDFAA,atlasSize,atlasSize,AtlasPopulationMode.Dynamic,true);asset.name=name;AssetDatabase.CreateAsset(asset,Dest+name+".asset");
   asset.TryAddCharacters(string.Concat(Enumerable.Range(32,95).Select(i=>(char)i))+"×—–…→−•ąćęłńóśźżĄĆĘŁŃÓŚŹŻ",out string missing);
   foreach(var texture in asset.atlasTextures)if(texture)AssetDatabase.AddObjectToAsset(texture,asset);AssetDatabase.AddObjectToAsset(asset.material,asset);EditorUtility.SetDirty(asset);
  }
  public static void Build(){
   bool release=Environment.GetCommandLineArgs().Contains("-cmRelease"),expansion=Environment.GetCommandLineArgs().Contains("-cmExpansion"),observatory=Environment.GetCommandLineArgs().Contains("-cmObservatory");Prepare();if(expansion||observatory){PlayerSettings.bundleVersion=observatory?GameUI.ObservatoryVersion:Core.ProgressionBalance.Version;AssetDatabase.SaveAssets();}string destination=Path.Combine(ProjectBootstrap.Root,"Builds/Windows/"+(observatory?"Observatory-Release":expansion?"Expansion-Release":release?"PC-UI-Release":"UIOverhaul")+"/CelestialMaker.exe");Directory.CreateDirectory(Path.GetDirectoryName(destination));
   string config=Base+"Resources/platform-config.json";string previous=File.ReadAllText(config);File.WriteAllText(config,"{\"backendUrl\":\"\"}");AssetDatabase.ImportAsset(config);BuildReport report;try{report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),target=BuildTarget.StandaloneWindows64,locationPathName=destination,options=release?BuildOptions.None:BuildOptions.Development});}finally{File.WriteAllText(config,previous);AssetDatabase.ImportAsset(config);}
   File.WriteAllText(Path.Combine(ProjectBootstrap.Root,"Logs/"+(observatory?"Observatory":expansion?"Expansion":"UIOverhaul")+"/"+(release?"build-release.json":"build.json")),JsonUtility.ToJson(new Result{result=report.summary.result.ToString(),errors=report.summary.totalErrors,seconds=report.summary.totalTime.TotalSeconds,path=destination,unity=Application.unityVersion,utc=DateTime.UtcNow.ToString("o")},true));if(report.summary.result!=BuildResult.Succeeded)throw new Exception("PC UI build failed");
  }
  [Serializable]class Result{public string result,path,unity,utc;public int errors;public double seconds;}
 }
}
