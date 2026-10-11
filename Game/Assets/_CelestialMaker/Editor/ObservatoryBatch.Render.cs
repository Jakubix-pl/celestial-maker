using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using CelestialMaker.Core;

namespace CelestialMaker.EditorTools {
 public static partial class ObservatoryBatch {
  static Camera renderCamera;
  static string renderDir;
  static string renderedPipeline;
  static readonly List<string> rendered=new List<string>();
  // Renders the real uGUI hierarchy to a private GPU texture. No desktop capture,
  // Play Mode, ScreenCapture, OS input, player window, or production save access.
  public static void RenderReview(){
   if(!Application.isBatchMode||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new Exception("Requires batch mode with an offscreen graphics device.");
   var args=Environment.GetCommandLineArgs();int fixtureIndex=Array.IndexOf(args,"-cmLateFixture");renderLateFixture=fixtureIndex>=0?args[fixtureIndex+1]:null;
   renderDir=Path.Combine(ProjectBootstrap.Root,"Logs/ObservatoryVisual",Environment.GetCommandLineArgs().Contains("-cmAfter")?"after":"before");Directory.CreateDirectory(renderDir);rendered.Clear();
   int previousQuality=QualitySettings.GetQualityLevel();QualitySettings.SetQualityLevel(Math.Min(1,QualitySettings.names.Length-1));
   var previousCulture=System.Globalization.CultureInfo.CurrentCulture;var previousUiCulture=System.Globalization.CultureInfo.CurrentUICulture;System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.GetCultureInfo("en-US");System.Globalization.CultureInfo.CurrentUICulture=System.Globalization.CultureInfo.GetCultureInfo("en-US");
   GameObject holder=null,hud=null,events=null,cameraObject=null;
   try{
    holder=new GameObject("Isolated visual session");holder.SetActive(false);session=holder.AddComponent<GameSession>();GameSession.Instance=session;GameSession.Content=JsonUtility.FromJson<Content>(Resources.Load<TextAsset>("content").text);SaveStore.OverridePath=Path.Combine(renderDir,"fixture-save.json");
    events=new GameObject("Visual EventSystem",typeof(EventSystem));if(!EventSystem.current)typeof(EventSystem).GetMethod("OnEnable",Private).Invoke(events.GetComponent<EventSystem>(),null);
    hud=new GameObject("Offscreen UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=(RectTransform)hud.transform;hud.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;hud.GetComponent<CanvasScaler>().enabled=false;ui=hud.AddComponent<GameUI>();ui.font=Resources.Load<TMP_FontAsset>("UI/PC/BodyFont");
    cameraObject=new GameObject("Private render camera",typeof(Camera));renderCamera=cameraObject.GetComponent<Camera>();renderCamera.enabled=false;renderCamera.orthographic=true;renderCamera.transform.position=new Vector3(0,0,-100);renderCamera.clearFlags=CameraClearFlags.SolidColor;renderCamera.nearClipPlane=.1f;renderCamera.farClipPlane=200;renderCamera.cullingMask=1<<31;renderCamera.allowHDR=false;renderCamera.allowMSAA=false;renderCamera.GetUniversalAdditionalCameraData().renderPostProcessing=false;hud.GetComponent<Canvas>().worldCamera=renderCamera;
    bool probe=Environment.GetCommandLineArgs().Contains("-cmRenderProbe");
    if(Environment.GetCommandLineArgs().Contains("-cmPhoneUi"))RenderPhone();else if(Environment.GetCommandLineArgs().Contains("-cmPublication"))RenderPublication();else if(Environment.GetCommandLineArgs().Contains("-cmDiscoveryOnly"))RenderDiscovery();else if(Environment.GetCommandLineArgs().Contains("-cmCadenceOnly"))RenderCadence();else if(Environment.GetCommandLineArgs().Contains("-cmEvolutionOnly"))RenderEvolution();else if(Environment.GetCommandLineArgs().Contains("-cmUnlocksOnly"))RenderUnlocks();else if(Environment.GetCommandLineArgs().Contains("-cmResourcesOnly"))RenderResources();else if(Environment.GetCommandLineArgs().Contains("-cmTabsOnly"))RenderTabs();else if(Environment.GetCommandLineArgs().Contains("-cmReadabilityOnly"))RenderReadability();else{
    Fixture(false,1,false,1920,1080);Shot("early-night-1920-production");ui.menu=true;Call("BuildPc");Shot("early-night-1920-menu");
    Fixture(true,2,false,1280,720);Shot("late-night-1280-200-production");
    if(!probe){RenderScreens();RenderMotion();RenderReadability();RenderEvolution();}}
   }finally{
    if(hud)UnityEngine.Object.DestroyImmediate(hud);if(holder)UnityEngine.Object.DestroyImmediate(holder);if(cameraObject)UnityEngine.Object.DestroyImmediate(cameraObject);
    if(events){if(EventSystem.current==events.GetComponent<EventSystem>())typeof(EventSystem).GetMethod("OnDisable",Private).Invoke(events.GetComponent<EventSystem>(),null);UnityEngine.Object.DestroyImmediate(events);}GameSession.Instance=null;SaveStore.OverridePath=null;QualitySettings.SetQualityLevel(previousQuality);System.Globalization.CultureInfo.CurrentCulture=previousCulture;System.Globalization.CultureInfo.CurrentUICulture=previousUiCulture;
   }
   File.WriteAllText(Path.Combine(renderDir,"render-manifest.json"),JsonUtility.ToJson(new RenderEvidence{files=rendered.ToArray(),utc=DateTime.UtcNow.ToString("o"),device=SystemInfo.graphicsDeviceName,pipeline=renderedPipeline},true));
  }
  static void RenderPublication(){
   RenderPurchases(1920,1080,"publication-");
   Fixture(false,1,false,1920,1080);Set("page","Production");Call("ShowPc");Shot("prestige-fresh-production");
   session.Sim.S.resources[0]=session.Sim.Cost(1)-session.Sim.Rate(0)*30;Call("ShowPc");Shot("prestige-30second-discovery");
   session.Sim.S.lifetime[0]=BreakInfinity.BigDouble.Pow10(ProgressionBalance.PrestigeEnergyLog);session.Sim.Rebirth(0);Call("BuildPc");Set("page","Production");Call("ShowPc");Shot("prestige-revealed-production");
   Set("page","Automation");Call("ShowPc");Shot("prestige-autobuyer-purchase");
   Call("AutobuyerGuidePc");Shot("autobuyer-guide-start");for(int step=0;step<5;step++){Click("Perform practice step");Shot("autobuyer-guide-step-"+(step+1));}Call("ClosePcModal");
   Fixture(false,1,false,1920,1080);ui.menu=true;Call("BuildPc");Shot("publication-menu");
   Fixture(true,1,false,1920,1080);
   foreach(string page in new[]{"Overview","Production","Research","Automation","Megastructures","Parallel universes","Challenges"}){Set("page",page);Call("ShowPc");Shot("publication-"+page.Replace(' ','-').ToLowerInvariant());if(page=="Overview"){var art=ui.GetComponentInChildren<ObservatoryCosmos>();for(int step=0;step<20;step++){art.AdvanceMotion(.5f,false);Shot("prestige-motion-"+step.ToString("00"));}}}
   Call("ModulesPc",2);Shot("publication-engineering");Call("ClosePcModal");
   Set("page","Automation");Call("ShowPc");Call("GoalSequencePc");Shot("publication-goals");Call("ClosePcModal");
   Call("ChroniclePc");Shot("publication-chronicle");Call("ClosePcModal");
   Fixture(true,1,true,1920,1080);Set("page","Overview");Call("ShowPc");Shot("publication-dawn");
  }
  static void RenderPurchases(int width,int height,string prefix){
   Fixture(false,1,false,width,height);session.Sim.S.resources[0]=new BreakInfinity.BigDouble(90);Set("page","Production");Call("ShowPc");Shot(prefix+"purchase-production");
   session.Sim.S.resources[0]=new BreakInfinity.BigDouble(200);session.Sim.Purchase(GameSession.Content.research[0]);session.Sim.S.resources[0]=new BreakInfinity.BigDouble(600);Set("page","Research");Call("ShowPc");Shot(prefix+"purchase-research");
  }
  static void RenderTabs(){
   foreach(bool light in new[]{false,true})foreach(float scale in new[]{1f,2f}){
    Fixture(true,scale,light,1920,1080);
    foreach(string page in new[]{"Production","Research","Prestige","Megastructures","Achievements"}){
     Set("page",page);Call("ShowPc");string key="tabs-"+(light?"dawn":"night")+"-"+(int)(scale*100)+"-"+page.ToLowerInvariant();Shot(key);
     if(scale==2){Call("SectionViewsPc");Shot(key+"-views");Call("ClosePcModal");}
    }
   }
   Fixture(true,1,false,1280,720);Set("page","Megastructures");Call("ShowPc");Shot("tabs-night-1280-cosmos-wrap");
   Fixture(true,2,true,960,720);Set("page","Megastructures");Call("ShowPc");Call("SectionViewsPc");Shot("tabs-dawn-960-200-views");
  }
  static void RenderMotion(){
   Fixture(false,1,false,1920,1080);var workspace=Get<RectTransform>("list");ObservatoryReveal.Begin(workspace,false);var reveal=workspace.GetComponent<ObservatoryReveal>();
   Shot("animation-reveal-000ms");reveal.Step(.035f,false);Shot("animation-reveal-035ms");reveal.Step(.035f,false);Shot("animation-reveal-070ms");reveal.Step(.07f,false);Shot("animation-reveal-140ms");
   ObservatoryReveal.Begin(workspace,true);Shot("animation-reduced-motion");
   for(int f=1;f<=60;f++){FixedStepClock.Advance(session.Sim,1d/60);Call("PresentPc");if(f==1||f==30||f==60)Shot("counter-frame-"+f.ToString("D2"));}
  }
  static void RenderScreens(){
   string[] pages={"Overview","Production","Research","Upgrades","Prestige","Trees","Constellations","Challenges","Expeditions","Achievements","Codex","Settings","Offline summary","Analytics","Automation","Roadmap","Megastructures","Reality laws","Parallel universes","Credits"};
   foreach(bool light in new[]{false,true}){
    Fixture(true,1,light,1920,1080);MakeOffline();
    foreach(string page in pages){Set("page",page);Set("offlineDetails",true);Call("ShowPc");Settle();string name="late-"+(light?"dawn":"night")+"-1920-"+page.Replace(' ','-').ToLowerInvariant();Shot(name);if(Get<ScrollRect>("pcScroll").content.rect.height>900){Shot(name+"-middle",.5f);Shot(name+"-bottom",0);}}
    Call("ResetPreviewPc",1,false);Shot("modal-reset-"+(light?"dawn":"night"));Call("ClosePcModal");
    Set("page","Overview");Call("ShowPc");foreach(string state in new[]{"Normal","Highlighted","Pressed","Selected","Disabled"}){foreach(var button in ui.GetComponentsInChildren<ObservatoryButton>())ButtonState(button,state);Shot("control-"+(light?"dawn":"night")+"-"+state.ToLowerInvariant());}
   }
   Fixture(false,1,false,1280,720);foreach(string page in pages){Set("page",page);Call("ShowPc");Shot("early-night-1280-"+page.Replace(' ','-').ToLowerInvariant());}
   Fixture(true,2,true,1280,720);foreach(string page in pages){Set("page",page);Call("ShowPc");Shot("large-dawn-1280-"+page.Replace(' ','-').ToLowerInvariant());Shot("large-dawn-1280-"+page.Replace(' ','-').ToLowerInvariant()+"-bottom",0);}
   Fixture(true,1,false,3440,1440);foreach(string page in new[]{"Production","Trees","Codex","Settings"}){Set("page",page);Call("ShowPc");Shot("wide-night-3440-"+page.ToLowerInvariant());}
   foreach(float scale in new[]{1f,2f}){
    Fixture(true,scale,false,1280,720);string key="modal-1280-"+Mathf.RoundToInt(scale*100)+"-";
    Call("ProcessDetailsPc",1);Shot(key+"process");Call("ClosePcModal");
    Call("ResetPreviewPc",2,false);Shot(key+"genesis");ModalBottom();Shot(key+"genesis-bottom");Call("ClosePcModal");
    Call("ChooseWorldPc");Shot(key+"economy");Call("ClosePcModal");
    Call("UniverseDetailsPc",1);Shot(key+"universe");Call("ClosePcModal");
    Call("StrategyTransferPc");Shot(key+"strategy");var strategy=Get<RectTransform>("pcModal").GetComponentInChildren<TMP_InputField>();strategy.text=JsonUtility.ToJson(session.Sim.Meta.strategies[0],true);Shot(key+"strategy-filled");ModalBottom();Shot(key+"strategy-bottom");Call("ClosePcModal");
    Call("ImportPc");Shot(key+"import");var input=Get<RectTransform>("pcModal").GetComponentInChildren<TMP_InputField>();input.text=JsonUtility.ToJson(session.Sim.S,true);Shot(key+"import-filled");ModalBottom();Shot(key+"import-bottom");Call("ClosePcModal");
    MakeOffline();Call("ShowOfflinePc");Shot(key+"offline");ModalBottom();Shot(key+"offline-bottom");Call("ClosePcModal");
    session.Saved=session.Sim.S;ui.menu=true;Call("BuildPc");Shot("menu-continue-1280-"+Mathf.RoundToInt(scale*100));Shot("menu-continue-1280-"+Mathf.RoundToInt(scale*100)+"-bottom",0);
   }
   Fixture(true,1.25f,false,2560,1440);foreach(string page in pages){Set("page",page);Call("ShowPc");Shot("qhd-125-"+page.Replace(' ','-').ToLowerInvariant());}
   Fixture(true,1,false,1920,1080);session.SaveFailed=true;session.Message="Save failed: test storage unavailable";Set("page","Settings");Call("ShowPc");Shot("save-failure-bottom",0);
   Fixture(true,1,false,1920,1080);session.Sim.EnterChallenge(0);Set("page","Challenges");Call("ShowPc");Shot("active-challenge");
  }
  static void ButtonState(ObservatoryButton button,string state){var method=typeof(ObservatoryButton).GetMethod("DoStateTransition",Private);method.Invoke(button,new[]{Enum.Parse(method.GetParameters()[0].ParameterType,state),(object)true});}
  static void MakeOffline(){session.Sim.S.offlineReport=null;session.Sim.BeginOffline(3600);while(session.Sim.S.offlineRemaining>0)session.Sim.ProcessOffline(128);}
  static void ModalBottom(){Settle();var modal=Get<RectTransform>("pcModal");foreach(var scroll in modal.GetComponentsInChildren<ScrollRect>())scroll.verticalNormalizedPosition=0;Settle();}
  static void Shot(string name,float position=1){
   var scroll=Get<ScrollRect>("pcScroll");if(scroll)scroll.verticalNormalizedPosition=position;Settle();
   foreach(var t in ui.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
   foreach(var sc in ui.GetComponentsInChildren<ScrollRect>())typeof(ScrollRect).GetMethod("LateUpdate",Private).Invoke(sc,null);
   foreach(var fit in ui.GetComponentsInChildren<ObservatoryArtworkFit>())fit.Fit();
   foreach(var graphic in ui.GetComponentsInChildren<Graphic>()){graphic.SetVerticesDirty();graphic.SetMaterialDirty();}
   foreach(var text in ui.GetComponentsInChildren<TMP_Text>())text.ForceMeshUpdate(true,true);Canvas.ForceUpdateCanvases();
   int width=Mathf.RoundToInt(canvas.rect.width),height=Mathf.RoundToInt(canvas.rect.height);renderCamera.orthographicSize=height/2f;renderCamera.aspect=(float)width/height;renderCamera.backgroundColor=CosmicStyle.Background;
   var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);target.Create();var previous=RenderTexture.active;Texture2D pixels=null;
   try{
    renderCamera.targetTexture=target;
    // Camera.Render initializes the configured SRP, including batch editor sessions.
    renderCamera.Render();
    renderedPipeline=(GraphicsSettings.currentRenderPipeline?.GetType().Name??"Built-in")+" / "+(GraphicsSettings.currentRenderPipeline?.name??"none")+" / "+QualitySettings.names[QualitySettings.GetQualityLevel()];
    RenderTexture.active=target;pixels=new Texture2D(width,height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(renderDir,name+".png"),pixels.EncodeToPNG());rendered.Add(name+".png");Debug.Log("OFFSCREEN_UI "+name);
   }finally{renderCamera.targetTexture=null;RenderTexture.active=previous;if(pixels)UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);}
  }
  [Serializable]class RenderEvidence {public string[] files;public string utc,device,pipeline;public string version=GameUI.ObservatoryVersion;public bool desktopControlled=false,desktopCaptured=false,visibleGameWindow=false;public string method="Edit-mode uGUI / TMP fixture rendered directly to RenderTexture";}
 }
}
