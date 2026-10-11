using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BreakInfinity;
using CelestialMaker.Core;

namespace CelestialMaker {
 public sealed partial class GameUI {
  int pcFlowResource,pcForecastGeneration;
  void DevelopmentPlanPc(){
   var card=PCard(list,"Development plan");Caption("DEVELOPMENT PLAN / SIX-HOUR VISITS",card);
   Label("Develop your chosen strategy between visits. The plan buys development, restores production and builds unlocked cosmic systems. Protected stock and research queue priorities stay in effect.",card,18,0,CosmicStyle.Muted);
   var state=Label("",card,20);Bind(()=>state.text="Plan: "+(S.evolution.developmentPlan==""?"Off":S.evolution.developmentPlan)+" / "+(Sim.CampaignComplete?"Campaign complete":S.evolution.developmentPlan!=""?"Active / next decision in "+OfflineReport.Duration(Math.Max(0,Sim.Meta.nextAutomation-S.time)):Sim.HasLayer(0)?"Ready to configure":"Complete your first Prestige"));
   Label("Runs online and offline. Each reset type needs your first manual confirmation; later resets run automatically. Stop the plan to take manual control.",card,18,0,CosmicStyle.Warning);
   var row=HRow(card,"Development choices",0);foreach(string option in new[]{"Energy","Matter","Time"}){string branch=option;var button=PcButton("Plan "+branch,row,()=>ConfirmPc("Enable "+branch+" development?","This plan spends earned development points, manages research/routing, chooses "+branch+" laws and focus, and repeats previously discovered resets. Resources and lower-layer progress can reset while you are away.\n\nFirst-time resets still wait for you. Manual reserves stay protected. You can stop the plan in Automation.",()=>{Sim.ConfigureDevelopmentPlan(branch);ShowPc();},"Enable development plan"));button.interactable=Sim.HasLayer(0);}
   if(S.evolution.developmentPlan!="")Label("Stop disables automatic spending, construction, expeditions, transfers and resets in every economy.",card,17,0,CosmicStyle.Muted);
   if(S.evolution.developmentPlan!="")PcButton("Stop plan and automatic resets",card,()=>{Sim.StopAllAutomation();ShowPc();});
  }
  void AdvicePc(Transform parent,Simulation sim){
   Caption("PROGRESS DIAGNOSTICS",parent);
   foreach(var advice in sim.Diagnose()){
    var a=advice;var c=PCard(parent,a.title);Label(a.title,c,22);Label(a.detail,c,18,0,CosmicStyle.Muted);
    PcButton("Inspect / "+a.route,c,()=>{if(a.resource>=0)pcFlowResource=a.resource;if(a.process>=0)pcSelectedProcess=a.process;NavigatePc(a.route);});
   }
  }
  void ReturnBriefPc(Transform parent){
   var c=PCard(parent,"Return briefing");Caption("WELCOME BACK TO YOUR COSMOS",c);
   var prefs=S.settings.observatory;string target=string.IsNullOrEmpty(prefs.goal)?CampaignGoalPc():"Pinned goal: "+(Sim.FindUpgrade(prefs.goal)?.name??prefs.goal)+" / Universe "+prefs.goalWorld;
   Label(target,c,20);var report=S.offlineReport;
   if(report!=null){
    var unlocked=Enumerable.Range(0,6).Where(i=>report.stagesAfter[i]&&!report.stagesBefore[i]).Select(i=>CosmicStyle.Chapters[i]);
    Label("New chapters: "+(unlocked.Any()?string.Join(", ",unlocked):"none")+"\nDiscoveries "+report.discoveries.Count+" / Stories "+report.stories.Count+" / Completed expeditions "+report.finishedExpeditions.Sum(),c,18,0,CosmicStyle.Muted);
   }
   var actions=HRow(c,"Return actions",0);PcButton("What needs attention?",actions,()=>{var d=ModalPc("Your next decisions",940,800);AdvicePc(d,Sim);PcButton("Close",d,ClosePcModal);FocusModalPc();});PcButton("Explain this view",actions,ExplainViewPc);
   if(S.evolution.hasBeforeReset)PcButton("Review rules from before the last reset",c,RestoreRulesPc);
  }
  void ExplainViewPc(){
   string title=PageTitlePc();var d=ModalPc("Guide / "+title,900,720);
   string help=page=="Overview"?"Your next decision and diagnostics explain the current economy. The cosmos illustration changes as chapters and projects develop. A pinned goal is retained between visits.":page=="Analytics"?"Choose a discovered resource. Producers appear on the left, the stock and reserve in the middle, and consumers on the right. Net output is production minus consumption. Follow an input-limited process upstream.":page=="Automation"?"READY means the next decision can execute. SAVING means stock is being protected for a target. LOCKED rules retain their intent until prerequisites return. Development Plan manages the selected strategy between visits and repeats only previously discovered resets. Review its spending and reset rules before enabling it or applying a profile.":page=="Prestige"?"A reset exchanges the listed losses for lasting rewards. Recovery forecasts use a private copy of your economy, with the stated automation settings. Historical runs record observed recovery separately.":page=="Trees"?"Branches change production, consumption and automation. Strategic focus also changes construction and transport with a tradeoff. Compare profiles using the same installed economy.":page=="Research"||page=="Upgrades"?"Expand a row for requirements and a production comparison. A research plan includes prerequisites. Guided routing buys basic research after Prestige when the queue is empty. Automatic queue execution requires Temporal workshop after Ascension. Both protect reserves.":page=="Megastructures"?"Each project consumes three resource chains. The planner funds the least-developed idle project. Strategic focus changes work speed; all construction continues offline.":page=="Parallel universes"?"Each universe has separate stock and production. Transfers move existing stock with losses and never count as lifetime production. Strategic focus changes their cadence or efficiency.":"Select a discovered item to inspect its requirements and effects. Global development is shared; Production, Research and Flows use the selected economy. Ctrl+K finds unlocked content.";
   Label(help,d,20);Label("Available now: "+(Sim.HasLayer(1)?"Ascension strategies and cosmic projects":Sim.HasLayer(0)?"Prestige development":"your first production chain")+". Undiscovered currencies stay hidden.",d,18,0,CosmicStyle.Muted);PcButton("Close",d,ClosePcModal);FocusModalPc();
  }
  void OperationsProfilesPc(){
   Caption("OPERATIONS PROFILES",list);Label("Stores research plans, protected stock, purchase order, routing and reset rules. Applying a profile is explicit; locked rules wait for their prerequisites.",list,18,0,CosmicStyle.Muted);
   var rows=HRow(list,"Save operations profiles",0);
   foreach(int slot in new[]{1,2,3}){int n=slot;PcButton("Save operations "+n,rows,()=>{Sim.SaveOperations(0,"Operations "+n);ShowPc();});}
   if(S.evolution.hasBeforeReset)PcButton("Review pre-reset configuration",list,RestoreRulesPc);
   for(int i=0;i<S.evolution.operations.Count;i++){int index=i;var p=S.evolution.operations[i];var row=HRow(list,"Stored operations "+i,0);PcButton("Review "+p.name,row,()=>ConfirmPc(p.name,OperationsSummary(p),()=>{Sim.ApplyOperations(index);ShowPc();},"Apply saved rules"));PcButton("Remove "+p.name,row,()=>{S.evolution.operations.RemoveAt(index);ShowPc();});}
  }
  string OperationsSummary(OperationsProfile p)=>"Goal sequence: "+p.goals.Count+" steps / "+(p.goalsEnabled?"enabled":"paused")+"\n"+"Development Plan: "+(p.developmentPlan==""?"Off":p.developmentPlan+" / repeats discovered resets")+"\nEconomy: "+p.world+"\nResearch plan: "+p.research.queue.Count+" entries\nAutomatic purchases: "+p.rules.buy+" / Guided routing: "+p.guided+"\nAutomatic research: "+(p.research.automatic||p.researchAll)+" / Construction: "+p.construction+"\nPurchase order: "+string.Join(" → ",p.rules.priority.Take(3).Select(i=>GameSession.Content.generators[i].name))+"\nNext research: "+(p.research.queue.Count==0?"none":string.Join(", ",p.research.queue.Take(3).Select(id=>Sim.FindUpgrade(id)?.name??id)))+"\nProtected stock: "+(p.rules.reserves.All(x=>x.Value==0)?"none":string.Join(", ",Enumerable.Range(0,12).Where(i=>p.rules.reserves[i].Value>0).Select(i=>GameSession.Content.resources[i]+" "+p.rules.reserves[i])))+"\nAutomatic resets: "+string.Join(", ",Enumerable.Range(0,3).Where(i=>p.rules.repeatPrestige[i]).Select(i=>ResetRules.Names[i]))+"\n\nEnabled reset rules may reset progress once their unlock and reward conditions are met. Locked rules wait. Reserves are exact saved amounts; review them after a reset.";
  void RestoreRulesPc(){var p=S.evolution.beforeReset;if(!S.evolution.hasBeforeReset)return;ConfirmPc("Restore pre-reset rules?",OperationsSummary(p),()=>{Sim.RestoreResetOperations();ShowPc();},"Apply saved rules");}
  void FocusPc(Transform parent){
   var c=PCard(parent,"Strategic focus");Caption("CONSTRUCTION & EXCHANGE / "+Sim.ActiveFocus.ToUpperInvariant(),c);Label(Simulation.FocusDescription(Sim.ActiveFocus),c,18);
   Label("Each focus requires its first Ascension branch node. The selection survives resets; its benefits wait until that node is owned again. Switching has a 10-minute cooldown.",c,17,0,CosmicStyle.Muted);
   foreach(string focus in Simulation.FocusNames){string f=focus;var b=PcButton(f+(Sim.ActiveFocus==f?" / active":" / preview"),c,()=>{var preview=new Simulation(GameSession.Content,JsonUtility.FromJson<State>(JsonUtility.ToJson(S)));preview.Evolution.focus=f;string speeds=string.Join("\n",Enumerable.Range(0,3).Select(i=>ExpansionContent.MegaNames[i]+": "+(Sim.Meta.megas[i].building?OfflineReport.Duration(Sim.MegaSeconds(i))+" now → "+OfflineReport.Duration(preview.MegaSeconds(i))+" with "+f:"not building")));ConfirmPc("Focus / "+f,Simulation.FocusDescription(f)+"\n\n"+speeds+"\nExisting transfer dates will never move earlier on a switch.",()=>{if(!Sim.SelectFocus(f))ToastPc("Focus is locked or its switch cooldown is active.");ShowPc();},"Apply focus");});Bind(()=>b.interactable=Sim.FocusUnlocked(f)&&S.time>=S.evolution.focusReadyAt&&Sim.ActiveFocus!=f);}
   var remaining=Label("",c,17,0,CosmicStyle.Muted);Bind(()=>remaining.text=S.time<S.evolution.focusReadyAt?"Switch available in "+OfflineReport.Duration(S.evolution.focusReadyAt-S.time):"Ready to compare another focus.");
  }
  void FlowMapPc(){
   var sim=EconomyPc;sim.RefreshRates();var resources=Enumerable.Range(0,12).Where(r=>sim.StageOpen(r/2)).ToArray();if(!resources.Contains(pcFlowResource))pcFlowResource=resources[0];
   Label("Select a resource to trace its producers, protected stock and consumers. Links use actual engine rates for "+WorldNamePc+".",list,18,0,CosmicStyle.Muted);
   var tabs=HRow(list,"Resource flow selectors",0);foreach(int resource in resources){int r=resource;var b=PcButton(GameSession.Content.resources[r],tabs,()=>{pcFlowResource=r;ShowPc();});EmphasizePc(b,pcFlowResource==r);}
   int selected=pcFlowResource;var map=HRow(list,"Interactive production flow",0,12);map.GetComponent<ObservatoryFlow>().cellWidth=240*PcScale;
   var producers=PCard(map,"Flow producers");Caption("01 / PRODUCERS",producers);
   var stock=PCard(map,"Flow resource");Caption("02 / "+GameSession.Content.resources[selected].ToUpperInvariant(),stock);var totals=Label("",stock,18);BindLive(totals,()=>{var e=EconomyPc;totals.text="Stock "+e.S.resources[selected]+"\nGross +"+new Number(e.CachedGross(selected))+" /s\nConsumed -"+new Number(e.CachedSpend(selected))+" /s\nNet "+new Number(e.CachedRate(selected))+" /s\nProtected "+new Number(e.ProtectedStock(selected));});
   PcButton("Review reserves & rules",stock,()=>NavigatePc("Automation"));
   var consumers=PCard(map,"Flow consumers");Caption("03 / CONSUMERS",consumers);
   foreach(int i in Enumerable.Range(0,24).Where(i=>sim.Open(GameSession.Content.generators[i])&&sim.S.owned[i]>0)){
    int id=i;var g=GameSession.Content.generators[i];if(g.output!=selected&&g.input!=selected)continue;bool source=g.output==selected;var parent=source?producers:consumers;
    var b=PcButton(g.name,parent,()=>{pcSelectedProcess=id;NavigatePc("Production");ProcessDetailsPc(id);});var status=Label("",parent,17,0,CosmicStyle.Muted);Bind(()=>status.text=ProcessStatePc(EconomyPc,id)+" / "+new Number(source?EconomyPc.CachedFlow(id):EconomyPc.CachedConsumption(id))+" /s");
    if(!source){int output=g.output;PcButton("Follow → "+GameSession.Content.resources[output],parent,()=>{pcFlowResource=output;ShowPc();});}
   }
   if(!Enumerable.Range(0,24).Any(i=>sim.S.owned[i]>0&&GameSession.Content.generators[i].output==selected))Label("No installed producers.",producers,18);
   if(!Enumerable.Range(0,24).Any(i=>sim.S.owned[i]>0&&GameSession.Content.generators[i].input==selected))Label("No installed consumers. Purchases and construction can still spend this stock.",consumers,18);
   AdvicePc(list,sim);
  }
  void RecoveryPanelPc(Transform parent,int layer){
   var c=PCard(parent,"Reset recovery");Caption("RECOVERY / REAL ECONOMY SIMULATION",c);Label("Compare the highest discovered resource currently being produced. A forecast measures recovery of its gross output, sampled every simulated minute, up to 48 hours. Further manual purchases and resets are excluded.",c,18,0,CosmicStyle.Muted);
   var output=Label("Choose a scenario to calculate. No rewards or time are applied to your save.",c,18);var row=HRow(c,"Recovery scenarios",0);
   PcButton("Forecast retained rules",row,()=>StartRecoveryPc(output,layer,false));PcButton("Forecast restored rules",row,()=>StartRecoveryPc(output,layer,true));
   foreach(var run in S.evolution.runs.Where(r=>r.layer==layer).Reverse().Take(3))Label("Previous run "+OfflineReport.Duration(run.duration)+" / reward "+run.reward+"\n"+GameSession.Content.resources[run.resource]+" gross "+run.output+" / recovered: "+(run.recoveredAt<0?"not observed before the next reset":OfflineReport.Duration(run.recoveredAt-run.at)),c,17,0,CosmicStyle.Muted);
  }
  void StartRecoveryPc(TMP_Text label,int layer,bool restore){
   int generation=++pcForecastGeneration;var copy=JsonUtility.FromJson<State>(JsonUtility.ToJson(S));var forecast=new ResetForecast(GameSession.Content,copy,layer);
   if(restore&&forecast.eligible){forecast.simulation.RestoreResetOperations();Array.Clear(copy.automation.repeatPrestige,0,3);forecast.simulation.ConfigureDevelopmentPlan("");}
   label.text="Calculating on an isolated copy...";StartCoroutine(CalculateRecoveryPc(label,forecast,generation,restore));
  }
  IEnumerator CalculateRecoveryPc(TMP_Text label,ResetForecast forecast,int generation,bool restore){
   var wall=System.Diagnostics.Stopwatch.StartNew();while(label&&generation==pcForecastGeneration&&!forecast.done&&wall.Elapsed.TotalSeconds<4){var frame=System.Diagnostics.Stopwatch.StartNew();do{forecast.Step();}while(!forecast.done&&frame.Elapsed.TotalMilliseconds<4);label.text="Simulated "+OfflineReport.Duration(forecast.simulation.S.time-forecast.started)+" / 48h";yield return null;}
   if(!label||generation!=pcForecastGeneration)yield break;
   label.text=!forecast.eligible?"This reset is not available.":forecast.recovered>=0?"Estimated recovery: "+OfflineReport.Duration(forecast.recovered):forecast.done?"Output did not recover within the 48-hour scenario.":"Calculation budget reached. Simulated "+OfflineReport.Duration(forecast.simulation.S.time-forecast.started)+"; recovery remains unknown.";
   label.text+="\nTarget: "+forecast.target+" "+GameSession.Content.resources[forecast.resource]+" /s gross. "+(restore?"Pre-reset operations restored; locked rules wait.":"Only rules retained by the reset.")+" No additional reset or manual purchase.";
  }
  void CheckpointsPc(){
   var d=ModalPc("Restore points",980,820);Label("Up to 12 local restore points. Created before imports, resets and New Game. Frequent Prestige checkpoints are limited to one per 10 simulated minutes. Restoring first retains the current state.",d,18);
   PcButton("Create restore point now",d,()=>{GameSession.Instance.CreateCheckpoint("manual");CheckpointsPc();});
   foreach(var checkpoint in SaveStore.Checkpoints()){
    var p=checkpoint;var c=PCard(d,p.file);Label(p.utc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")+" / "+p.reason,c,19);
    if(p.state==null){Label("Unavailable: "+p.error,c,17,0,CosmicStyle.Warning);continue;}
    Label("Cosmos time "+OfflineReport.Duration(p.state.time)+" / Historical resets "+string.Join(" / ",p.state.expansion.historicResets)+"\nMegastructure stages "+p.state.expansion.megas.Sum(m=>m.level)+" / Universes "+(p.state.expansion.universes.Count+1),c,18);
    PcButton("Review restore",c,()=>ConfirmPc("Restore this point?","Saved "+p.utc.ToLocalTime()+"\nCosmos time "+OfflineReport.Duration(p.state.time)+"\nThis replaces current progress. A restore point of the current state is created first. Elapsed real time since the old point is not awarded again.",()=>{try{GameSession.Instance.RestoreCheckpoint(p.file);}catch(Exception e){ToastPc("Restore failed: "+e.Message);}},"Restore progress"));
   }
   PcButton("Close",d,ClosePcModal);FocusModalPc();
  }
  void LivingCosmosPc(){
   var card=PCard(list,"Living cosmos");Caption("YOUR OBSERVATORY",card);var host=PcBox("Evolving cosmos",card,CosmicStyle.Background,220);var drawing=new GameObject("Cosmic progression geometry",typeof(RectTransform));drawing.transform.SetParent(host,false);Stretch((RectTransform)drawing.transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var art=drawing.AddComponent<ObservatoryCosmos>();art.raycastTarget=false;
   var caption=Label("",card,18);Action refresh=()=>{int stage=HighestStage();int[] levels=Sim.Meta.megas.Select(m=>m.level).ToArray();int style=S.evolution.cosmosStyle;if(style==1&&stage<3||style==2&&!Sim.HasLayer(2))style=0;art.Configure(stage,levels,Sim.Meta.universes.Count,style,CosmicStyle.Light,Enumerable.Range(0,3).Select(i=>Sim.Meta.megas[i].building?(int)Math.Min(100,100*Sim.Meta.megas[i].work/Sim.MegaWork(i)):0).ToArray());caption.text=CosmicStyle.Chapters[stage]+" / "+S.achievements.Count+" discoveries"+(Sim.HasLayer(1)?" / Megastructure stages "+levels.Sum():"");};refresh();Bind(refresh);
   var buttons=HRow(card,"Cosmos appearance",0);foreach(int i in new[]{0,1,2}){int style=i;var b=PcButton(new[]{"Orbital","Stellar","Genesis"}[i],buttons,()=>{S.evolution.cosmosStyle=style;ShowPc();});b.interactable=i==0||i==1&&HighestStage()>=3||i==2&&Sim.HasLayer(2);Hint(b,i==1?"Earned on discovering Stars.":i==2?"Earned on completing Genesis.":"Available from the first particles.");}
   PcButton("Read cosmic history",card,()=>NavigatePc("Codex"));
  }
 }
}
