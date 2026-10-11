using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CelestialMaker.Core;
using BreakInfinity;

namespace CelestialMaker {
 // Opt-in verification of the actual release executable. Never active in ordinary play.
 public sealed partial class ObservatoryHeadlessCheck : MonoBehaviour {
  string output;readonly List<string> checks=new List<string>(),errors=new List<string>();double began;bool finished;
  static string Argument(string key){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:null;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Install(){
   if(!Environment.GetCommandLineArgs().Contains("-cmObservatoryCheck"))return;
   if(!Application.isBatchMode||SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)throw new InvalidOperationException("Observatory checks require -batchmode -nographics");
   string folder=Argument("-cmArtifacts");if(string.IsNullOrWhiteSpace(folder)||!Path.IsPathRooted(folder))throw new ArgumentException("An absolute isolated artifacts directory is required");
   Directory.CreateDirectory(folder);SaveStore.OverridePath=Path.Combine(folder,"isolated-player-save.json");
   if(File.Exists(SaveStore.OverridePath))throw new InvalidOperationException("Use a fresh artifacts directory for each runtime check");
   var go=new GameObject("Headless release verification");DontDestroyOnLoad(go);var runner=go.AddComponent<ObservatoryHeadlessCheck>();runner.output=folder;runner.began=Time.realtimeSinceStartupAsDouble;
  }
  void Awake(){Application.logMessageReceived+=Log;}
  void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message+"\n"+stack);}
  void Update(){if(!finished&&Time.realtimeSinceStartupAsDouble-began>360){errors.Add("Headless runtime check timed out");Finish();}}
  void Check(bool condition,string name){if(!condition)throw new Exception(name);checks.Add(name);File.WriteAllText(Path.Combine(output,"checks-progress.txt"),string.Join("\n",checks));}
  GameUI UI=>FindFirstObjectByType<GameUI>();
  void Call(string method,params object[] args)=>typeof(GameUI).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(UI,args);
  void Page(string page){typeof(GameUI).GetField("page",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(UI,page);Call("ShowPc");}
  void Click(string name){var button=UI.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name==name&&b.gameObject.activeInHierarchy&&b.interactable);Check(button,"Enabled action: "+name);button.onClick.Invoke();}
  IEnumerator Start(){var run=Run();while(!finished){object next;try{if(!run.MoveNext())break;next=run.Current;}catch(Exception e){errors.Add((e.InnerException??e).ToString());break;}yield return next;}if(!finished)Finish();}
  IEnumerator Run(){
   while(SceneManager.GetActiveScene().name!="01_MainMenu")yield return null;yield return null;yield return null;
   Check(UI&&GameObject.Find("PC Observatory"),"Actual main menu builds Observatory");Check(System.Globalization.CultureInfo.CurrentCulture.Name=="en-US","Runtime language and number culture are English");
   GameSession.Instance.StartGame(true);while(SceneManager.GetActiveScene().name!="02_Game")yield return null;yield return null;yield return null;
   var sim=GameSession.Instance.Sim;sim.S.automation.buy=false;sim.Meta.guidedAutomation=false;double start=sim.S.time;for(int i=0;i<20;i++)yield return null;Check(sim.S.time>start,"Real player loop advances simulation");
   sim.S.resources[0]=new BigDouble(10000);Page("Production");yield return null;long owned=sim.S.owned[1];Click("Buy process 1");yield return null;Check(sim.S.owned[1]==owned+1,"Release UI purchase reaches production engine");
   sim.S.lifetime[0]=BigDouble.Parse("1e9");Call("ResetPreviewPc",0,false);yield return null;int resets=sim.S.resets[0];Click("Cancel");Check(sim.S.resets[0]==resets,"Reset cancellation preserves progression");
   Call("ResetPreviewPc",0,false);yield return null;Click("Confirm reset 0");yield return null;Check(sim.S.resets[0]==resets+1,"Confirmed reset reaches Core in release player");
   Check(!sim.AnyAutobuyer,"First reset does not grant free autobuyers");Page("Automation");yield return null;var beforeUnlock=sim.S.prestige[0].Value;Click("Unlock autobuyer 0");yield return null;Check(sim.S.autobuyers[0]&&sim.S.prestige[0].Value==beforeUnlock-1,"Release autobuyer purchase spends one Prestige Point");sim.S.automation.buy=false;
   Page("Overview");sim.S.settings.reducedMotion=false;yield return null;
   // Unlocks may rebuild the view on the next UI tick. Observe the live graphic,
   // rather than holding a destroyed instance from the previous view.
   var artwork=UI.GetComponentInChildren<ObservatoryCosmos>();float phase=artwork.Phase;bool animated=false;double motionDeadline=Time.realtimeSinceStartupAsDouble+2;
   while(Time.realtimeSinceStartupAsDouble<motionDeadline&&!animated){yield return null;var current=UI.GetComponentInChildren<ObservatoryCosmos>();if(current==artwork&&current&&current.Phase!=phase)animated=true;else if(current!=artwork){artwork=current;phase=current?current.Phase:0;}}
   Check(animated,"Actual player updates Observatory animation");sim.S.settings.reducedMotion=true;yield return null;artwork=UI.GetComponentInChildren<ObservatoryCosmos>();phase=artwork.Phase;
   for(int i=0;i<10;i++){yield return null;var current=UI.GetComponentInChildren<ObservatoryCosmos>();if(current!=artwork){artwork=current;phase=current?current.Phase:0;}else if(current&&current.Phase!=phase)throw new Exception("Reduced motion changed the active artwork phase");}
   Check(artwork&&artwork.Phase==phase,"Actual player respects reduced motion");sim.S.settings.reducedMotion=false;
   foreach(string page in new[]{"Overview","Production","Research","Upgrades","Prestige","Trees","Constellations","Challenges","Expeditions","Achievements","Codex","Settings","Offline summary","Analytics","Automation","Roadmap","Megastructures","Reality laws","Parallel universes"}){Page(page);yield return null;Check(GameObject.Find("Workspace"),"Release route: "+page);}
   Page("Research");yield return null;Click("Research filter 2");Check(sim.S.settings.observatory.researchFilter==2,"Release filter persists selection");Click("Research filter 0");
   Call("SearchPalettePc",false);yield return null;var searchModal=(RectTransform)typeof(GameUI).GetField("pcModal",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(UI);var input=searchModal.GetComponentInChildren<TMPro.TMP_InputField>();input.text="Quantum memory 1";double searchDeadline=Time.realtimeSinceStartupAsDouble+2;while(Time.realtimeSinceStartupAsDouble<searchDeadline&&!UI.GetComponentsInChildren<Button>().Any(b=>b.name=="Research / Quantum memory 1"))yield return null;Check(UI.GetComponentsInChildren<Button>().Any(b=>b.name=="Research / Quantum memory 1"),"Debounced search updates in actual player");Click("Research / Quantum memory 1");yield return null;Click("Add favorite");yield return null;Check(sim.S.settings.observatory.favorites.Contains(GameSession.Content.research[0].id),"Favorite callback persists in actual player");Click("Pin goal");yield return null;Check(sim.S.settings.observatory.goal==GameSession.Content.research[0].id,"Pinned goal callback reaches settings");Click("Queue with prerequisites");yield return null;Check(sim.S.researchPlan.queue.Count==1,"Release research plan callback");
   Call("ResearchPlanPc");yield return null;Call("ClosePcModal");
   sim.S.settings.textScale=2;sim.S.settings.lightUI=true;Call("BuildPc");yield return null;yield return null;Check(UI.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.fontSize>=36),"200 percent text and Dawn rebuild in player");
   sim.S.automation.buy=false;sim.BeginOffline(3600);while(sim.S.offlineRemaining>0)yield return null;yield return null;Check(sim.S.offlineReport!=null&&!sim.S.offlineReport.active,"Player loop completes offline catch-up");
   GameSession.Instance.Save();Check(!GameSession.Instance.SaveFailed&&GameSession.Instance.LastSavedUtc>0,"Player reports successful isolated save");var saved=SaveStore.Load();Check(saved!=null&&saved.settings.textScale==2&&saved.settings.lightUI&&saved.resets[0]==sim.S.resets[0],"Player save reload retains UI settings and progress");
   Check(saved.settings.observatory.favorites.Contains(GameSession.Content.research[0].id)&&saved.settings.observatory.goal==GameSession.Content.research[0].id&&saved.researchPlan.queue.Count==1,"Release planning and favorites survive save reload");
   Check(SaveStore.Checkpoints().Length>0,"Confirmed release reset created a restore point");
   sim.S.lifetime[0]=BigDouble.Parse("1e9");Call("ResetPreviewPc",0,false);yield return null;
   var oldPoints=sim.S.prestige[0].Value;int oldResets=sim.S.resets[0];Click("Forecast retained rules");double deadline=Time.realtimeSinceStartupAsDouble+7;
   while(Time.realtimeSinceStartupAsDouble<deadline&&!UI.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text.Contains("Target:")))yield return null;
   Check(UI.GetComponentsInChildren<TMPro.TMP_Text>().Any(t=>t.text.Contains("Target:")),"Recovery coroutine finishes in actual release");Check(sim.S.prestige[0].Value==oldPoints&&sim.S.resets[0]==oldResets,"Recovery coroutine awards no live reset");Call("ClosePcModal");
   double beforeCheckpointTime=sim.S.time;sim.S.time+=10000;Check(GameSession.Instance.CreateCheckpoint("Prestige"),"Prestige checkpoint records later time");sim.S.time=beforeCheckpointTime;
   Check(GameSession.Instance.CreateCheckpoint("runtime-restore"),"Manual release checkpoint succeeds");var point=SaveStore.Checkpoints().First();long expected=point.state.owned[0];sim.S.owned[0]+=123;
   GameSession.Instance.RestoreCheckpoint(point.file);yield return null;yield return null;sim=GameSession.Instance.Sim;
   Check(sim.S.owned[0]==expected,"Actual player restore replaces progress with selected checkpoint");Check(SaveStore.Checkpoints().Any(x=>x.reason=="before-restore"),"Actual restore keeps a rollback copy");
   int pointCount=SaveStore.Checkpoints().Length;Check(GameSession.Instance.CreateCheckpoint("Prestige")&&SaveStore.Checkpoints().Length>pointCount,"Restoring an earlier time resets checkpoint throttle");
   Call("ClosePcModal");Page("Automation");yield return null;Click("Plan Energy");Click("Enable development plan");Check(sim.Evolution.developmentPlan=="Energy","Release enables reviewed Development Plan");
   sim.S.researchPlan.queue.Clear();sim.BeginOffline(21600);for(int i=0;i<10;i++)yield return null;
   Check(sim.S.offlineRemaining>0&&sim.S.offlineReport.fixedTicks>0,"Six-hour replay yields to actual player frames");
   GameSession.Instance.Save();var cursor=SaveStore.Load();Check(cursor.offlineRemaining>0&&cursor.offlineReport.fixedTicks>0&&cursor.evolution.developmentPlan=="Energy","Actual partial catch-up saves cursor and strategy");
   GameSession.Instance.ReturnToMenu();while(SceneManager.GetActiveScene().name!="01_MainMenu")yield return null;GameSession.Instance.StartGame(false);while(SceneManager.GetActiveScene().name!="02_Game")yield return null;sim=GameSession.Instance.Sim;
   while(sim.S.offlineRemaining>0)yield return null;yield return null;
   Check(sim.S.offlineReport.processedSeconds>=21600&&sim.S.offlineReport.fixedTicks>=1295900,"Actual executable resumes full six-hour tick replay");Check(sim.S.offlineReport.automaticUnits.Sum()>0&&sim.S.purchases.Count>0,"Automation purchases and research run while absent");
   Check(sim.Meta.historicResets[1]==0,"First Ascension still waits for player approval");Check(SaveStore.Checkpoints().Any(x=>x.reason=="before-offline"),"Catch-up has a pre-absence restore point");
   Call("ClosePcModal");Page("Automation");Click("Stop plan and automatic resets");Check(sim.Evolution.developmentPlan=="","Release stops development plan");
   var discovery=DiscoveryRuntime();while(discovery.MoveNext())yield return discovery.Current;
   Check(errors.Count==0,"No runtime exceptions or errors during headless smoke");
  }
  void Finish(){if(finished)return;finished=true;File.WriteAllText(Path.Combine(output,"result.json"),JsonUtility.ToJson(new Result{version=Application.version,checks=checks.ToArray(),errors=errors.ToArray(),pass=errors.Count==0,utc=DateTime.UtcNow.ToString("o")},true));Application.Quit(errors.Count==0?0:1);}
  void OnDestroy(){Application.logMessageReceived-=Log;}
  [Serializable]class Result {public bool pass;public string version,utc;public string[] checks,errors;public bool batchmode=true,graphicsEnabled=false,desktopControlled=false,screenCaptured=false;public string visual="UNVERIFIED - release executed with null graphics device";}
 }
}
