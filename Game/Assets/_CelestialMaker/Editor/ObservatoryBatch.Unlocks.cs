using System;
using System.Linq;
using UnityEngine;
using TMPro;
using BreakInfinity;
using CelestialMaker.Core;

namespace CelestialMaker.EditorTools {
 public static partial class ObservatoryBatch {
  static bool VisibleCurrency(string currency)=>ui.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.IndexOf(currency,StringComparison.OrdinalIgnoreCase)>=0);
  static void ReadyPrestige(){session.Sim.S.lifetime[0]=BigDouble.Pow10(ProgressionBalance.PrestigeEnergyLog);Call("RefreshPc");Settle();}
  static void UnlockChecks(){
   Check("Prestige paid buyers and progressive navigation",()=>{
    Fixture(false,1,false,1920,1080);var tabs=Get<System.Collections.Generic.Dictionary<string,UnityEngine.UI.Button>>("pcTabs");Require(tabs.Keys.SequenceEqual(new[]{"Overview","Production"}),"Locked category visible");
    Require(!Button("Buy process 1"),"Distant building visible");var sim=session.Sim;sim.S.resources[0]=sim.Cost(1)-sim.Rate(0)*30;Call("ShowPc");Settle();Require(Button("Buy process 1"),"30 second building missing");
    sim.S.lifetime[0]=BigDouble.Pow10(ProgressionBalance.PrestigeEnergyLog);Require(sim.Rebirth(0),"Reset not ready");Call("BuildPc");Require(Get<System.Collections.Generic.Dictionary<string,UnityEngine.UI.Button>>("pcTabs").ContainsKey("Automation"),"Automation not discovered");
    Set("page","Production");Call("ShowPc");Settle();Require(ui.GetComponentsInChildren<UnityEngine.UI.Button>().Count(b=>b.name.StartsWith("Buy process "))==24,"Reset did not reveal every building");Require(!Button("Buy process 23").interactable,"Discovery bypassed chapter gate");
    Call("ProcessDetailsPc",0);Require(!Button("Pause process")&&!Button("Resume process"),"Production toggle remains");Call("ClosePcModal");Set("page","Automation");Call("ShowPc");Settle();Click("Unlock autobuyer 0");Require(sim.S.autobuyers[0]&&sim.S.prestige[0].Value==0&&!Button("Unlock autobuyer 1").interactable,"Paid unlock callback differs from Core");
   });
   Check("Overview geometry moves and reduced motion freezes it",()=>{
    Fixture(true,1,false,1920,1080);Set("page","Overview");Call("ShowPc");Settle();var art=ui.GetComponentInChildren<ObservatoryCosmos>();Require(art,"Missing living artwork");float phase=art.Phase;art.AdvanceMotion(1,false);Require(art.Phase!=phase,"Static artwork");phase=art.Phase;art.AdvanceMotion(1,true);Require(art.Phase==phase,"Reduced motion ignored");
   });
   Check("Autobuyer practice uses a separate economy and retains dismissal",()=>{
    Fixture(true,1,false,1920,1080);var live=session.Sim;live.S.offlineReport=new OfflineReport{id="guide-fixture",active=false,dismissed=true};Set("page","Automation");Call("ShowPc");string before=JsonUtility.ToJson(live.S);Click("Learn about autobuyers");Require(ui.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("PRACTICE ECONOMY / step 1 of 5")),"Missing five-step guide");Require(before==JsonUtility.ToJson(live.S),"Opening the guide changed the campaign");
    for(int step=0;step<4;step++)Click("Perform practice step");Require(before==JsonUtility.ToJson(live.S),"Practice changed live points or ownership");Click("Perform practice step");Require(live.S.settings.observatory.autobuyerGuideSeen,"Completion not retained");live.S.settings.observatory.autobuyerGuideSeen=false;Require(before==JsonUtility.ToJson(live.S),"Guide changed more than its completion preference");Call("ClosePcModal");
   });
   Check("Static orbital geometry is reused during animation",()=>{
    Fixture(true,1,false,1920,1080);Set("page","Overview");Call("ShowPc");Settle();var art=ui.GetComponentInChildren<ObservatoryCosmos>();Require(art&&art.SceneryBuilds>0,"Scenery was not built");int builds=art.SceneryBuilds;for(int frame=0;frame<60;frame++){art.AdvanceMotion(1f/60,false);Canvas.ForceUpdateCanvases();}Require(art.SceneryBuilds==builds,"Static scenery rebuilt while only phase changed");
   });
   Check("Live visibility cache refreshes on scroll without repeating idle geometry work",()=>{
    Fixture(true,1,false,1920,1080);Set("page","Production");Call("ShowPc");Settle();Call("RefreshPc");int count=Get<int>("pcVisibilityCalculations");for(int frame=0;frame<60;frame++)Call("PresentPc");Require(Get<int>("pcVisibilityCalculations")==count,"Idle presentation repeats viewport geometry checks");var scroll=Get<UnityEngine.UI.ScrollRect>("pcScroll");scroll.verticalNormalizedPosition=.5f;Canvas.ForceUpdateCanvases();Call("PresentPc");Require(Get<int>("pcVisibilityCalculations")>count,"Scrolling did not refresh visibility");
   });
   Check("Fresh UI hides unknown balances and currencies without leaving meter gaps",()=>{
    foreach(bool light in new[]{false,true}){
     Fixture(false,1,light,1920,1080);
     Require(!ResourceCell(2).gameObject.activeSelf&&!ResourceCell(3).gameObject.activeSelf,"Fresh header reveals future resources");
     var meters=(RectTransform)ResourceCell(0).parent;Require(((RectTransform)ResourceCell(0)).rect.width>meters.rect.width*.45f,"Hidden meters left blank space");
     foreach(string page in new[]{"Overview","Production","Prestige","Trees","Roadmap","Automation","Constellations","Megastructures","Reality laws","Parallel universes","Achievements","Codex"}){
      Set("page",page);Call("ShowPc");foreach(string currency in ResetRules.Currencies)Require(!VisibleCurrency(currency),page+" reveals "+currency);
     }
     Call("ResetPreviewPc",2,false);Require(!VisibleCurrency(ResetRules.Currencies[2])&&!Button("Confirm reset 2"),"Locked preview reveals award or confirmation");Call("ClosePcModal");
    }
   });
   Check("First reset availability reveals points; spend, higher reset and reload retain discovery",()=>{
    Fixture(false,1,false,1920,1080);Set("page","Prestige");Call("ShowPc");Require(!Button("Review Prestige").interactable,"Locked reset can be reviewed");
    ReadyPrestige();Require(ResourceCell(3).gameObject.activeSelf&&Button("Review Prestige").interactable&&VisibleCurrency("Prestige Points"),"Ready Prestige remained hidden");
    Require(!VisibleCurrency("Ascension Points")&&!VisibleCurrency("Genesis Points"),"Ready Prestige reveals higher currencies");
    Require(session.Sim.Rebirth(0)&&session.Sim.PrestigeUpgrade(0,0),"Could not earn and spend first point");Require(session.Sim.S.prestige[0].Value==0,"Point not spent");Call("ShowPc");Require(ResourceCell(3).gameObject.activeSelf&&VisibleCurrency("Prestige Points"),"Spent balance became hidden");
    var saved=SaveStore.Parse(JsonUtility.ToJson(session.Sim.S));session.Sim=new Simulation(GameSession.Content,saved);Call("BuildPc");Require(ResourceCell(3).gameObject.activeSelf,"Reload lost discovery");
    session.Sim.Meta.prestigeEarned=new BigDouble(ProgressionBalance.PrestigeForAscension);session.Sim.S.lifetime[5]=BigDouble.Parse("1e100");Call("ShowPc");Require(VisibleCurrency("Ascension Points")&&!VisibleCurrency("Genesis Points"),"Ascension discovery boundary wrong");
    Require(session.Sim.Rebirth(1),"Ascension reset failed");Call("BuildPc");Require(ResourceCell(3).gameObject.activeSelf&&VisibleCurrency("Prestige Points")&&VisibleCurrency("Ascension Points"),"Higher reset hid discovered lower currency");
    session.Sim.Meta.ascensionEarned=new BigDouble(ProgressionBalance.AscensionForGenesis);session.Sim.S.lifetime[9]=BigDouble.Parse("1e100");foreach(var mega in session.Sim.Meta.megas)mega.level=1;Call("ShowPc");Require(VisibleCurrency("Genesis Points"),"Ready Genesis remained hidden");
    session.Sim.S.prestige[2]=new BigDouble(1);session.Sim.S.resets[2]=1;session.Sim.Meta.historicEarned[2]=new BigDouble(1);session.Sim.S.prestige[2]=BigDouble.Zero;Call("ShowPc");Require(VisibleCurrency("Genesis Points"),"Spent Genesis became hidden");
   });
   Check("Header discovery follows selected economy, imported balance and historical metadata",()=>{
    Fixture(false,1,false,1920,1080);session.Sim.S.prestige[0]=new BigDouble(4);session.Sim.S.resources[2]=new BigDouble(3);Call("RefreshPc");Require(ResourceCell(2).gameObject.activeSelf&&ResourceCell(3).gameObject.activeSelf,"Imported positive balance hidden");
    session.Sim.S.prestige[0]=BigDouble.Zero;session.Sim.Meta.historicEarned[0]=new BigDouble(4);Call("RefreshPc");Require(ResourceCell(3).gameObject.activeSelf,"Historical discovery lost");
    Fixture(true,1,false,1920,1080);Set("pcWorld",1);Call("ShowPc");var local=session.Sim.Universe(1);for(int i=2;i<12;i++){local.S.resources[i]=BigDouble.Zero;local.S.lifetime[i]=BigDouble.Zero;}local.S.lifetime[1]=BigDouble.Zero;Call("RefreshPc");Require(!ResourceCell(2).gameObject.activeSelf&&ResourceCell(3).gameObject.activeSelf,"Parallel header used home resource unlock or lost home currency");
    Set("pcWorld",0);Call("ShowPc");Require(ResourceCell(2).gameObject.activeSelf,"Home resource stayed hidden");
   });
  }
  static void RenderUnlocks(){
   foreach(bool light in new[]{false,true}){
    string theme=light?"dawn":"night";
    Fixture(false,1,light,1920,1080);Shot("unlocks-"+theme+"-fresh-production");Set("page","Prestige");Call("ShowPc");Shot("unlocks-"+theme+"-fresh-resets");
    ReadyPrestige();Shot("unlocks-"+theme+"-prestige-ready");session.Sim.Rebirth(0);session.Sim.PrestigeUpgrade(0,0);Call("ShowPc");Shot("unlocks-"+theme+"-prestige-spent");
    session.Sim.Meta.prestigeEarned=new BigDouble(ProgressionBalance.PrestigeForAscension);session.Sim.S.lifetime[5]=BigDouble.Parse("1e100");Call("ShowPc");Shot("unlocks-"+theme+"-ascension-ready");
    Fixture(true,1,light,1920,1080);Set("page","Prestige");Call("ShowPc");Shot("unlocks-"+theme+"-late-resets");
    Fixture(false,2,light,1280,720);Set("page","Prestige");Call("ShowPc");Shot("unlocks-"+theme+"-fresh-200");
   }
  }
 }
}
