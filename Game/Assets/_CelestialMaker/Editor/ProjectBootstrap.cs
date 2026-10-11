using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace CelestialMaker.EditorTools {
 public static class ProjectBootstrap {
  const string Base="Assets/_CelestialMaker/";
  public static string Root=>Directory.GetParent(Application.dataPath).Parent.FullName;
  [MenuItem("Tools/Celestial Maker/Setup Project")]
  public static void Run(){
   EditorSettings.serializationMode=SerializationMode.ForceText;EditorSettings.externalVersionControl="Visible Meta Files";
   PlayerSettings.companyName="CelestialMaker";PlayerSettings.productName="Celestial Maker";PlayerSettings.bundleVersion="1.0.0";
   PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.resizableWindow=true;
   PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.celestialmaker.game");
   PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone,"com.celestialmaker.game");
   if(!AssetDatabase.LoadAssetAtPath<Mesh>(Base+"Resources/body-mesh.asset")){var temporary=GameObject.CreatePrimitive(PrimitiveType.Sphere);var mesh=UnityEngine.Object.Instantiate(temporary.GetComponent<MeshFilter>().sharedMesh);UnityEngine.Object.DestroyImmediate(temporary);AssetDatabase.CreateAsset(mesh,Base+"Resources/body-mesh.asset");}
   var font=LoadFont();if(font==null)throw new Exception("TMP essential resources must be imported first");
   if(!AssetDatabase.IsValidFolder(Base+"Resources/UI"))AssetDatabase.CreateFolder(Base+"Resources","UI");
   foreach(string element in new[]{"Panel","GeneratorCard","Tab","Modal","Settings","OfflineDialog","Tooltip"}){string p=Base+"Resources/UI/"+element+".prefab";if(File.Exists(p))continue;var obj=new GameObject(element,typeof(RectTransform),typeof(Image));obj.GetComponent<Image>().color=new Color(.08f,.11f,.19f,.9f);PrefabUtility.SaveAsPrefabAsset(obj,p);UnityEngine.Object.DestroyImmediate(obj);}
   if(!File.Exists(Base+"Resources/UI/Text.prefab")){var obj=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));var text=obj.GetComponent<TMP_Text>();text.font=font;text.fontSize=18;text.color=Color.white;text.raycastTarget=false;PrefabUtility.SaveAsPrefabAsset(obj,Base+"Resources/UI/Text.prefab");UnityEngine.Object.DestroyImmediate(obj);}
   var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>(Base+"Settings/Renderer.asset");if(!renderer){renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,Base+"Settings/Renderer.asset");}
   var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Base+"Settings/Desktop.asset");if(!rp){rp=UniversalRenderPipelineAsset.Create(renderer);rp.msaaSampleCount=2;rp.supportsHDR=false;rp.renderScale=1;AssetDatabase.CreateAsset(rp,Base+"Settings/Desktop.asset");}
   var mobile=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Base+"Settings/Mobile.asset");if(!mobile){mobile=UniversalRenderPipelineAsset.Create(renderer);mobile.msaaSampleCount=1;mobile.supportsHDR=false;mobile.renderScale=.8f;AssetDatabase.CreateAsset(mobile,Base+"Settings/Mobile.asset");}
   GraphicsSettings.defaultRenderPipeline=rp;for(int i=0;i<QualitySettings.names.Length;i++){QualitySettings.SetQualityLevel(i);QualitySettings.renderPipeline=i==0?mobile:rp;}QualitySettings.SetQualityLevel(Math.Min(1,QualitySettings.names.Length-1));
   if(!AssetDatabase.LoadAssetAtPath<Texture2D>(Base+"Resources/planet-surface.asset")){var tex=new Texture2D(64,32,TextureFormat.RGBA32,false);tex.name="Original banded planet";for(int y=0;y<32;y++)for(int x=0;x<64;x++){float band=.55f+.25f*Mathf.Sin(y*.65f+Mathf.Sin(x*.3f));tex.SetPixel(x,y,Color.Lerp(new Color(.12f,.28f,.5f),new Color(.8f,.68f,.38f),band));}tex.Apply();AssetDatabase.CreateAsset(tex,Base+"Resources/planet-surface.asset");}
   var material=AssetDatabase.LoadAssetAtPath<Material>(Base+"Art/Cosmos.mat");if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.SetColor("_BaseColor",new Color(.3f,.8f,.8f));AssetDatabase.CreateAsset(material,Base+"Art/Cosmos.mat");}
   if(material.shader.name!="Universal Render Pipeline/Lit"){material.shader=Shader.Find("Universal Render Pipeline/Lit");EditorUtility.SetDirty(material);}
   string[] names={"00_Bootstrap","01_MainMenu","02_Game"};foreach(string name in names){string path=Base+"Scenes/"+name+".unity";if(File.Exists(path))continue;var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    if(name=="00_Bootstrap"){var go=new GameObject("GameSession");go.AddComponent<GameSession>();SavePrefab(go,"GameRoot");}
    else{var cam=new GameObject("CameraRig").AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.025f,.035f,.075f);cam.transform.position=new Vector3(0,0,-18);cam.orthographic=true;cam.orthographicSize=7.5f;SavePrefab(cam.gameObject,"CameraRig");
     var ev=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));ev.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
     var canvas=new GameObject(name=="01_MainMenu"?"MainMenu":"HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.matchWidthOrHeight=.5f;var ui=canvas.AddComponent<GameUI>();ui.font=font;ui.menu=name=="01_MainMenu";SavePrefab(canvas,ui.menu?"MainMenu":"HUD");
     if(name=="02_Game"){var cosmos=new GameObject("Cosmos").AddComponent<Cosmos>();cosmos.material=material;SavePrefab(cosmos.gameObject,"Cosmos");}
    }EditorSceneManager.SaveScene(scene,path);
   }
   EditorBuildSettings.scenes=Array.ConvertAll(names,n=>new EditorBuildSettingsScene(Base+"Scenes/"+n+".unity",true));
   if(!File.Exists(Base+"Settings/Controls.inputactions")){var actions=ScriptableObject.CreateInstance<InputActionAsset>();var map=actions.AddActionMap("Game");map.AddAction("Pointer",InputActionType.Value,"<Pointer>/position");map.AddAction("Select",InputActionType.Button,"<Pointer>/press");map.AddAction("Back",InputActionType.Button,"<Keyboard>/escape");File.WriteAllText(Base+"Settings/Controls.inputactions",actions.ToJson());}
   AssetDatabase.SaveAssets();AssetDatabase.Refresh();File.WriteAllText(Path.Combine(Root,"Logs/bootstrap-result.json"),"{\"status\":\"PASS\",\"scenes\":3,\"unity\":\""+Application.unityVersion+"\"}");Debug.Log("CELESTIAL BOOTSTRAP PASS");
  }
  static TMP_FontAsset LoadFont(){var ids=AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");return ids.Length>0?AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(ids[0])):null;}
  static void SavePrefab(GameObject go,string name){string path=Base+"Prefabs/"+name+".prefab";if(!File.Exists(path))PrefabUtility.SaveAsPrefabAsset(go,path);}
  public static void ImportTMP(){if(LoadFont()!=null){EditorApplication.Exit(0);return;}string pkg=UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(TMP_Text).Assembly).resolvedPath;AssetDatabase.importPackageCompleted+=n=>EditorApplication.Exit(0);AssetDatabase.importPackageFailed+=(n,e)=>{Debug.LogError(e);EditorApplication.Exit(1);};AssetDatabase.ImportPackage(Path.Combine(pkg,"Package Resources/TMP Essential Resources.unitypackage"),false);}
 }
 [InitializeOnLoad] static class PlayFromBootstrap {static PlayFromBootstrap(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-runTests")>=0)return;EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.ExitingEditMode){string path="Assets/_CelestialMaker/Scenes/00_Bootstrap.unity";if(File.Exists(path))EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(path);}};}}
}



