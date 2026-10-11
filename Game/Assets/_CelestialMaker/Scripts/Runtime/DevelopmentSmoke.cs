#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using CelestialMaker.Core;
namespace CelestialMaker {
 public sealed class DevelopmentSmoke:MonoBehaviour {
  public static bool Enabled=>Environment.GetCommandLineArgs().Contains("-cmSmoke")||Environment.GetCommandLineArgs().Contains("-cmQA");
  string output;int errors;List<Measurement> measurements=new List<Measurement>();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Install(){if(!Enabled)return;var go=new GameObject("DevelopmentSmoke");DontDestroyOnLoad(go);go.AddComponent<DevelopmentSmoke>();}
  IEnumerator Start(){
   if(Environment.GetCommandLineArgs().Contains("-cmQA"))yield break;
   var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-cmArtifacts");output=index>=0?args[index+1]:Application.persistentDataPath;Directory.CreateDirectory(output);Application.logMessageReceived+=Log;
   yield return new WaitUntil(()=>SceneManager.GetActiveScene().name=="01_MainMenu");yield return new WaitForSecondsRealtime(1);yield return Capture("windows-menu.png");
   GameSession.Instance.Saved=null;Click("New Game");yield return new WaitUntil(()=>SceneManager.GetActiveScene().name=="02_Game");yield return new WaitForSecondsRealtime(1);
   var sim=GameSession.Instance.Sim;double start=sim.S.time;var energy=sim.S.resources[0].Value;yield return Measure("early passive Overview",600);bool passive=sim.S.time>start&&sim.S.resources[0].Value>energy;yield return Capture("windows-overview.png");Click("Production");yield return null;yield return Capture("windows-production.png");
   int fixture=Array.IndexOf(args,"-cmFixture");if(fixture>=0){
    string fixtureFile=args[fixture+1],stageFolder=Path.Combine(Path.GetDirectoryName(fixtureFile),"StageFixtures");
    for(int stage=0;stage<6;stage++){string file=Path.Combine(stageFolder,"stage-"+stage+".json");if(File.Exists(file)){GameSession.Instance.Import(File.ReadAllText(file));yield return new WaitForSecondsRealtime(.8f);yield return Capture("windows-stage-"+stage+".png");}}
    GameSession.Instance.Import(File.ReadAllText(fixtureFile));sim=GameSession.Instance.Sim;Array.Clear(sim.S.paused,0,24);sim.S.reserves=new Number[12];sim.S.settings.tutorial=false;sim.S.automation.buy=false;sim.S.automation.expeditions=false;yield return new WaitForSecondsRealtime(1);Click("Production");yield return null;yield return Capture("windows-late-game.png");yield return Measure("late chain, all unpaused, Production UI",600);
    sim.S.automation.buy=true;sim.S.automation.expeditions=true;yield return Measure("late autobuy and expeditions, Production UI",600);
    sim.BeginOffline(31536000);yield return Measure("365-day catch-up and summary",600);while(sim.S.offlineRemaining>0)yield return null;yield return Capture("windows-offline-summary.png");
   }
   GameSession.Instance.Save();var restored=SaveStore.Load();Finish(passive&&restored!=null&&restored.time>start&&errors==0,"passive="+passive+" save="+(restored!=null));
  }
  void Click(string name){var button=FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name==name);if(!button)throw new Exception("Missing UI button: "+name);button.onClick.Invoke();}
  void Log(string text,string stack,LogType type){if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)errors++;}
  IEnumerator Capture(string filename){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,filename));yield return null;yield return null;}
  IEnumerator Measure(string scenario,int count){
   var frames=new List<double>();var ticks=new List<double>();var waits=new List<double>();var cpu=new List<double>();var timings=new FrameTiming[1];double started=Time.realtimeSinceStartupAsDouble;int catchingUp=0;
   for(int i=0;i<count;i++){yield return null;frames.Add(Time.unscaledDeltaTime*1000);ticks.Add(GameSession.Instance.LastTickMs);if(GameSession.Instance.Sim.S.offlineRemaining>0)catchingUp++;FrameTimingManager.CaptureFrameTimings();if(FrameTimingManager.GetLatestTimings(1,timings)>0){cpu.Add(timings[0].cpuMainThreadFrameTime);waits.Add(timings[0].cpuMainThreadPresentWaitTime);}}
   frames.Sort();ticks.Sort();var sim=GameSession.Instance.Sim;sim.RefreshRates();measurements.Add(new Measurement{scenario=scenario,frames=count,seconds=Time.realtimeSinceStartupAsDouble-started,width=Screen.width,height=Screen.height,frameLimit=sim.S.settings.fps,vSync=QualitySettings.vSyncCount,displayHz=Screen.currentResolution.refreshRateRatio.value,meanFrameMs=frames.Average(),p95FrameMs=frames[(int)(count*.95)],fps=1000/frames.Average(),meanTickMs=ticks.Average(),p95TickMs=ticks[(int)(count*.95)],maxTickMs=ticks.Last(),unityAllocatedMiB=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()/1048576d,ownedTypes=sim.S.owned.Count(n=>n>0),flowingTypes=Enumerable.Range(0,24).Count(i=>sim.CachedFlow(i)>0),ownedUnits=sim.S.owned.Sum(),voyages=sim.S.voyages.Count(v=>!v.claimed),catchupFrames=catchingUp,timingSamples=cpu.Count,meanMainThreadMs=cpu.Count>0?cpu.Average():-1,meanPresentWaitMs=waits.Count>0?waits.Average():-1});
  }
  void Finish(bool pass,string reason){var result=new Result{pass=pass,reason=reason,errors=errors,processor=SystemInfo.processorType,gpu=SystemInfo.graphicsDeviceName,systemMemoryMB=SystemInfo.systemMemorySize,unity=Application.unityVersion,utc=DateTime.UtcNow.ToString("o"),measurements=measurements.ToArray()};File.WriteAllText(Path.Combine(output,"windows-smoke.json"),JsonUtility.ToJson(result,true));Debug.Log("CELESTIAL RUNTIME "+(pass?"PASS":"FAIL")+" "+reason);Application.Quit(pass?0:1);}
  [Serializable]class Result{public bool pass;public string reason,processor,gpu,unity,utc;public int errors,systemMemoryMB;public Measurement[] measurements;}
  [Serializable]class Measurement{public string scenario;public int frames,width,height,frameLimit,vSync,ownedTypes,flowingTypes,voyages,catchupFrames,timingSamples;public long ownedUnits;public double seconds,displayHz,meanFrameMs,p95FrameMs,fps,meanTickMs,p95TickMs,maxTickMs,unityAllocatedMiB,meanMainThreadMs,meanPresentWaitMs;}
 }
}
#endif
