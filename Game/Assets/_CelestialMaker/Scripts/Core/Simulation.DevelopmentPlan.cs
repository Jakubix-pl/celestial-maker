using System;
using System.Linq;

namespace CelestialMaker.Core {
 public sealed partial class Simulation {
  public bool ConfigureDevelopmentPlan(string branch){
   if(S.satellite||branch!=""&&(!HasLayer(0)||!new[]{"Energy","Matter","Time"}.Contains(branch)))return false;
   Evolution.developmentPlan=branch;Revision++;return true;
  }
  void RunDevelopmentPurchases(){
   string branch=Evolution.developmentPlan;if(S.satellite||branch==""||S.activeChallenge!="")return;
   S.automation.buy=true;Meta.guidedAutomation=true;Meta.autoBuild=HasLayer(1);
   for(int i=0;i<4;i++)PrestigeUpgrade(0,i);
   foreach(var node in ExpansionContent.Ascension)if(node.branch==branch)BuyMeta(node.id);
   foreach(var node in ExpansionContent.Genesis)BuyMeta(node.id,true);
   if(AllGenesisNodes)foreach(var node in ExpansionContent.Ascension)BuyMeta(node.id);
   if(Evolution.focus!=branch&&FocusUnlocked(branch)&&S.time>=Evolution.focusReadyAt)SelectFocus(branch);
   string[] laws=branch=="Energy"?new[]{"flux_radiance","time_patience"}:branch=="Matter"?new[]{"flux_balance","space_fold"}:new[]{"time_burst","space_open"};
   if(HasLayer(1)&&S.time>=Meta.lawReadyAt&&!Meta.laws.SequenceEqual(laws))SetLaws(laws);
   while(CreateUniverse(branch=="Matter"?2:1)){}
   Meta.autoResearch=HasA("a_time_2");
  }
  void RunDevelopmentReset(){
   if(GoalHoldsResets||S.activeChallenge!=""||AllGenesisNodes&&AllAscensionNodes)return;
   // First discovery of a reset stays a manual decision. The persistent plan
   // restores only previously discovered reset actions after higher resets.
   for(int layer=2;layer>=0;layer--){
    if(Meta.historicResets[layer]<=0||layer==2&&AllGenesisNodes||layer<2&&PreparingGenesis||layer==0&&Meta.prestigeEarned.Value>=ProgressionBalance.PrestigeForAscension)continue;
    if(PrestigeReward(layer)>=1&&Rebirth(layer))break;
   }
  }
 }
}
