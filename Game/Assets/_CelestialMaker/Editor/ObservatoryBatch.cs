using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using BreakInfinity;
using CelestialMaker.Core;

namespace CelestialMaker.EditorTools {
 // Edit-mode fixtures only: no Play Mode, game window, OS input, capture, or user saves.
 public static partial class ObservatoryBatch {
  static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
  static readonly List<string> checks=new List<string>(),failures=new List<string>();
  static GameSession session;static GameUI ui;static RectTransform canvas;
  static string Dir=>Path.Combine(ProjectBootstrap.Root,"Logs/Observatory");
  static void Call(string method,params object[] args){typeof(GameUI).GetMethod(method,Private).Invoke(ui,args);}
  static void Set(string field,object value){typeof(GameUI).GetField(field,Private).SetValue(ui,value);}
  static T Get<T>(string field)=>(T)typeof(GameUI).GetField(field,Private).GetValue(ui);
  static Button Button(string name){var modal=Get<RectTransform>("pcModal");return ui.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.name==name&&b.gameObject.activeInHierarchy&&(!modal||b.transform.IsChildOf(modal)));}
  static void Click(string name){var b=Button(name);if(!b||!b.interactable)throw new Exception("Missing/disabled button "+name);b.onClick.Invoke();Settle();}
  static void Check(string name,Action action){try{action();checks.Add(name);}catch(Exception e){failures.Add(name+": "+(e.InnerException??e).Message);Debug.LogWarning(failures.Last()+"\n"+(e.InnerException??e).StackTrace);}}
  static void Require(bool value,string message){if(!value)throw new Exception(message);}
  static void Settle(){for(int n=0;n<4;n++){Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(canvas);foreach(var sc in ui.GetComponentsInChildren<ScrollRect>())if(sc.content)LayoutRebuilder.ForceRebuildLayoutImmediate(sc.content);}Canvas.ForceUpdateCanvases();}
  static string renderLateFixture;
  static void Fixture(bool late,float scale,bool light,int width,int height){
   State state;if(late){state=JsonUtility.FromJson<State>(File.ReadAllText(renderLateFixture??Path.Combine(ProjectBootstrap.Root,"Tools/Expansion/Fixtures/late-campaign.json")));state.offlineRemaining=state.liveRemaining=0;state.offlineReport=null;}else state=new State();
   state.settings.textScale=scale;state.settings.lightUI=light;state.settings.tutorial=false;state.automation.buy=false;state.automation.expeditions=false;state.automation.repeatPrestige=new bool[3];state.expansion.guidedAutomation=false;
   session.Sim=new Simulation(GameSession.Content,state);session.Saved=null;session.SaveFailed=false;session.LastSavedUtc=0;
   canvas.sizeDelta=new Vector2(width,height);ui.menu=false;Set("pcWorld",0);Set("pcSelectedProcess",-1);Set("pcProductionStage",-1);Set("pcAvailability",0);Set("pcResearchFilter",0);Set("pcExpandedResearch","");Set("page","Production");Call("BuildPc");Settle();
  }
  static void Geometry(string context){
   var violations=new List<string>();
   foreach(var t in ui.GetComponentsInChildren<TMP_Text>()){
    if(string.IsNullOrEmpty(t.text))continue;var r=t.rectTransform.rect;float expected=t.GetPreferredValues(t.text,Mathf.Max(1,r.width),0).y;
    if(r.width<=0||r.height<=0||expected>r.height+3)violations.Add(t.transform.parent.name+" / "+t.text.Replace('\n',' ').Substring(0,Math.Min(70,t.text.Length))+" ["+r.width+"x"+r.height+", needs "+expected+"]");
    if(t.enableAutoSizing)violations.Add("Automatic text shrinking: "+t.text);
   }
   foreach(var sc in ui.GetComponentsInChildren<ScrollRect>())Require(!sc.horizontal,context+" horizontal scroll");
   if(violations.Count>0)throw new Exception(string.Join(" | ",violations.Take(8)));
  }
  public static void Validate(){
   Directory.CreateDirectory(Dir);checks.Clear();failures.Clear();
   GameObject holder=null,hud=null,events=null;
   try{
    holder=new GameObject("Isolated observatory session");holder.SetActive(false);session=holder.AddComponent<GameSession>();GameSession.Instance=session;GameSession.Content=JsonUtility.FromJson<Content>(Resources.Load<TextAsset>("content").text);SaveStore.OverridePath=Path.Combine(Dir,"fixture-save.json");
    events=new GameObject("Fixture EventSystem",typeof(EventSystem));if(!EventSystem.current)typeof(EventSystem).GetMethod("OnEnable",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(events.GetComponent<EventSystem>(),null);
    hud=new GameObject("Isolated UI canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));canvas=(RectTransform)hud.transform;hud.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;hud.GetComponent<CanvasScaler>().enabled=false;ui=hud.AddComponent<GameUI>();ui.font=Resources.Load<TMP_FontAsset>("UI/PC/BodyFont");
    if(Environment.GetCommandLineArgs().Contains("-cmPhoneUi"))PhoneChecks();else{
    bool quick=Environment.GetCommandLineArgs().Contains("-cmQuickUi");
    string[] pages={"Overview","Production","Research","Upgrades","Prestige","Trees","Constellations","Challenges","Expeditions","Achievements","Codex","Settings","Offline summary","Analytics","Automation","Roadmap","Megastructures","Reality laws","Parallel universes"};
    int[,] sizes={{1280,720},{1920,1080},{2560,1440},{3440,1440}};
    foreach(bool late in new[]{false,true})foreach(bool light in quick?new[]{false}:new[]{false,true})foreach(float scale in quick?new[]{1f}:new[]{1f,1.25f,1.5f,2f})for(int size=quick?1:0;size<(quick?2:4);size++){
     Fixture(late,scale,light,sizes[size,0],sizes[size,1]);
     foreach(string page in pages){string context=(late?"late":"early")+"/"+(light?"dawn":"night")+"/"+sizes[size,0]+"x"+sizes[size,1]+"/"+scale+"/"+page;Check(context,()=>{Set("page",page);Call("ShowPc");Settle();Geometry(context);});}
     Check("Reset preview layout "+late+"/"+light+"/"+scale+"/"+size,()=>{Call("ResetPreviewPc",1,false);Settle();Geometry("reset");Call("ClosePcModal");});
     Check("Menu layout "+late+"/"+light+"/"+scale+"/"+size,()=>{ui.menu=true;Call("BuildPc");Settle();Geometry("menu");ui.menu=false;});
    }
    Check("Six primary sections preserve every original route",()=>{Fixture(true,1,false,1920,1080);var names=Get<Dictionary<string,Button>>("pcTabs").Keys.ToArray();Require(names.SequenceEqual(new[]{"Overview","Production","Development","Automation","Cosmos","Journal"}),"Wrong navigation");foreach(string route in pages){Set("page",route);Call("ShowPc");Require(ui.GetComponentsInChildren<TMP_Text>().Any(t=>!string.IsNullOrEmpty(t.text)),"Empty "+route);}});
    Check("Live purchase uses Core cost and retains scroll and selection",()=>{Fixture(false,1,false,1280,720);session.Sim.S.resources[0]=new BigDouble(10000);Set("pcSelectedProcess",1);Call("ShowPc");Settle();Get<ScrollRect>("pcScroll").verticalNormalizedPosition=.3f;var before=session.Sim.S.resources[0].Value;var cost=session.Sim.Cost(1);long owned=session.Sim.S.owned[1];Click("Buy process 1");Require(session.Sim.S.owned[1]==owned+1&&session.Sim.S.resources[0].Value==before-cost,"Purchase binding/cost differs");Require(Get<int>("pcSelectedProcess")==1&&Math.Abs(Get<ScrollRect>("pcScroll").verticalNormalizedPosition-.3f)<.01,"Context lost: selected="+Get<int>("pcSelectedProcess")+" scroll="+Get<ScrollRect>("pcScroll").verticalNormalizedPosition);});
    Check("Input shortage diagnosis and upstream navigation",()=>{Fixture(false,1,false,1280,720);session.Sim.S.owned[1]=10;session.Sim.S.resources[0]=BigDouble.Zero;session.Sim.S.reserves[0]=new BigDouble(1e9);Call("ProcessDetailsPc",1);Settle();Require(ui.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Input limited")),"No reason shown");Click("Go to input producer");Require(Get<int>("pcSelectedProcess")==0&&Get<RectTransform>("pcModal")==null,"Upstream route incorrect");});
    Check("Reset preview and cancel preserve exact state; confirm uses Core",()=>{Fixture(true,1,false,1920,1080);var s=session.Sim;for(int i=0;i<12;i++){s.S.resources[i]=BigDouble.Parse("1e18");s.S.lifetime[i]=BigDouble.Parse("1e18");}s.Meta.prestigeEarned=new BigDouble(100);s.S.prestige[0]=new BigDouble(10);s.S.prestigePurchases.Add("p0_0");s.S.offlineReport=new OfflineReport{active=false,dismissed=true};string before=JsonUtility.ToJson(s.S);Call("ResetPreviewPc",1,false);Settle();if(before!=JsonUtility.ToJson(s.S)){File.WriteAllText(Path.Combine(Dir,"preview-before.json"),before);File.WriteAllText(Path.Combine(Dir,"preview-after.json"),JsonUtility.ToJson(s.S));}Require(before==JsonUtility.ToJson(s.S),"Preview mutates save");Click("Cancel");Require(before==JsonUtility.ToJson(s.S),"Cancel mutates save");Call("ResetPreviewPc",1,false);Settle();Click("Confirm reset 1");Require(s.S.prestige[0].Value==0&&!s.S.prestigePurchases.Contains("p0_0"),"Ascension retained lower points/upgrades");});
    Check("Automatic reset needs explicit confirmation",()=>{Fixture(true,1,false,1920,1080);session.Sim.S.resets[0]=1;Call("ResetPreviewPc",0,true);Settle();Require(!session.Sim.S.automation.repeatPrestige[0],"Enabled before confirmation");Click("Cancel");Require(!session.Sim.S.automation.repeatPrestige[0],"Cancel enabled reset");Call("ResetPreviewPc",0,true);Settle();Click("Confirm reset 0");Require(session.Sim.S.automation.repeatPrestige[0],"Confirmation did not enable");});
    Check("Selected parallel economy purchase never spends home stock",()=>{Fixture(true,1,false,1920,1080);Set("pcWorld",1);session.Sim.EditUniverse(1,s=>{s.S.resources[0]=new BigDouble(1e12);s.S.owned[0]=1;});Set("page","Production");Call("ShowPc");Settle();var home=session.Sim.S.resources[0].Value;long homeOwned=session.Sim.S.owned[0];long parallel=session.Sim.Universe(1).S.owned[0];Click("Buy process 0");Require(session.Sim.Universe(1).S.owned[0]==parallel+1&&session.Sim.S.owned[0]==homeOwned&&session.Sim.S.resources[0].Value==home,"Cross-universe mutation");});
    Check("Settings 80-200 percent survive JSON without changing economy",()=>{foreach(float scale in new[]{.8f,1f,1.25f,1.5f,2f}){var s=new State();s.settings.textScale=scale;s.settings.lightUI=true;var round=SaveStore.Parse(JsonUtility.ToJson(s));Require(round.settings.textScale==scale&&round.settings.lightUI&&round.resources[0].Value==10,"Setting lost");}var invalid=new State();invalid.settings.textScale=2.1f;bool rejected=false;try{new Simulation(GameSession.Content,invalid);}catch(ArgumentException){rejected=true;}Require(rejected,"Out-of-range scale accepted");});
    Check("Offline accounting and save failure remain readable",()=>{Fixture(false,2,false,1280,720);session.Sim.BeginOffline(3600);while(session.Sim.S.offlineRemaining>0)session.Sim.ProcessOffline(128);Set("page","Offline summary");Set("offlineDetails",true);Call("ShowPc");Settle();Geometry("offline");session.SaveFailed=true;session.Message="Save failed: test storage unavailable";Set("page","Settings");Call("ShowPc");Settle();Geometry("save failure");Require(Button("Retry save"),"No retry action");});
    Check("Modal contains focus and restores invoking control",()=>{Fixture(false,1,false,1280,720);var target=Button("Buy process 0");Require(target,"Focus fixture target missing");Require(EventSystem.current,"Fixture EventSystem missing");EventSystem.current.SetSelectedGameObject(target.gameObject);Call("ProcessDetailsPc",0);Settle();Require(EventSystem.current.currentSelectedGameObject.transform.IsChildOf(Get<RectTransform>("pcModal")),"Focus escaped modal");Call("ClosePcModal");Require(EventSystem.current.currentSelectedGameObject==target.gameObject,"Focus not restored");});
    Check("English authored content and runtime language",()=>{foreach(string file in Directory.GetFiles(Path.Combine(Application.dataPath,"_CelestialMaker/Scripts"),"*.cs",SearchOption.AllDirectories).Concat(new[]{Path.Combine(Application.dataPath,"_CelestialMaker/Resources/content.json")})){string source=File.ReadAllText(file);Require(!System.Text.RegularExpressions.Regex.IsMatch(source,"[ąćęłńóśźżĄĆĘŁŃÓŚŹŻ]"),"Non-English authored text: "+file);}Require(File.ReadAllText(Path.Combine(Application.dataPath,"_CelestialMaker/Scripts/Runtime/GameSession.cs")).Contains("GetCultureInfo(\"en-US\")"),"No English culture");});
    Check("Navigation words fit at 200 percent",()=>{Fixture(false,2,false,1280,720);foreach(var b in Get<Dictionary<string,Button>>("pcTabs").Values){var label=b.GetComponentInChildren<TMP_Text>();Require(label.GetPreferredValues(label.text).x<=label.rectTransform.rect.width+1,"Wrapped navigation: "+label.text);}});
    Check("Report and universe dialogs use one full-height scroll",()=>{Fixture(true,2,false,1280,720);MakeOffline();Call("ShowOfflinePc");Settle();Require(Get<RectTransform>("pcModal").GetComponentsInChildren<ScrollRect>().Length==1,"Nested report scroll");Require(Button("Continue experiment"),"Missing dismiss");Geometry("offline modal");Call("ClosePcModal");Call("UniverseDetailsPc",1);Settle();Require(Get<RectTransform>("pcModal").GetComponentsInChildren<ScrollRect>().Length==1,"Nested universe scroll");Geometry("universe modal");Call("ClosePcModal");});
    Check("Save and profile input text is clipped inside its field",()=>{Fixture(true,2,false,1280,720);foreach(string method in new[]{"ImportPc","StrategyTransferPc"}){Call(method);Settle();var input=Get<RectTransform>("pcModal").GetComponentInChildren<TMP_InputField>();Require(input.textViewport.GetComponent<RectMask2D>(),method+" lacks a text mask");input.text=new string('x',10000);Settle();Require(input.textComponent.GetComponentInParent<RectMask2D>().transform==input.textViewport,"Wrong input clipping boundary");Call("ClosePcModal");}});
    Check("Both themes retain text contrast in all control states",()=>{foreach(bool light in new[]{false,true}){Fixture(true,1,light,1920,1080);foreach(string route in new[]{"Overview","Production","Research","Prestige","Trees","Megastructures","Reality laws","Achievements","Codex","Offline summary"}){Set("page",route);Call("ShowPc");foreach(var button in ui.GetComponentsInChildren<ObservatoryButton>())foreach(string state in new[]{"Normal","Highlighted","Pressed","Selected","Disabled"}){ButtonState(button,state);Color foreground=button.GetComponentInChildren<TMP_Text>().color,background=button.GetComponent<CosmicPanel>().color;float a=Luminance(foreground),b=Luminance(background),ratio=(Mathf.Max(a,b)+.05f)/(Mathf.Min(a,b)+.05f);Require(ratio>=4.5f,light+" "+button.name+" "+state+" contrast "+ratio);var icon=button.GetComponentInChildren<ObservatoryTabIcon>();if(icon&&state!="Disabled"){float c=Luminance(icon.color);Require((Mathf.Max(c,b)+.05f)/(Mathf.Min(c,b)+.05f)>=3,"View icon contrast: "+route+" / "+state);}}}}});
    Check("Ultrawide workspace keeps a bounded reading width",()=>{Fixture(true,1,false,3440,1440);Require(Get<RectTransform>("root").rect.width<=2560,"Unbounded ultrawide workspace");});
    PresentationChecks();ReadabilityChecks();ResourceChecks();UnlockChecks();EvolutionChecks();DiscoveryChecks();}
   }finally{if(hud)UnityEngine.Object.DestroyImmediate(hud);if(holder)UnityEngine.Object.DestroyImmediate(holder);if(events){if(EventSystem.current==events.GetComponent<EventSystem>())typeof(EventSystem).GetMethod("OnDisable",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(events.GetComponent<EventSystem>(),null);UnityEngine.Object.DestroyImmediate(events);}GameSession.Instance=null;SaveStore.OverridePath=null;}
   File.WriteAllText(Path.Combine(Dir,"ui-headless-tests.json"),JsonUtility.ToJson(new Result{pass=checks.Count,fail=failures.Count,checks=checks.ToArray(),failures=failures.ToArray(),utc=DateTime.UtcNow.ToString("o")},true));Debug.Log("OBSERVATORY UI CHECKS "+checks.Count+" passed / "+failures.Count+" failed");if(failures.Count>0)throw new Exception("Observatory headless UI checks failed: "+failures.Count);
  }
  static float Luminance(Color color){float Channel(float c)=>c<=.04045f?c/12.92f:Mathf.Pow((c+.055f)/1.055f,2.4f);return .2126f*Channel(color.r)+.7152f*Channel(color.g)+.0722f*Channel(color.b);}
  [Serializable]class Result {public int pass,fail;public string[] checks,failures;public string utc;public bool desktopControlled=false,screenCaptured=false,playerLaunched=false;public string visual="UNVERIFIED: edit-mode geometry and callback fixtures, no rendered player";}
 }
}
