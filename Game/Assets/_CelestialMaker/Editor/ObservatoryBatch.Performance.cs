using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
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
  [Serializable] class FrameSample {public string page,mode;public int width,height,frames,texts,universes,ownedTypes;public double meanMs,p50Ms,p95Ms,p99Ms,maxMs,simulationMeanMs,uiMeanMs,renderMeanMs,uiP95Ms,simulationP95Ms,simulationMaxMs,allocatedBytesPerFrame=-1,uiAllocatedBytesPerFrame,submitP95Ms,readbackP95Ms;public int overBudget,gen0Collections;}
  [Serializable] class PerformanceEvidence {public string version=GameUI.ObservatoryVersion,utc,device,cpu,method="Isolated late-game edit-mode fixture with six purchased autobuyers: actual Core 60 Hz, animated Overview, actual uGUI, URP camera to private RenderTexture, synchronous 1-pixel GPU readback. Submission and readback are measured separately; not desktop/player FPS.";public string fixture="Tools/Expansion/Fixtures/late-campaign.json";public bool desktopControlled=false,visibleWindow=false,allocationCounterSupported;public int warmup=45;public FrameSample[] results;}
  public static void Benchmark(){
   if(!Application.isBatchMode||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new Exception("Offscreen GPU batch required");
   bool after=Environment.GetCommandLineArgs().Contains("-cmAfter");string dir=Path.Combine(ProjectBootstrap.Root,"Logs/ObservatoryPerformance");Directory.CreateDirectory(dir);
   var culture=System.Globalization.CultureInfo.CurrentCulture;System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.GetCultureInfo("en-US");
   long probeStart=GC.GetAllocatedBytesForCurrentThread();var probe=new byte[4096];GC.KeepAlive(probe);bool allocationSupported=GC.GetAllocatedBytesForCurrentThread()-probeStart>=4096;
   int quality=QualitySettings.GetQualityLevel();QualitySettings.SetQualityLevel(1);GameObject holder=null,hud=null,events=null,cam=null;var samples=new List<FrameSample>();
   try{
    holder=new GameObject("Private performance fixture");holder.SetActive(false);session=holder.AddComponent<GameSession>();GameSession.Instance=session;GameSession.Content=JsonUtility.FromJson<Content>(Resources.Load<TextAsset>("content").text);SaveStore.OverridePath=Path.Combine(dir,"isolated-save.json");
    events=new GameObject("Fixture events",typeof(EventSystem));if(!EventSystem.current)typeof(EventSystem).GetMethod("OnEnable",Private).Invoke(events.GetComponent<EventSystem>(),null);
    hud=new GameObject("Offscreen performance UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=(RectTransform)hud.transform;hud.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;hud.GetComponent<CanvasScaler>().enabled=false;ui=hud.AddComponent<GameUI>();ui.font=Resources.Load<TMP_FontAsset>("UI/PC/BodyFont");
    cam=new GameObject("Offscreen performance camera",typeof(Camera));var camera=cam.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.transform.position=new Vector3(0,0,-100);camera.nearClipPlane=.1f;camera.farClipPlane=200;camera.cullingMask=1<<31;camera.allowHDR=false;camera.allowMSAA=false;camera.GetUniversalAdditionalCameraData().renderPostProcessing=false;hud.GetComponent<Canvas>().worldCamera=camera;
    var args=Environment.GetCommandLineArgs();int pagesIndex=Array.IndexOf(args,"-cmPerfPages");var selectedPages=pagesIndex>=0?args[pagesIndex+1].Split(','):null;
    foreach(string mode in after?new[]{"optimized-60hz"}:new[]{"baseline-4hz","naive-60hz"})foreach(string page in new[]{"Production","Analytics","Automation","Parallel universes","Trees","Research","Production-QHD","Production-4K","Overview","Prestige","Megastructures","Satellite","Automation burst","Research-expanded","Research-goal","Search","Goal sequence","Laboratory","Chronicle","Engineering","Signals","Overview-large-text"}.Where(p=>selectedPages==null||selectedPages.Contains(p))){
     int width=page.EndsWith("4K")?3840:page.EndsWith("QHD")?2560:1920,height=width*9/16;
     Fixture(true,page=="Overview-large-text"?2:1,false,width,height);session.Sim.S.autobuyers=Enumerable.Repeat(true,6).ToArray();session.Sim.S.automation.buy=true;session.Sim.S.automation.expeditions=true;session.Sim.Meta.autoResearch=true;session.Sim.Meta.autoBuild=true;
     if(page=="Satellite")Set("pcWorld",1);Set("page",page=="Satellite"?"Production":page=="Automation burst"?"Automation":page.Split('-')[0]);Call("ShowPc");if(page=="Research-expanded"){Set("pcExpandedResearch",GameSession.Content.research[0].id);Call("ShowPc");}if(page=="Research-goal"){session.CurrentSettings.observatory.goal=GameSession.Content.research[0].id;Call("ShowPc");}if(page=="Search")Call("SearchPalettePc",false);
     if(page=="Goal sequence")Call("GoalSequencePc");
     if(page=="Laboratory"){PopulateDiscoveryLab();Call("PlanLaboratoryPc");}
     if(page=="Chronicle")Call("ChroniclePc");
     if(page=="Engineering")Call("ModulesPc",2);
     if(page=="Signals"){session.Sim.Discover.signals=Enumerable.Repeat(true,8).ToArray();Set("page","Expeditions");Call("ShowPc");Click("Signals and route discoveries");}
     Settle();foreach(var t in hud.GetComponentsInChildren<Transform>())t.gameObject.layer=31;
     var animated=ui.GetComponentsInChildren<ObservatoryCosmos>();
     var refresh=(Action)Delegate.CreateDelegate(typeof(Action),ui,typeof(GameUI).GetMethod("RefreshPc",Private));var presentMethod=typeof(GameUI).GetMethod("PresentPc",Private);Action present=presentMethod==null?null:(Action)Delegate.CreateDelegate(typeof(Action),ui,presentMethod);
     camera.orthographicSize=height/2f;camera.aspect=(float)width/height;camera.backgroundColor=CosmicStyle.Background;var target=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;var pixel=new Texture2D(1,1,TextureFormat.RGB24,false);var old=RenderTexture.active;
     var frameTimes=new List<double>();var uiTimes=new List<double>();var simTimes=new List<double>();var submitTimes=new List<double>();var readbackTimes=new List<double>();double simTotal=0,uiTotal=0,renderTotal=0;long allocated=0,uiAllocated=0;int collections=0;
     try{for(int f=-45;f<240;f++){
      // Separate synthetic stress case: force decision boundaries every second,
      // including both parallel economies. The normal campaign interval is unchanged.
      if(page=="Automation burst"&&f%60==0){session.Sim.Meta.nextAutomation=session.Sim.S.time;foreach(var u in session.Sim.Meta.universes){u.nextAutomation=session.Sim.S.time;session.Sim.Universe(u.id).Meta.nextAutomation=session.Sim.S.time;}}
      long frameBytes=GC.GetAllocatedBytesForCurrentThread();int gen0=GC.CollectionCount(0);long start=Stopwatch.GetTimestamp();FixedStepClock.Advance(session.Sim,1d/60);long simEnd=Stopwatch.GetTimestamp();long uiBytes=GC.GetAllocatedBytesForCurrentThread();
      foreach(var art in animated)art.AdvanceMotion(1f/60,session.CurrentSettings.reducedMotion);
      if(mode=="naive-60hz"||f%15==0)refresh();if(mode=="optimized-60hz")present?.Invoke();Canvas.ForceUpdateCanvases();long uiEnd=Stopwatch.GetTimestamp();
      long uiEndBytes=GC.GetAllocatedBytesForCurrentThread();camera.Render();long submitEnd=Stopwatch.GetTimestamp();RenderTexture.active=target;pixel.ReadPixels(new Rect(0,0,1,1),0,0,false);long end=Stopwatch.GetTimestamp();
      if(f>=0){frameTimes.Add(Millis(end-start));simTimes.Add(Millis(simEnd-start));uiTimes.Add(Millis(uiEnd-simEnd));submitTimes.Add(Millis(submitEnd-uiEnd));readbackTimes.Add(Millis(end-submitEnd));simTotal+=Millis(simEnd-start);uiTotal+=Millis(uiEnd-simEnd);renderTotal+=Millis(end-uiEnd);allocated+=GC.GetAllocatedBytesForCurrentThread()-frameBytes;uiAllocated+=uiEndBytes-uiBytes;collections+=GC.CollectionCount(0)-gen0;}
     }}finally{RenderTexture.active=old;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(pixel);target.Release();UnityEngine.Object.DestroyImmediate(target);}
     frameTimes.Sort();uiTimes.Sort();simTimes.Sort();submitTimes.Sort();readbackTimes.Sort();var s=new FrameSample{page=page,mode=mode,width=width,height=height,frames=frameTimes.Count,texts=ui.GetComponentsInChildren<TMP_Text>().Length,universes=session.Sim.Meta.universes.Count,ownedTypes=session.Sim.S.owned.Count(n=>n>0),meanMs=frameTimes.Average(),p50Ms=frameTimes[120],p95Ms=frameTimes[228],p99Ms=frameTimes[237],maxMs=frameTimes.Last(),simulationMeanMs=simTotal/240,uiMeanMs=uiTotal/240,renderMeanMs=renderTotal/240,uiP95Ms=uiTimes[228],simulationP95Ms=simTimes[228],simulationMaxMs=simTimes.Last(),allocatedBytesPerFrame=allocationSupported?allocated/240d:-1,uiAllocatedBytesPerFrame=allocationSupported?uiAllocated/240d:-1,submitP95Ms=submitTimes[228],readbackP95Ms=readbackTimes[228],gen0Collections=collections,overBudget=frameTimes.Count(x=>x>1000d/60)};samples.Add(s);UnityEngine.Debug.Log("PERFORMANCE "+mode+" "+page+" p95="+s.p95Ms+" mean="+s.meanMs);
    }
   }finally{if(hud)UnityEngine.Object.DestroyImmediate(hud);if(holder)UnityEngine.Object.DestroyImmediate(holder);if(cam)UnityEngine.Object.DestroyImmediate(cam);if(events)UnityEngine.Object.DestroyImmediate(events);GameSession.Instance=null;SaveStore.OverridePath=null;QualitySettings.SetQualityLevel(quality);System.Globalization.CultureInfo.CurrentCulture=culture;}
   var commandLine=Environment.GetCommandLineArgs();int outputIndex=Array.IndexOf(commandLine,"-cmPerfOutput");string output=outputIndex>=0?commandLine[outputIndex+1]:Path.Combine(dir,after?"after.json":"before.json");File.WriteAllText(output,JsonUtility.ToJson(new PerformanceEvidence{allocationCounterSupported=allocationSupported,utc=DateTime.UtcNow.ToString("o"),device=SystemInfo.graphicsDeviceName,cpu=SystemInfo.processorType,results=samples.ToArray()},true));
  }
  static double Millis(long ticks)=>ticks*1000d/Stopwatch.Frequency;
 }
}
