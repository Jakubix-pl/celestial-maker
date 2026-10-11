using System;
using System.Collections.Generic;
using System.Linq;
using BreakInfinity;

namespace CelestialMaker.Core {
 [Serializable] public sealed class EvolutionState {
  public string focus="Balanced",developmentPlan="";
  public double focusReadyAt;
  public int cosmosStyle;
  public List<ResetRun> runs=new List<ResetRun>();
  public List<OperationsProfile> operations=new List<OperationsProfile>();
  public bool hasBeforeReset; public OperationsProfile beforeReset=new OperationsProfile();
 }
 [Serializable] public sealed class ResetRun {
  public int layer,resource; public double at,duration,recoveredAt=-1;
  public Number reward,output; public string focus="Balanced";
 }
 [Serializable] public sealed class OperationsProfile {
  public List<GoalOrder> goals=new List<GoalOrder>(); public bool goalsEnabled; public int goalIndex; public string developmentPlan=""; public string name="Operations"; public int world;
  public AutomationProfile rules=new AutomationProfile{priority=Enumerable.Range(0,24).ToArray()};
  public ResearchPlan research=new ResearchPlan();
  public bool guided=true,researchAll,construction;
 }
 public sealed class ObservatoryAdvice {
  public string title,detail,route; public int process=-1,resource=-1;
 }
 public sealed class RuleStatus {
  public string state,detail; public RuleStatus(string state,string detail){this.state=state;this.detail=detail;}
  public override string ToString()=>state+" / "+detail;
 }
 public sealed partial class Simulation {
  public EvolutionState Evolution=>S.evolution;
  public static readonly string[] FocusNames={"Balanced","Energy","Matter","Time"};
  public static string FocusDescription(string focus)=>focus=="Energy"?"Dyson work +20%; Forge work -10%. Energy transfers send 6% of spare stock; particle transfers send 4%.":focus=="Matter"?"Forge work +20%; Dyson work -10%. Transfers retain 90% instead of 80%; interval 75 minutes.":focus=="Time"?"Loom work +20%; Dyson work -35%. Transfers every 45 minutes; retain 70% instead of 80%.":"Standard construction. Transfer 5% of spare stock hourly; retain 80%.";
  public bool FocusUnlocked(string focus)=>focus=="Balanced"||HasA("a_"+focus.ToLowerInvariant()+"_1");
  public bool SelectFocus(string focus){if(S.satellite||!FocusNames.Contains(focus)||!FocusUnlocked(focus)||S.time+1e-7<Evolution.focusReadyAt)return false;SettleUniverses();Evolution.focus=focus;Evolution.focusReadyAt=S.time+600;foreach(var t in Meta.transfers)t.next=Math.Max(t.next,S.time+TransferInterval);Revision++;return true;}
  public string ActiveFocus=>FocusUnlocked(Evolution.focus)?Evolution.focus:"Balanced";
  public double FocusWork(int project)=>ActiveFocus=="Energy"?(project==0?1.2:project==1?.9:1):ActiveFocus=="Matter"?(project==1?1.2:project==0?.9:1):ActiveFocus=="Time"?(project==2?1.2:project==0?.65:1):1;
  public double TransferInterval=>(ActiveFocus=="Matter"?4500:ActiveFocus=="Time"?2700:3600)*(Module(2)==2?.8:1);
  public double TransferRetention=>DiscoveryTransferRetention;
  public double TransferFraction(int resource)=>(ActiveFocus=="Energy"?(resource==0?.06:.04):.05)*(Module(0)==2?1.5:1);
  public BigDouble ProtectedStock(int resource)=>EffectiveReserve(resource);
  public static AutomationProfile CopyRules(AutomationProfile p)=>new AutomationProfile{name=p.name,buy=p.buy,expeditions=p.expeditions,priority=(int[])p.priority.Clone(),reserves=(Number[])p.reserves.Clone(),prestigeThreshold=p.prestigeThreshold,repeatPrestige=(bool[])p.repeatPrestige.Clone()};
  public bool SaveOperations(int world,string name){
   if(name==null||name.Length>48||world<0||world>Meta.universes.Count)return false;
   var e=Universe(world);var p=new OperationsProfile{goals=e.Discover.goals.Select(CopyGoal).ToList(),goalsEnabled=e.Discover.goalsEnabled,goalIndex=e.Discover.goalIndex,developmentPlan=e.Evolution.developmentPlan,name=name,world=world,rules=CopyRules(e.S.automation),research=new ResearchPlan{queue=new List<string>(e.S.researchPlan.queue),automatic=e.S.researchPlan.automatic},guided=e.Meta.guidedAutomation,researchAll=e.Meta.autoResearch,construction=e.Meta.autoBuild};
   p.rules.reserves=(Number[])e.S.reserves.Clone();var old=Evolution.operations.FindIndex(x=>x.world==world&&x.name==name);
   if(old>=0)Evolution.operations[old]=p;else{if(Evolution.operations.Count>=12)return false;Evolution.operations.Add(p);}return true;
  }
  OperationsProfile CaptureOperations()=>new OperationsProfile{goals=Discover.goals.Select(CopyGoal).ToList(),goalsEnabled=Discover.goalsEnabled,goalIndex=Discover.goalIndex,developmentPlan=Evolution.developmentPlan,name="Before last reset",rules=new AutomationProfile{name=S.automation.name,buy=S.automation.buy,expeditions=S.automation.expeditions,priority=(int[])S.automation.priority.Clone(),reserves=(Number[])S.reserves.Clone(),prestigeThreshold=S.automation.prestigeThreshold,repeatPrestige=(bool[])S.automation.repeatPrestige.Clone()},research=new ResearchPlan{queue=new List<string>(S.researchPlan.queue),automatic=S.researchPlan.automatic},guided=Meta.guidedAutomation,researchAll=Meta.autoResearch,construction=Meta.autoBuild};
  public bool RestoreResetOperations(){if(!Evolution.hasBeforeReset)return false;ApplyOperationsTo(this,Evolution.beforeReset);return true;}
  static GoalOrder CopyGoal(GoalOrder g)=>new GoalOrder{kind=g.kind,project=g.project,level=g.level,resource=g.resource,amount=g.amount,holdResets=g.holdResets,after=g.after};
  static void ApplyOperationsTo(Simulation e,OperationsProfile p){e.Discover.goals=p.goals.Select(CopyGoal).ToList();e.Discover.goalsEnabled=!e.S.satellite&&p.goalsEnabled;e.Discover.goalIndex=p.goalIndex;e.Evolution.developmentPlan=e.S.satellite?"":p.developmentPlan;e.S.automation=CopyRules(p.rules);e.S.reserves=(Number[])p.rules.reserves.Clone();e.S.researchPlan=new ResearchPlan();foreach(var id in p.research.queue)e.QueueResearch(id);e.S.researchPlan.automatic=p.research.automatic;e.Meta.guidedAutomation=p.guided;e.Meta.autoResearch=p.researchAll;e.Meta.autoBuild=!e.S.satellite&&p.construction;}
  public bool ApplyOperations(int index){
   if(index<0||index>=Evolution.operations.Count)return false;var p=Evolution.operations[index];if(p.world>Meta.universes.Count)return false;
   EditUniverse(p.world,e=>ApplyOperationsTo(e,p));return true;
  }
  void ValidateEvolution(){
   var e=Evolution;if(e==null||e.developmentPlan==null||e.developmentPlan!=""&&!new[]{"Energy","Matter","Time"}.Contains(e.developmentPlan)||!FocusNames.Contains(e.focus)||e.cosmosStyle<0||e.cosmosStyle>2||e.runs==null||e.runs.Count>24||e.operations==null||e.operations.Count>12||!FiniteTime(e.focusReadyAt))throw new ArgumentException("Invalid Observatory evolution");
   foreach(var r in e.runs)if(r==null||r.layer<0||r.layer>2||r.resource<0||r.resource>11||!FiniteTime(r.at)||!FiniteTime(r.duration)||r.recoveredAt< -1||double.IsNaN(r.recoveredAt)||double.IsInfinity(r.recoveredAt)||!ValidHistoryNumber(r.output)||!ValidHistoryNumber(r.reward)||!FocusNames.Contains(r.focus))throw new ArgumentException("Invalid reset history");
   foreach(var p in e.operations.Concat(!e.hasBeforeReset?Array.Empty<OperationsProfile>():new[]{e.beforeReset})){if(p==null||p.goals==null||p.goals.Count>6||p.goals.Any(g=>!ValidGoal(g))||p.goalIndex<0||p.goalIndex>p.goals.Count||p.name==null||p.name.Length>48||p.developmentPlan==null||p.developmentPlan!=""&&!new[]{"Energy","Matter","Time"}.Contains(p.developmentPlan)||p.world<0||p.world>2)throw new ArgumentException("Invalid operations profile");ValidateProfile(p.rules);ValidateResearchPlan(p.research);foreach(var n in p.rules.reserves)if(n.mantissa<0||double.IsNaN(n.mantissa)||double.IsInfinity(n.mantissa)||Math.Abs(n.exponent)>1000000000)throw new ArgumentException("Invalid profile reserve");}
  }
  static bool ValidHistoryNumber(Number n)=>n.mantissa>=0&&!double.IsNaN(n.mantissa)&&!double.IsInfinity(n.mantissa)&&Math.Abs((double)n.exponent)<=1000000000;
  static bool FiniteTime(double t)=>t>=0&&!double.IsInfinity(t)&&!double.IsNaN(t);
  public int RecoveryResource(){RefreshRates();for(int r=11;r>=0;r--)if(StageOpen(r/2)&&gross[r]>0)return r;return 0;}
  void RememberReset(int layer,BigDouble reward){
   Evolution.beforeReset=CaptureOperations();Evolution.hasBeforeReset=true;
   int resource=RecoveryResource();Evolution.runs.Add(new ResetRun{layer=layer,resource=resource,at=S.time,duration=Math.Max(0,S.time-(layer==0?Meta.prestigeStarted:layer==1?Meta.ascensionStarted:Meta.genesisStarted)),reward=reward,output=gross[resource],focus=ActiveFocus});
   if(Evolution.runs.Count>24)Evolution.runs.RemoveAt(0);
  }
  void TrackRecovery(){
   // Only the most recent run can recover: another reset interrupts the experiment.
   if(S.satellite||Evolution.runs.Count==0||S.activeChallenge!="")return;var r=Evolution.runs[Evolution.runs.Count-1];
   if(r.recoveredAt<0&&gross[r.resource]>=r.output.Value)r.recoveredAt=S.time;
  }
  public string ResearchBlock(){
   if(S.researchPlan.queue.Count==0)return "Queue is empty. Add a research goal.";
   var u=FindUpgrade(S.researchPlan.queue[0]);if(!StageOpen(u.stage))return "Next: "+u.name+". Discover its chapter first.";
   if(!string.IsNullOrEmpty(u.requires)&&!S.purchases.Contains(u.requires))return "Next: "+u.name+". Requires "+FindUpgrade(u.requires)?.name+".";
   var cost=BigDouble.Parse(u.cost);var missing=BigDouble.Max(0,cost-ResearchAvailable(u.costResource));
   return missing>0?"Next: "+u.name+". Need "+new Number(missing)+" unreserved "+Data.resources[u.costResource]+"; protected "+new Number(EffectiveReserve(u.costResource))+".":"Next: "+u.name+" is funded.";
  }
  public RuleStatus ExplainRule(string key){
   RefreshRates();string tick="Next decision in "+Math.Max(0,Math.Ceiling(Meta.nextAutomation-S.time))+"s. ";
   if(key=="research"){
    var next=S.researchPlan.queue.Count>0?FindUpgrade(S.researchPlan.queue[0]):null;
    return new RuleStatus(!S.researchPlan.automatic?"OFF":!HasA("a_time_2")?"LOCKED":next==null?"WAITING":CanResearch(next)&&ResearchAvailable(next.costResource)>=BigDouble.Parse(next.cost)?"READY":"BLOCKED",(!HasA("a_time_2")?"Temporal workshop required. ":tick)+ResearchBlock());
   }
   if(key=="buy"){
    if(!AnyAutobuyer)return new RuleStatus("LOCKED","Buy a chapter autobuyer for 1 Prestige Point in Automation. Production continues without it.");
    if(!S.automation.buy)return new RuleStatus("OFF","Manual purchases remain available.");
    int missing=Array.FindIndex(Data.generators,g=>Open(g)&&AutobuyerUnlocked(Array.IndexOf(Data.generators,g))&&S.owned[Array.IndexOf(Data.generators,g)]==0);
    if(Meta.guidedAutomation&&missing>=0){var g=Data.generators[missing];var need=BigDouble.Max(0,Cost(missing)-ResearchAvailable(g.costResource));return new RuleStatus(need>0?"SAVING":"READY",tick+g.name+" needs "+new Number(need)+" more unreserved "+Data.resources[g.costResource]+". Routing banks 20% of incoming supply; consumers keep running.");}
    int next=Array.Find(S.automation.priority,i=>AutobuyerUnlocked(i)&&Open(Data.generators[i])&&MaxBuy(i)>0);
    bool ready=AutobuyerUnlocked(next)&&Open(Data.generators[next])&&MaxBuy(next)>0;return new RuleStatus(ready?"READY":"WAITING",tick+(ready?"Buy "+Data.generators[next].name+" in saved priority order.":"No process is affordable above reserves and construction funding."));
   }
   if(key=="build"){
    if(!Meta.autoBuild)return new RuleStatus("OFF","Manual construction remains available.");if(!HasLayer(1))return new RuleStatus("LOCKED","Complete Ascension.");int p=PlannedMega();if(p<0)return new RuleStatus("WAITING","Projects are building or reached the planner target.");
    string deficits=string.Join(", ",Enumerable.Range(0,3).Where(i=>S.resources[ExpansionContent.MegaResources[p][i]].Value-S.reserves[ExpansionContent.MegaResources[p][i]].Value<MegaCost(p,i)).Select(i=>Data.resources[ExpansionContent.MegaResources[p][i]]));return new RuleStatus(deficits==""?"READY":"SAVING",tick+ExpansionContent.MegaNames[p]+(deficits==""?" is funded.":" needs "+deficits+" above reserves."));
   }
   if(key=="all-research")return new RuleStatus(!Meta.autoResearch?"OFF":!HasA("a_time_2")?"LOCKED":"ARMED",!HasA("a_time_2")?"Temporal workshop required.":tick+"Affordable research after queued work; reserves remain protected.");
   if(key=="expeditions")return new RuleStatus(!S.automation.expeditions?"OFF":!HasLayer(1)?"LOCKED":"ARMED",!HasLayer(1)?"Complete Ascension.":tick+"Claim completed voyages; relaunch funded voyages above reserves.");
   return new RuleStatus(Meta.guidedAutomation&&S.automation.buy?"ARMED":"OFF", "Routing saves for a purchased autobuyer's next process or upgrade by banking 20% of incoming supply. Production stays active. After Prestige it also restores affordable research, unless an explicit research queue takes priority.");
  }
  public ObservatoryAdvice[] Diagnose(){
   RefreshRates();var result=new List<ObservatoryAdvice>();
   if(S.researchPlan.automatic&&S.researchPlan.queue.Count>0)result.Add(new ObservatoryAdvice{title="Research plan",detail=ExplainRule("research").ToString(),route="Research"});
   foreach(int i in Enumerable.Range(0,24).Where(i=>S.owned[i]>0&&Open(Data.generators[i]))){var g=Data.generators[i];if(g.input<0||flows[i]>=installedMultipliers[i]*g.rate*S.owned[i]*.999)continue;
    int r=g.input;var protectedStock=EffectiveReserve(r);string cause=protectedStock>0&&S.resources[r].Value<=protectedStock?"Protected "+new Number(protectedStock)+" "+Data.resources[r]+" limits available input. Review reserves and construction funding.":"Demand for "+Data.resources[r]+" exceeds supply. Inspect producers and competing consumers.";
    result.Add(new ObservatoryAdvice{title=g.name+" is input limited",detail=cause,route="Analytics",resource=r,process=i});break;
   }
   if(!S.automation.buy)result.Add(new ObservatoryAdvice{title="Automatic purchases are off",detail="The economy produces resources, but installed capacity will not grow by itself.",route="Automation"});
   var ready=Data.research.FirstOrDefault(u=>CanResearch(u));if(ready!=null)result.Add(new ObservatoryAdvice{title="Research ready: "+ready.name,detail="An available upgrade can strengthen production. Compare its effect before buying.",route="Research"});
   if(result.Count==0)result.Add(new ObservatoryAdvice{title="Production is running",detail="Review your next chapter or compare another strategy. Current checks found no input-limited owned process.",route="Roadmap"});
   return result.Take(3).ToArray();
  }
 }
 // Callers provide a serialized copy of the entire state. Work is resumable and
 // bounded; this never evaluates against or writes to the live player economy.
 public sealed class ResetForecast {
  public readonly Simulation simulation; public readonly int resource; public readonly Number target;
  public readonly double started; public bool done,eligible; public double recovered=-1;
  public const double Horizon=172800;
  public ResetForecast(Content data,State copy,int layer){simulation=new Simulation(data,copy);resource=simulation.RecoveryResource();target=simulation.Gross(resource);eligible=simulation.Rebirth(layer);started=simulation.S.time;Array.Clear(simulation.S.automation.repeatPrestige,0,3);simulation.ConfigureDevelopmentPlan("");copy.offlineReport=null;copy.offlineRemaining=copy.liveRemaining=0;done=!eligible;}
  public void Step(){if(done)return;double span=Math.Min(60,Horizon-(simulation.S.time-started));simulation.Advance(span,128);simulation.RefreshRates();if(simulation.CachedGross(resource)>=target.Value){recovered=simulation.S.time-started;done=true;}else if(simulation.S.time-started>=Horizon-1e-6)done=true;}
 }
}
