using System;
using System.Collections.Generic;
namespace CelestialMaker.Core {
 public static class SaveMigrations {
  // Version 0 is the documented development schema using identical Number pairs.
  // Foreign schemas and unknown future versions are rejected by the validator.
  public static State Upgrade(State state){
   if(state==null)throw new ArgumentException("Missing save state");
   if(state.version==0||state.version==1){
    state.discoveredStories??=new List<string>();state.prestigePurchases??=new List<string>();state.profiles??=new List<AutomationProfile>();state.settings??=new Settings();
    if(state.automation!=null)state.automation.reserves??=new Number[12];foreach(var profile in state.profiles)if(profile!=null)profile.reserves??=new Number[12];
    if(state.offlineReport!=null&&state.offlineReport.requestedSeconds<=0&&state.offlineReport.processedSeconds<=0)state.offlineReport=null;
    // Currency arrays retain their exact values. Spent points are reconstructed
    // once from stable old IDs, so hoarding is never needed for Ascension.
    var e=new ExpansionState{migration="legacy-v"+state.version,prestigeStarted=state.time,ascensionStarted=state.time,genesisStarted=state.time,nextAutomation=state.time+60};
    for(int layer=0;layer<3;layer++){
     var earned=state.prestige[layer].Value;
     for(int node=0;node<4;node++)if(state.prestigePurchases.Contains("p"+layer+"_"+node))earned+=BreakInfinity.BigDouble.Pow(4,node);
     // Frozen v1 talent schedule: t0..t23, three branches costing 1..8.
     if(layer==1&&state.talents!=null)foreach(string id in state.talents)if(id!=null&&id.StartsWith("t")&&int.TryParse(id.Substring(1),out int n)&&n>=0&&n<24)earned+=n%8+1;
     e.historicEarned[layer]=earned;e.historicResets[layer]=state.resets[layer];
     if(layer==0)e.prestigeEarned=earned;if(layer==1)e.ascensionEarned=earned;
    }
    // Retain existing p1/p2 upgrade IDs and talent investments without refunding
    // them again. Their effects stay active alongside the new trees.
    if(state.resets[2]>0)e.laws.Add(new[]{"flux_radiance","flux_density","flux_balance"}[Math.Max(0,Math.Min(2,state.universeLaw))]);
    if(state.settings.theme>0)e.cosmetics.Add("theme-"+state.settings.theme);
    state.expansion=e;state.version=2;
   }
   if(state.offlineReport!=null&&state.offlineReport.requestedSeconds<=0&&state.offlineReport.processedSeconds<=0)state.offlineReport=null;
   state.evolution??=new EvolutionState(); state.discovery??=new DiscoveryState();
   if(state.expansion?.strategies!=null)foreach(var profile in state.expansion.strategies)if(profile!=null)profile.focus??="Balanced";
   state.researchPlan??=new ResearchPlan();
   if(state.evolution.operations!=null)foreach(var p in state.evolution.operations)if(p!=null)p.goals??=new List<GoalOrder>();
   if(state.evolution.beforeReset!=null)state.evolution.beforeReset.goals??=new List<GoalOrder>();
   if(state.settings!=null)state.settings.observatory??=new ObservatoryPreferences();
   if(state.expansion?.universes!=null)foreach(var universe in state.expansion.universes)if(universe!=null)universe.researchPlan??=new ResearchPlan();
   return state;
  }
 }
}

