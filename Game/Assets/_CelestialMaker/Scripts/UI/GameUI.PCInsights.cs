using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using BreakInfinity;
using CelestialMaker.Core;

namespace CelestialMaker {
 public sealed partial class GameUI {
  int pcResearchFilter;
  bool ResearchMatches(Upgrade u)=>Sim.StageOpen(u.stage)&&(pcResearchFilter==0||pcResearchFilter==1&&ResearchReady(u)||pcResearchFilter==2&&S.purchases.Contains(u.id));
  bool ResearchReady(Upgrade u)=>Sim.StageOpen(u.stage)&&!S.purchases.Contains(u.id)&&(u.requires==""||S.purchases.Contains(u.requires))&&S.resources[u.costResource].Value>=BigDouble.Parse(u.cost);

  void ResearchFiltersPc(Upgrade[] defs){
   var row=HRow(list,"Research filters",44);
   var names=new[]{"All research","Available","Purchased research"};
   for(int i=0;i<names.Length;i++){
    int filter=i;var b=PcButton(names[i],row,()=>{pcResearchFilter=filter;pcLastPage="";ShowPc();});
    if(i==pcResearchFilter){b.GetComponent<CosmicPanel>().color=CosmicStyle.Hex("345462");b.GetComponent<CosmicPanel>().edgeColor=CosmicStyle.Cyan;}
   }
   var summary=Label("",list,16,28,CosmicStyle.Muted);
   Bind(()=>summary.text=defs.Count(ResearchReady)+" ready to unlock  /  "+defs.Count(u=>Sim.StageOpen(u.stage)&&S.purchases.Contains(u.id))+" acquired  /  "+defs.Count(u=>Sim.StageOpen(u.stage))+" discovered");
   if(!defs.Any(ResearchMatches))Label(pcResearchFilter==2?"No research acquired yet. Start with an affordable discovery.":"Every discovered research is acquired. A new chapter brings new possibilities.",list,19,90,CosmicStyle.Muted);
  }

  string ProcessStatusPc(int id){
   var g=GameSession.Content.generators[id];
   if(S.owned[id]==0)return "NOT OWNED";
   var capacity=Sim.CachedProductionMultiplier(id)*g.rate*S.owned[id];
   if(g.input>=0&&capacity>0&&Sim.CachedFlow(id)<capacity*.999){
    double ratio=Math.Max(0,Math.Min(1,(Sim.CachedFlow(id)/capacity).ToDouble()));
    return "INPUT LIMITED / "+Math.Floor(ratio*100)+"% output";
   }
   return "RUNNING / full output";
  }

  void LegacyProcessDetailsPc(int id){
   Sim.RefreshRates();var g=GameSession.Content.generators[id];var dialog=ModalPc(g.name,840,620);
   Caption(ProcessStatusPc(id),dialog);
   var capacity=Sim.CachedProductionMultiplier(id)*g.rate*S.owned[id];
   Label("Actual output  "+new Number(Sim.CachedFlow(id))+" "+GameSession.Content.resources[g.output]+" /s\nInstalled capacity  "+new Number(capacity)+" /s\nProduction multiplier  ×"+new Number(Sim.CachedProductionMultiplier(id)),dialog,20,108,CosmicStyle.Cyan);
   string note=S.owned[id]==0?"Buy the first unit to begin production.":"This process uses its full installed capacity.";
   if(g.input>=0){
    int r=g.input;bool limited=S.owned[id]>0&&Sim.CachedFlow(id)<capacity*.999;
    note="Input: "+GameSession.Content.resources[r]+" / Stock "+S.resources[r]+" / Reserve "+S.reserves[r]+"\n\n"+(limited?(S.resources[r].Value<S.reserves[r].Value?"The input reserve is still filling. Release the reserve or increase upstream production.":"Input supply limits output. Expand or resume an upstream producer; review any protected reserve."):note);
   }
   Label(note,dialog,18,140,CosmicStyle.Muted);
   var row=HRow(dialog,"Process diagnosis actions",48);PcButton("Close",row,ClosePcModal);
   if(g.input>=0)Primary("View input producers",row,()=>{ClosePcModal();pcProductionStage=g.input/2;page="Production";pcLastPage="";ShowPc();});
  }

  string CampaignGoalPc()=>Sim.CampaignComplete?"Campaign complete at "+(Sim.Meta.campaignCompletedAt/3600).ToString("0.0")+" h.\nYour cosmos continues; challenges and records are optional.":Sim.AllGenesisNodes?"Complete all Ascension branches and raise each megastructure to stage 6.\nKeep three universes. Pause resets once the trees are complete.":Sim.PreparingGenesis?"Ascension target reached. Pause lower resets; reach Universes and finish each project's first stage.":"Build toward Genesis and its permanent foundations.\nThe Roadmap tracks your campaign goals.";
  void CampaignPc(Transform parent){
   var card=PCard(parent,"Campaign progress");var title=Caption("MAIN CAMPAIGN",card);var progress=Label("",card,19,62,CosmicStyle.Cyan);var goal=Label("",card,17,85,CosmicStyle.Muted);
   Bind(()=>{title.text=Sim.CampaignComplete?"CAMPAIGN COMPLETE / THE COSMOS IS YOURS":"MAIN CAMPAIGN";progress.text="Ascension nodes "+Sim.Meta.ascensionUpgrades.Count+" / 9  •  Genesis foundations "+Sim.Meta.genesisUpgrades.Count+" / 3\nUniverses "+(Sim.Meta.universes.Count+1)+" / 3  •  Megastructure stages "+Sim.Meta.megas.Sum(m=>m.level)+" / 18";goal.text=CampaignGoalPc();});
   Label("Pacing assumes regular decisions and automation while the game runs, including in the background. Offline time advances the same economy. This is a target, not a timer or guarantee.",card,16,85,CosmicStyle.Muted);
  }
  void HorizonPc(Transform parent){
   int stage=HighestStage();var card=PCard(parent,"Next horizon",156);Caption("NEXT HORIZON / "+(stage<5?CosmicStyle.Chapters[stage+1]:"UNIVERSAL GENESIS"),card);
   var value=Label("",card,19,50);
   var track=PcBox("Chapter progress track",card,CosmicStyle.Background,8);var fill=PcBox("Chapter progress",track,CosmicStyle.Cyan);fill.GetComponent<CosmicPanel>().edge=0;
   BindLive(value,()=>{
    double ratio=1;
    if(stage<5){int r=stage*2+1;var target=BigDouble.Pow10((stage+1)*2);ratio=Math.Max(0,Math.Min(1,(S.lifetime[r].Value/target).ToDouble()));value.text=S.lifetime[r]+" / "+new Number(target)+" lifetime "+GameSession.Content.resources[r]+"\n"+(ratio*100).ToString("0.0")+"% of the next chapter";}
    else value.text=Sim.AllGenesisNodes||Sim.CampaignComplete?CampaignGoalPc():ResetCurrencyVisible(2)?"Grow your universe; Genesis creates lasting power.\nCurrent reward: +"+new Number(Sim.PrestigeReward(2))+" "+ResetRules.Currencies[2]:"Develop Ascension and complete the first stage of each megastructure to unlock Genesis.";
    Stretch(fill,Vector2.zero,new Vector2((float)ratio,1),Vector2.zero,Vector2.zero);
   });
  }
 }
}
