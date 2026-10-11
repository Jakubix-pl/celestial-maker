using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using TMPro;
using BreakInfinity;
using CelestialMaker.Core;
namespace CelestialMaker {
 public sealed partial class GameUI {
  int pcGoalKind,pcGoalProject,pcGoalLevel=4,pcGoalResource,pcGoalAfter,pcLabChoice,pcScenarioSeed=17;
  bool pcGoalHold=true;
  string pcGoalAmount="1e6";
  readonly string[] goalAfterOptions={"Save","Resume","Energy","Matter","Time"};
  PlanExperiment pcLabA,pcLabB;PlanComparison pcLabJob;string pcLabName="";
  TMP_Text pcLabOutput;
  PracticeLesson pcLesson;
  bool pcLabRunning;
  void DiscoveryAutomationPc(){
   var c=PCard(list,"Goal sequences and experiments");Caption("PLAN YOUR NEXT VISIT",c);
   var status=Label("",c,18);Bind(()=>status.text=Sim.Discover.goalStatus);
   var row=HRow(c,"Planning actions",0);PcButton("Goal sequence",row,GoalSequencePc);PcButton("Six-hour laboratory",row,PlanLaboratoryPc);PcButton("Practice mechanics",row,LessonsPc);
  }
  TMP_InputField DiscoveryField(Transform parent,string value,Action<string> changed){
   var box=PcBox("Amount",parent,CosmicStyle.Background,54*PcScale);box.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();var input=box.gameObject.AddComponent<TMP_InputField>();var t=(TextMeshProUGUI)PcText(value,box,18);Stretch(t.rectTransform,Vector2.zero,Vector2.one,new Vector2(14,8),new Vector2(-14,-8));input.textComponent=t;input.textViewport=box;input.characterLimit=40;input.text=value;input.onValueChanged.AddListener(v=>changed(v));return input;
  }
  void GoalSequencePc(){
   var d=ModalPc("Goal sequence",1000,850);PcButton("Close",d,ClosePcModal);
   Label("Up to six steps. Goals protect their stock or prioritize one construction project. Holding resets affects automatic resets only. Save stops automation and protects current home stock; production continues.",d,18);
   Label(Sim.Discover.goalStatus,d,20);var actions=HRow(d,"Goal controls",0);
   var start=Primary("Enable sequence",actions,()=>{Sim.EnableGoals();GoalSequencePc();});start.interactable=Sim.HasLayer(0)&&Sim.Discover.goalIndex<Sim.Discover.goals.Count;
   PcButton("Pause sequence",actions,()=>{Sim.PauseGoals();GoalSequencePc();});
   PcButton("Clear sequence",actions,()=>ConfirmPc("Clear this sequence?","Queued goals will be removed. Your existing production, reserves and other automation remain configured.",()=>{Sim.ClearGoals();GoalSequencePc();},"Clear goals"));
   for(int i=0;i<Sim.Discover.goals.Count;i++){var g=Sim.Discover.goals[i];Label((i<Sim.Discover.goalIndex?"DONE / ":i==Sim.Discover.goalIndex?"NEXT / ":"THEN / ")+Sim.GoalName(g)+" / "+g.after+(g.holdResets?" / holds resets":""),d,18);}
   Caption("ADD A STEP",d);PcButton("Goal type: "+(pcGoalKind==0?"Construction":"Stock"),d,()=>{pcGoalKind=1-pcGoalKind;GoalSequencePc();});
   if(pcGoalKind==0){PcButton("Project: "+ExpansionContent.MegaNames[pcGoalProject],d,()=>{pcGoalProject=(pcGoalProject+1)%3;GoalSequencePc();});PcButton("Target stage: "+pcGoalLevel,d,()=>{pcGoalLevel=pcGoalLevel%6+1;GoalSequencePc();});}
   else{PcButton("Resource: "+GameSession.Content.resources[pcGoalResource],d,()=>{do{pcGoalResource=(pcGoalResource+1)%12;}while(!Sim.StageOpen(pcGoalResource/2));GoalSequencePc();});Label("Target stock (for example 1e6)",d,18);DiscoveryField(d,pcGoalAmount,v=>pcGoalAmount=v);}
   PcButton("Hold automatic resets: "+(pcGoalHold?"Yes":"No"),d,()=>{pcGoalHold=!pcGoalHold;GoalSequencePc();});PcButton("After completion: "+goalAfterOptions[pcGoalAfter],d,()=>{pcGoalAfter=(pcGoalAfter+1)%goalAfterOptions.Length;GoalSequencePc();});
   var feedback=Label("Construction above stage 3 requires Endless horizon. A held reset can delay that unlock: use Pause sequence to resolve it.",d,18,0,CosmicStyle.Muted);
   var add=PcButton("Add goal",d,()=>{try{var order=new GoalOrder{kind=pcGoalKind,project=pcGoalProject,level=pcGoalLevel,resource=pcGoalResource,amount=new Number(BigDouble.Parse(pcGoalAmount)),holdResets=pcGoalHold,after=goalAfterOptions[pcGoalAfter]};if(!Sim.QueueGoal(order)){feedback.text="Cannot add: complete Prestige, use a valid positive target, or clear a full sequence.";return;}GoalSequencePc();}catch{feedback.text="Enter a positive number such as 1e6.";}});add.interactable=Sim.HasLayer(0)&&Sim.Discover.goals.Count<6;
   if(!Sim.HasLayer(0))Label("Complete your first Prestige to enable goal sequences.",d,18);FocusModalPc();
  }
  void PlanLaboratoryPc(){
   var d=ModalPc("Six-hour laboratory",1100,880);PcButton("Close",d,ClosePcModal);
   Label("Compare two independent copies of this moment. Includes offline purchases, reserves, construction and previously enabled resets. Future manual choices are excluded. Calculation can be cancelled; partial results are labeled.",d,18);
   var names=new[]{"Energy plan","Matter plan","Time plan"}.Concat(Sim.Evolution.operations.Where(p=>p.world==0).Select(p=>"Profile: "+p.name)).ToArray();pcLabChoice%=names.Length;
   PcButton("Variant: "+names[pcLabChoice],d,()=>{pcLabChoice=(pcLabChoice+1)%names.Length;pcLabJob?.Cancel();pcLabJob=null;pcLabA=pcLabB=null;PlanLaboratoryPc();});
   var compare=Primary("Compare next 6 hours",d,()=>{
    if(pcLabRunning)return;Sim.SettleUniverses();string json=JsonUtility.ToJson(S);var a=JsonUtility.FromJson<State>(json);var b=JsonUtility.FromJson<State>(json);var candidate=new Simulation(GameSession.Content,b);
    if(pcLabChoice<3)candidate.ConfigureDevelopmentPlan(new[]{"Energy","Matter","Time"}[pcLabChoice]);else{int slot=Sim.Evolution.operations.Select((p,i)=>new{p,i}).Where(x=>x.p.world==0).ElementAt(pcLabChoice-3).i;candidate.ApplyOperations(slot);}
    pcLabName=names[pcLabChoice];pcLabA=new PlanExperiment(GameSession.Content,a);pcLabB=new PlanExperiment(GameSession.Content,b);pcLabJob?.Cancel();pcLabJob=new PlanComparison(pcLabA,pcLabB);StartLabPc();
   });compare.interactable=Sim.HasLayer(0);
   pcLabOutput=Label(pcLabA==null?"Ready. Your live campaign keeps running.":LabSummaryPc(),d,18);
   PcButton("Continue calculation",d,StartLabPc);PcButton("Cancel calculation",d,()=>{pcForecastGeneration++;pcLabJob?.Cancel();pcLabRunning=false;if(pcLabOutput)pcLabOutput.text=LabSummaryPc()+"\nCalculation cancelled. Results are partial.";});FocusModalPc();
  }
  string LabSummaryPc()=>pcLabJob==null?"No comparison started.":"Variant: "+pcLabName+"\n"+pcLabJob.Snapshot;
  void StartLabPc(){if(pcLabRunning||pcLabJob==null||!pcLabJob.Start())return;pcLabRunning=true;int generation=++pcForecastGeneration;StartCoroutine(RunLabPc(generation,pcLabJob));}
  IEnumerator RunLabPc(int generation,PlanComparison job){
   while(pcLabOutput&&generation==pcForecastGeneration&&job.Running){pcLabOutput.text=LabSummaryPc();yield return null;}
   if(generation!=pcForecastGeneration){job.Cancel();yield break;}
   pcLabRunning=false;if(pcLabOutput)pcLabOutput.text=LabSummaryPc()+(job.Error!=""?"\nCalculation failed: "+job.Error:!job.first.Done||!job.second.Done?"\nCalculation paused. Continue to finish.":"\nBoth six-hour scenarios completed. No resources were awarded to your campaign.");
  }
  void ModulePanelPc(Transform parent,int project){
   PcButton("Engineering modules / "+ExpansionContent.MegaNames[project],parent,()=>ModulesPc(project));
  }
  void ModulesPc(int project){
   var d=ModalPc("Engineering / "+ExpansionContent.MegaNames[project],1000,800);PcButton("Close",d,ClosePcModal);
   Label("Output and input tradeoffs apply to Home; transport bonuses affect the network. Select one module per megastructure. Choices persist through resets, but their effects require the structure to reach the unlock stage again. Switching has a one-hour cooldown and does not reset construction work.",d,18);
   for(int i=0;i<4;i++){int choice=i;var card=PCard(d,DiscoveryContent.ModuleNames[project][i]);Label(DiscoveryContent.ModuleNames[project][i],card,24);Label(DiscoveryContent.ModuleEffects[project][i],card,18);int level=i==0?0:i==1?1:i==2?2:4;var button=PcButton(Sim.Discover.modules[project]==i?"Selected":"Select "+DiscoveryContent.ModuleNames[project][i],card,()=>{Sim.SelectModule(project,choice);ModulesPc(project);});button.interactable=Sim.ModuleUnlocked(project,i)&&S.time>=Sim.Discover.moduleReady[project]&&Sim.Discover.modules[project]!=i;Label(!Sim.ModuleUnlocked(project,i)?"Requires stage "+level:S.time<Sim.Discover.moduleReady[project]?"Switch in "+OfflineReport.Duration(Sim.Discover.moduleReady[project]-S.time):"Available",card,17,0,CosmicStyle.Muted);}
   FocusModalPc();
  }
  void ExpeditionDiscoveriesPc(){
   PcButton("Signals and route discoveries",list,()=>{
    var d=ModalPc("Signals and route discoveries",1020,850);PcButton("Close",d,ClosePcModal);Label("Completed expeditions reveal anomalies. Choose a permanent route; pending signals never expire. The new route applies to future launches. Repeat expeditions retain their automation.",d,18);
    bool any=false;for(int i=0;i<8;i++){if(!Sim.Discover.signals[i])continue;any=true;int id=i;var c=PCard(d,GameSession.Content.expeditions[i].name);Label(GameSession.Content.expeditions[i].name,c,24);Label(DiscoveryContent.Anomalies[i],c,18);if(Sim.Discover.routes[i]>0)Label(Sim.RouteDescription(Sim.Discover.routes[i]),c,18);else foreach(int kind in new[]{1,2}){int choice=kind;PcButton(Sim.RouteDescription(kind),c,()=>ConfirmPc("Commit this route?",Sim.RouteDescription(choice)+"\nThis choice is permanent for this campaign. It applies to future launches; your pending signal does not expire.",()=>{Sim.ChooseRoute(id,choice);ClosePcModal();ShowPc();},"Commit route"));}}
    if(!any)Label("Complete and claim an expedition to discover its signal.",d,18);FocusModalPc();
   });
  }
  void UniverseRolePc(Transform parent,int id){PcButton("Specialization / "+DiscoveryContent.Roles[Sim.Meta.universes.Find(u=>u.id==id).role],parent,()=>{
   var d=ModalPc("Universe "+id+" specialization",1000,800);PcButton("Close",d,ClosePcModal);foreach(int r in new[]{0,1,2,3}){int role=r;var c=PCard(d,DiscoveryContent.Roles[r]);Label(DiscoveryContent.Roles[r],c,23);Label(DiscoveryContent.RoleEffects[r],c,18);PcButton("Assign "+DiscoveryContent.Roles[r],c,()=>ConfirmPc("Assign "+DiscoveryContent.Roles[role]+"?",DiscoveryContent.RoleEffects[role]+"\nChanging roles clears any current construction-support delivery.",()=>{Sim.SetUniverseRole(id,role);ClosePcModal();ShowPc();},"Assign role"));}FocusModalPc();});}
  void ChallengeTrialPc(int id){
   Sim.SettleUniverses();var copy=JsonUtility.FromJson<State>(JsonUtility.ToJson(S));var test=new Simulation(GameSession.Content,copy);if(!test.EnterChallenge(id)){ToastPc("Unlock this chapter after Prestige first.");return;}test.Discover.goalsEnabled=false;test.ConfigureDevelopmentPlan("");test.Meta.autoBuild=false;test.S.automation.expeditions=false;Array.Clear(test.S.automation.repeatPrestige,0,3);test.S.automation.buy=true;test.Meta.guidedAutomation=true;
   var d=ModalPc("Readiness trial / "+GameSession.Content.challenges[id].name,1000,800);PcButton("Close",d,ClosePcModal);Label(Sim.ChallengeReadiness(id)+"\nUses guided purchases. Your actual challenge may differ with manual choices. This trial cannot grant campaign rewards.",d,18);var output=Label("Calculating...",d,18);int generation=++pcForecastGeneration;StartCoroutine(RunTrialPc(new PlanExperiment(GameSession.Content,copy),output,id,generation));FocusModalPc();
  }
  IEnumerator RunTrialPc(PlanExperiment trial,TMP_Text output,int id,int generation){var timer=System.Diagnostics.Stopwatch.StartNew();while(output&&generation==pcForecastGeneration&&!trial.Done&&trial.simulation.S.activeChallenge!=""&&timer.Elapsed.TotalSeconds<30){trial.Step(4096,3);output.text=trial.Summary();yield return null;}if(!output||generation!=pcForecastGeneration)yield break;var s=trial.simulation;var c=GameSession.Content.challenges[id];output.text=s.S.activeChallenge==""?"READY / Guided trial completed in approximately "+OfflineReport.Duration(s.S.time-trial.started)+".\n"+s.Discover.challengeReview:(trial.Done?"NOT REACHED IN 6 HOURS":"PARTIAL / calculation time budget reached")+"\nReached "+s.S.resources[c.resource]+" of "+c.target+" "+GameSession.Content.resources[c.resource]+"\n"+s.ConstraintDescription+"\n"+string.Join("\n",s.Diagnose().Select(a=>a.title+": "+a.detail).Take(3));}
  void LessonsPc(){var d=ModalPc("Practice mechanics",940,800);PcButton("Close",d,ClosePcModal);Label("Short exercises use separate practice economies. They do not spend your campaign resources and can be repeated at any time.",d,18);for(int i=0;i<3;i++){int id=i;PcButton((Sim.Discover.lessons[i]?"Completed / ":"")+DiscoveryContent.Lessons[i],d,()=>{pcLesson=new PracticeLesson(GameSession.Content,id);LessonPc("");});}FocusModalPc();}
  void LessonPc(string last){var d=ModalPc(DiscoveryContent.Lessons[pcLesson.lesson],940,800);PcButton("Close",d,ClosePcModal);Label("PRACTICE ECONOMY / step "+Math.Min(3,pcLesson.step+1)+" of 3",d,18,0,CosmicStyle.Cyan);Label(pcLesson.Instruction,d,24);if(last!="")Label(last,d,19);if(!pcLesson.Complete)Primary("Perform practice step",d,()=>{string result=pcLesson.Act();if(pcLesson.Complete){Sim.Discover.lessons[pcLesson.lesson]=true;Sim.Chronicle("lesson-"+pcLesson.lesson,"Practice completed: "+DiscoveryContent.Lessons[pcLesson.lesson],true);}LessonPc(result);});else PcButton("More lessons",d,LessonsPc);FocusModalPc();}
  void ChroniclePc(){
   var d=ModalPc(Sim.CampaignComplete?"Your completed cosmos":"Cosmic chronicle",1100,880);PcButton("Close",d,ClosePcModal);
   Label(Sim.CampaignComplete?"Your three connected universes form a stable cosmos. Completed in "+(Sim.Meta.campaignCompletedAt/3600).ToString("0.0")+" hours. Your campaign continues safely while optional scenarios use separate saves.":"Decisions, blueprints and expedition signals remain part of your history across resets.",d,21);
   Label("Megastructures: "+string.Join(" / ",Sim.Meta.megas.Select(m=>m.level))+"\nRoute discoveries: "+Sim.Discover.routes.Count(r=>r>0)+" / 8\nCompleted challenges: "+S.completedChallenges.Count+" / 12",d,18);
   Label("Engineering: "+string.Join(" / ",Enumerable.Range(0,3).Select(i=>DiscoveryContent.ModuleNames[i][Sim.Discover.modules[i]]))+"\nFocus: "+Sim.ActiveFocus+"\nParallel roles: "+string.Join(" / ",Sim.Meta.universes.Select(u=>DiscoveryContent.Roles[u.role]))+"\nHistoric resets: "+string.Join(" / ",Sim.Meta.historicResets),d,18);
   if(Sim.CampaignComplete){PcButton("Scenario seed: "+pcScenarioSeed,d,()=>{pcScenarioSeed=(pcScenarioSeed+1)%1000;ChroniclePc();});for(int i=1;i<=3;i++){int scenario=i;PcButton("Start or resume / "+DiscoveryContent.Scenarios[i],d,()=>ConfirmPc("Open a separate scenario?",DiscoveryContent.Rules[scenario]+"\nSeed "+pcScenarioSeed+". Produce "+new Number(BigDouble.Pow10(6+pcScenarioSeed%3))+" Matter. Existing scenario progress resumes. The campaign is saved separately.",()=>GameSession.Instance.OpenScenario(scenario,pcScenarioSeed),"Open scenario"));}}
   if(Sim.Discover.chronicle.Count==0)Label("New decisions will appear here. Older saves retain their original roadmap records.",d,18);
   foreach(var e in Sim.Discover.chronicle.AsEnumerable().Reverse().Take(60))Label(OfflineReport.Duration(e.time)+" / "+e.text,d,18);FocusModalPc();
  }
  void DiscoveryOverviewPc(){
   if(Sim.Discover.scenario>0){var c=PCard(list,"Optional scenario");Label(DiscoveryContent.Scenarios[Sim.Discover.scenario]+" / seed "+Sim.Discover.seed,c,24);Label(Sim.ConstraintDescription+"\nGoal: produce "+new Number(BigDouble.Pow10(6+Sim.Discover.seed%3))+" Matter. Current "+S.lifetime[3]+(Sim.Discover.scenarioComplete?"\nCompleted at "+OfflineReport.Duration(Sim.Discover.scenarioFinished):""),c,18);PcButton("Return to campaign",c,()=>GameSession.Instance.ReturnFromScenario());}
   var row=HRow(list,"Discovery actions",0);PcButton("Cosmic chronicle",row,ChroniclePc);PcButton("Practice mechanics",row,LessonsPc);
  }
 }
}
