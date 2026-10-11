#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;
using CelestialMaker.Core;
using BreakInfinity;

namespace CelestialMaker {
 // Opt-in, isolated UI acceptance fixture. Never installed in a normal session.
 public sealed class PcUiReview:MonoBehaviour {
  string output;int errors;readonly List<string> checks=new List<string>(),overflows=new List<string>();
  static string Arg(string key,string fallback){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Install(){
   if(!Environment.GetCommandLineArgs().Contains("-cmUiReview"))return;
   string path=Arg("-cmArtifacts",Path.Combine(Application.temporaryCachePath,"CelestialMakerUI"));Directory.CreateDirectory(path);SaveStore.OverridePath=Path.Combine(path,"ui-review-save.json");
   var go=new GameObject("PC UI acceptance");DontDestroyOnLoad(go);go.AddComponent<PcUiReview>().output=path;
  }
  void Awake(){Application.logMessageReceived+=OnLog;}
  void OnLog(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){errors++;File.AppendAllText(Path.Combine(output??Application.temporaryCachePath,"ui-errors.txt"),message+"\n"+stack+"\n");}}
  void Check(bool pass,string name){checks.Add((pass?"PASS ":"FAIL ")+name);Debug.Log("UI CHECK "+checks.Last());if(!pass)errors++;}
  static bool Under(Transform t,string name){while(t){if(t.name==name)return true;t=t.parent;}return false;} Button Find(string name)=>FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.gameObject.activeInHierarchy&&b.name==name).OrderByDescending(b=>Under(b.transform,"Modal backdrop")?2:Under(b.transform,"Navigation rail")?1:0).ThenByDescending(b=>b.interactable).FirstOrDefault();
  void Click(string name){var b=Find(name);if(!b||!b.interactable)throw new Exception("Missing/disabled UI action: "+name);b.onClick.Invoke();}
  IEnumerator Settle(){yield return null;yield return null;yield return new WaitForSecondsRealtime(.6f);Canvas.ForceUpdateCanvases();}
  IEnumerator Capture(string name){yield return Settle();foreach(var t in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)){t.ForceMeshUpdate();if(t.isTextOverflowing)overflows.Add(name+" / "+t.transform.parent.name+" / "+t.text.Replace('\n',' '));}yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return null;yield return null;}
  IEnumerator LateFixture(){var s=SaveStore.Parse(File.ReadAllText(Arg("-cmFixture","")));s.utc=0;s.offlineRemaining=0;s.liveRemaining=0;s.offlineReport=null;s.settings.textScale=1;s.settings.tutorial=false;s.automation.buy=false;s.automation.expeditions=false;for(int i=0;i<3;i++)s.automation.repeatPrestige[i]=false;GameSession.Instance.Import(JsonUtility.ToJson(s));yield return Settle();}
  IEnumerator Start(){
   yield return new WaitUntil(()=>SceneManager.GetActiveScene().name=="01_MainMenu");yield return Settle();GameSession.Instance.Saved=null;FindFirstObjectByType<GameUI>().SendMessage("ShowMenu");yield return Capture("01-main-menu");Check(Screen.width==int.Parse(Arg("-screen-width","1920"))&&Screen.height==int.Parse(Arg("-screen-height","1080")),"requested render resolution "+Screen.width+"x"+Screen.height);
   Click("New Game");yield return new WaitUntil(()=>SceneManager.GetActiveScene().name=="02_Game");yield return Settle();var sim=GameSession.Instance.Sim;double time=sim.S.time;yield return new WaitForSecondsRealtime(.5f);Check(sim.S.time>time,"passive simulation continues");yield return Capture("02-overview-early");
   Click("Production");yield return Settle();sim.S.resources[0]=new BigDouble(10000);sim.S.lifetime[0]=new BigDouble(10000);yield return Settle();long owned=sim.S.owned.Sum();Click("Buy");yield return Settle();Check(sim.S.owned.Sum()>owned,"production Buy uses simulation binding");yield return Capture("03-production-early");
   var hint=Find("Buy").GetComponent<GameTooltip>();var eventData=new PointerEventData(EventSystem.current){position=new Vector2(Screen.width*.5f,Screen.height*.45f)};hint.OnPointerEnter(eventData);yield return new WaitForSecondsRealtime(.7f);Check(GameObject.Find("Hint")!=null,"hover tooltip visible");yield return Capture("04-tooltip");hint.OnPointerExit(eventData);
   Click("Research");yield return Settle();var unlock=FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.name=="Research"&&b.transform.parent.name!="Content"&&b.interactable);if(unlock){int count=sim.S.purchases.Count;unlock.onClick.Invoke();yield return Settle();Check(sim.S.purchases.Count>count,"research purchase binding");}else Check(false,"research affordable action exists");yield return Capture("05-research-early");
   yield return ReviewImprovements();
   yield return LateFixture();foreach(string p in new[]{"Overview","Production","Research","Prestige","Constellations","Challenges","Expeditions","Achievements","Codex","Analytics","Automation","Roadmap","Megastructures","Reality laws","Parallel universes","Settings"}){Click(p);yield return Settle();yield return Capture("late-"+p.ToLowerInvariant());Check(FindFirstObjectByType<GameUI>()!=null,p+" loaded");}
   Click("Import save");yield return Settle();var field=FindFirstObjectByType<TMP_InputField>();field.text="invalid json";string profile=GameSession.Instance.Sim.S.profileId;Click("Import validated save");yield return Settle();Check(GameSession.Instance.Sim.S.profileId==profile&&FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text.StartsWith("Import rejected:")),"invalid import rejected without replacing state");yield return Capture("06-import-window");Click("Cancel");
   Click("Prestige");yield return Settle();sim=GameSession.Instance.Sim;long oldUnits=sim.S.owned.Sum();Click("Begin Prestige");yield return Settle();Check(sim.S.owned.Sum()==oldUnits,"prestige waits for confirmation");yield return Capture("07-prestige-confirmation");Click("Cancel");Check(sim.S.owned.Sum()==oldUnits,"cancel preserves economy");Click("Begin Prestige");yield return Settle();var modal=FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name=="Begin Prestige"&&b.GetComponentInParent<Canvas>()!=null&&b.transform.parent.name=="Confirmation actions");int resets=sim.S.resets[0];modal.onClick.Invoke();yield return Settle();Check(sim.S.resets[0]==resets+1,"confirmed prestige binding");
   yield return LateFixture();Click("Challenges");yield return Settle();Click("Enter challenge");yield return Settle();Click("Begin challenge");yield return Settle();Check(GameSession.Instance.Sim.S.activeChallenge!="","confirmed challenge starts");Click("Exit and restore economy");yield return Settle();Click("Restore economy");yield return Settle();Check(GameSession.Instance.Sim.S.activeChallenge=="","challenge restores economy");
   Click("Expeditions");yield return Settle();var launch=Find("Launch expedition");if(launch&&launch.interactable){int count=GameSession.Instance.Sim.S.voyages.Count;launch.onClick.Invoke();yield return Settle();Check(GameSession.Instance.Sim.S.voyages.Count>count,"expedition launch binding");}
   sim=GameSession.Instance.Sim;sim.BeginOffline(3600);yield return new WaitUntil(()=>sim.S.offlineRemaining==0);yield return new WaitForSecondsRealtime(.7f);Check(Find("Expand details")!=null,"offline modal visible");yield return Capture("08-offline-summary-modal");Click("Expand details");yield return Settle();Check(sim.S.offlineReport.dismissed,"offline report dismiss state preserved");yield return Capture("09-offline-summary-details");
   Click("Overview");yield return Settle();sim.S.resources[0]=new BigDouble(9.99,99999);sim.S.lifetime[0]=new BigDouble(9.99,99999);yield return Capture("10-long-values");
   Click("Settings");yield return Settle();Click("Text scale: 100%");yield return Settle();yield return Capture("11-large-text");Check(Find("Text scale: 125%")!=null,"125 percent text mode retained");Check(GameObject.Find("Workspace").GetComponent<ScrollRect>().verticalNormalizedPosition>.99f,"text scale rebuild starts at readable top");Click("Overview");yield return Settle();Check(GameObject.Find("Next horizon")!=null,"next horizon remains present at large text without inspector");yield return Capture("11b-large-text-overview");Click("Settings");yield return Settle();Click("Text scale: 125%");yield return Settle();
   GameSession.Instance.Save();Check(SaveStore.Load()!=null,"UI actions leave a valid isolated save");Click("Save and return to menu");yield return Settle();Check(Find("Continue")!=null,"Continue presentation available");yield return Capture("12-continue-menu");
   File.WriteAllText(Path.Combine(output,"result.json"),JsonUtility.ToJson(new Result{errors=errors,width=Screen.width,height=Screen.height,checks=checks.ToArray(),textOverflowCandidates=overflows.Distinct().ToArray(),utc=DateTime.UtcNow.ToString("o")},true));Debug.Log("PC UI REVIEW "+(errors==0?"PASS":"FAIL"));Application.Quit(errors==0?0:1);
  }
  IEnumerator ReviewImprovements(){
   var sim=GameSession.Instance.Sim;
   Click("Purchased research");yield return Settle();Check(FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Where(t=>t.text=="Purchased").Count()==GameSession.Content.research.Count(u=>sim.StageOpen(u.stage)&&sim.S.purchases.Contains(u.id)),"purchased research filter includes exact owned discoveries");
   Click("Not owned");yield return Settle();Check(!FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text=="ACQUIRED / permanent this run"),"not-owned research filter excludes acquired cards");yield return Capture("13-research-filter");Click("All research");yield return Settle();
   Click("Production");yield return Settle();var scroll=GameObject.Find("Workspace").GetComponent<ScrollRect>();scroll.verticalNormalizedPosition=1;scroll.OnScroll(new PointerEventData(EventSystem.current){scrollDelta=new Vector2(0,-8)});yield return Settle();Check(scroll.verticalNormalizedPosition<.99f,"mouse-wheel ScrollRect event moves production content");scroll.verticalNormalizedPosition=1;
   int id=Array.FindIndex(GameSession.Content.generators,g=>g.stage==0&&g.input>=0);var generator=GameSession.Content.generators[id];sim.S.owned[id]=10;sim.S.paused[id]=false;sim.S.resources[generator.input]=BigDouble.Zero;sim.S.reserves[generator.input]=new BigDouble(1,20);sim.RefreshRates();Click("Production");yield return Settle();
   var inspect=FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name=="Inspect production"&&b.transform.parent.name==generator.name);inspect.onClick.Invoke();yield return Settle();Check(FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text.StartsWith("INPUT LIMITED")),"diagnostics identifies reserve-constrained input");yield return Capture("14-production-diagnosis");
   var navigation=Find("Research");var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,navigation.transform.position)};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);Check(hits.Count>0&&Under(hits[0].gameObject.transform,"Modal backdrop"),"modal intercepts raycasts over background navigation");navigation.onClick.Invoke();Check(Find("View input producers")!=null,"modal blocks background button callback");Click("View input producers");yield return Settle();Check(GameObject.Find("Modal backdrop")==null&&GameObject.Find(generator.name)!=null,"diagnosis routes to input production chapter");
   sim.S.reserves[generator.input]=BigDouble.Zero;sim.S.resources[generator.input]=new BigDouble(1000000);sim.RefreshRates();Click("Production");yield return Settle();Check(FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Any(t=>t.text.Contains("RUNNING / full output")),"healthy output is distinguished from input constraint");
   Click("Settings");yield return Settle();string clipboard=GUIUtility.systemCopyBuffer;Click("Export save");string json=GUIUtility.systemCopyBuffer;GUIUtility.systemCopyBuffer=clipboard;Check(SaveStore.Parse(json).profileId==sim.S.profileId,"UI export produces a valid current profile");Click("Import save");yield return Settle();FindFirstObjectByType<TMP_InputField>().text=json;Click("Import validated save");yield return Settle();Check(GameSession.Instance.Sim.S.profileId==sim.S.profileId&&FindFirstObjectByType<TMP_InputField>()==null,"valid UI import round trip preserves profile");
  }
  [Serializable]class Result{public int errors,width,height;public string[] checks,textOverflowCandidates;public string utc;}
 }
}
#endif
