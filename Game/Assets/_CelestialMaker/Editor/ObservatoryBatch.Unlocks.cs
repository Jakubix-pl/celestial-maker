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
