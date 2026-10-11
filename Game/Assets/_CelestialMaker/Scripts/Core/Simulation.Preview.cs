using System;
using System.Collections.Generic;
using System.Linq;
using BreakInfinity;
namespace CelestialMaker.Core {
 public sealed partial class Simulation {
  public Number[] PreviewRates(IEnumerable<string> laws,IEnumerable<string> upgrades,int[] links=null,string[] talents=null,Upgrade purchase=null){
   // Only a disposable economy snapshot is evaluated. No time, rewards, respec
   // or writes are applied to the player's state.
   var state=new State{resources=(Number[])S.resources.Clone(),lifetime=(Number[])S.lifetime.Clone(),owned=(long[])S.owned.Clone(),paused=(bool[])S.paused.Clone(),reserves=(Number[])S.reserves.Clone(),prestige=(Number[])S.prestige.Clone(),resets=(int[])S.resets.Clone(),links=links==null?(int[])S.links.Clone():(int[])links.Clone(),purchases=new List<string>(S.purchases),talents=talents==null?new List<string>(S.talents):new List<string>(talents),achievements=new List<string>(S.achievements),completedChallenges=new List<string>(S.completedChallenges)};
   state.autobuyers=(bool[])S.autobuyers.Clone();state.automation=CopyRules(S.automation);state.expansion.guidedAutomation=Meta.guidedAutomation;
   state.expansion.ascensionUpgrades=upgrades.ToList();state.expansion.laws=laws.ToList();state.expansion.genesisUpgrades=new List<string>(Meta.genesisUpgrades);state.expansion.megas=Meta.megas.Select(m=>new MegaProject{level=m.level,building=m.building,work=m.work}).ToArray();
   state.discovery.modules=(int[])Discover.modules.Clone();state.discovery.role=Discover.role;state.discovery.scenario=Discover.scenario;state.satellite=S.satellite;state.universeKind=S.universeKind;state.activeChallenge=S.activeChallenge;state.prestigePurchases=new List<string>(S.prestigePurchases);state.expansion.autoBuild=Meta.autoBuild;state.expansion.records=new List<string>(Meta.records);
   var budget=state.prestige[1].Value;foreach(var id in Meta.ascensionUpgrades)budget+=Array.Find(ExpansionContent.Ascension,u=>u.id==id).cost;foreach(var id in state.expansion.ascensionUpgrades)budget-=Array.Find(ExpansionContent.Ascension,u=>u.id==id).cost;if(budget<0)throw new ArgumentException("Insufficient strategy budget");state.prestige[1]=budget;
   var preview=new Simulation(Data,state);if(purchase!=null&&!preview.Purchase(purchase))return null;preview.RefreshRates();return Enumerable.Range(0,12).Select(i=>new Number(preview.CachedRate(i))).ToArray();
  }
  public bool ImportStrategy(int slot,StrategyProfile profile){
   if(slot<0||slot>=3||profile==null||!FocusNames.Contains(profile.focus)||profile.name==null||profile.name.Length>48||profile.links?.Length!=6||!ValidLaws(profile.laws,3)||profile.upgrades==null)return false;
   if(profile.upgrades.Distinct().Count()!=profile.upgrades.Count||profile.upgrades.Any(id=>!ExpansionContent.Ascension.Any(u=>u.id==id)))return false;
   for(int i=0;i<6;i++)if(profile.links[i]<-1||profile.links[i]>5||profile.links[i]==i)return false;
   Meta.strategies[slot]=profile;return true;
  }
 }
}
