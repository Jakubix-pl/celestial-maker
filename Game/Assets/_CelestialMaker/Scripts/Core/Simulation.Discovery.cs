using System;
using System.Linq;
using System.Collections.Generic;
using BreakInfinity;
namespace CelestialMaker.Core {
 public sealed partial class Simulation {
  public DiscoveryState Discover=>S.discovery;
  public GoalOrder CurrentGoal=>Discover.goalsEnabled&&Discover.goalIndex<Discover.goals.Count?Discover.goals[Discover.goalIndex]:null;
  public bool GoalHoldsResets=>CurrentGoal!=null&&CurrentGoal.holdResets;
  public int Constraint=>Discover.scenario>0?Discover.scenario:S.activeChallenge==""?0:1+Array.FindIndex(Data.challenges,c=>c.id==S.activeChallenge)%3;
  public string ConstraintDescription=>DiscoveryContent.Rules[Constraint];
  void ValidateDiscovery(){
   var d=Discover;
   if(d==null||d.schema!=1||d.modules?.Length!=3||d.moduleReady?.Length!=3||d.routes?.Length!=8||d.signals?.Length!=8||d.lessons?.Length!=3||d.goals==null||d.goals.Count>6||d.goalIndex<0||d.goalIndex>d.goals.Count||d.role<0||d.role>3||d.scenario<0||d.scenario>3||d.seed<0||d.seed>999999||d.chronicle==null||d.chronicle.Count>160||d.firsts==null||d.firsts.Count>256)throw new ArgumentException("Invalid discovery state");
   if(d.modules.Any(i=>i<0||i>3)||d.routes.Any(i=>i<0||i>2)||d.moduleReady.Any(t=>!FiniteTime(t))||!FiniteTime(d.scenarioFinished))throw new ArgumentException("Invalid discovery choice");
   foreach(var g in d.goals)if(!ValidGoal(g))throw new ArgumentException("Invalid goal order");
   foreach(var e in d.chronicle)if(e==null||!FiniteTime(e.time)||e.key==null||e.key.Length>80||e.text==null||e.text.Length>320)throw new ArgumentException("Invalid chronicle");
   if(d.firsts.Any(x=>x==null||x.Length>80)||d.goalStatus==null||d.goalStatus.Length>320||d.challengeReview==null||d.challengeReview.Length>600)throw new ArgumentException("Invalid discovery text");
   foreach(var u in Meta.universes)if(u.role<0||u.role>3||!FiniteTime(u.supportUntil))throw new ArgumentException("Invalid universe role");
  }
  bool ValidGoal(GoalOrder g)=>g!=null&&g.kind>=0&&g.kind<=1&&g.project>=0&&g.project<3&&g.level>=1&&g.level<=6&&g.resource>=0&&g.resource<12&&ValidHistoryNumber(g.amount)&&g.amount.Value>=0&&(g.kind==0||g.amount.Value>0)&&new[]{"Save","Resume","Energy","Matter","Time"}.Contains(g.after);
  public bool QueueGoal(GoalOrder goal){if(S.satellite||!HasLayer(0)||Discover.goals.Count>=6||!ValidGoal(goal))return false;Discover.goals.Add(new GoalOrder{kind=goal.kind,project=goal.project,level=goal.level,resource=goal.resource,amount=goal.amount,holdResets=goal.holdResets,after=goal.after});Revision++;return true;}
  public bool EnableGoals(){if(S.satellite||!HasLayer(0)||Discover.goalIndex>=Discover.goals.Count)return false;Discover.goalsEnabled=true;Discover.goalStatus="Armed: "+GoalName(CurrentGoal);Chronicle("goals-enabled",Discover.goalStatus);Revision++;return true;}
  public void PauseGoals(){Discover.goalsEnabled=false;Discover.goalStatus="Sequence paused; other automation retains its settings.";Revision++;}
  public void ClearGoals(){Discover.goalsEnabled=false;Discover.goals.Clear();Discover.goalIndex=0;Discover.goalStatus="No goal sequence";Revision++;}
  public string GoalName(GoalOrder g)=>g==null?"No active goal":g.kind==0?ExpansionContent.MegaNames[g.project]+" stage "+g.level:"Hold "+g.amount+" "+Data.resources[g.resource];
  public void StopAllAutomation(){Discover.goalsEnabled=false;Evolution.developmentPlan="";S.automation.buy=S.automation.expeditions=Meta.autoBuild=Meta.autoResearch=false;S.researchPlan.automatic=false;Meta.guidedAutomation=false;Array.Clear(S.automation.repeatPrestige,0,3);Array.Clear(S.paused,0,24);foreach(var t in Meta.transfers)t.enabled=false;foreach(var u in Meta.universes)EditUniverse(u.id,e=>{e.S.automation.buy=false;e.S.automation.expeditions=false;e.S.researchPlan.automatic=false;e.Meta.autoResearch=false;});Revision++;}
  bool RunGoal(){
   var g=CurrentGoal;if(g==null||S.activeChallenge!="")return false;
   bool done=g.kind==0?Meta.megas[g.project].level>=g.level:Meets(S.resources[g.resource].Value,g.amount.Value);
   if(!done){Discover.goalStatus=(g.kind==0&&!HasLayer(1)?"LOCKED / Ascension required: ":g.kind==0&&g.level>3&&!HasG("g_horizon")&&Meta.megas[g.project].level>=3?"LOCKED / Endless horizon required: ":"WORKING / ")+GoalName(g)+(g.holdResets?"; automatic resets held":"");if(g.kind==0&&HasLayer(1))Meta.autoBuild=true;return false;}
   Chronicle("goal-finished",GoalName(g)+" complete; next action: "+g.after);Discover.goalIndex++;Discover.goalStatus="Completed: "+GoalName(g);
   if(g.after=="Save"){
    StopAllAutomation();for(int r=0;r<12;r++)S.reserves[r]=BigDouble.Max(S.reserves[r].Value,S.resources[r].Value);
    Discover.goalStatus+=". Automation stopped; current home stock protected.";return true;
   }
   if(g.after!="Resume")ConfigureDevelopmentPlan(g.after);
   Discover.goalsEnabled=Discover.goalIndex<Discover.goals.Count;Revision++;return false;
  }
  BigDouble GoalReserve(int resource){var g=CurrentGoal;return g!=null&&g.kind==1&&g.resource==resource?g.amount.Value:BigDouble.Zero;}
  public void Chronicle(string key,string text,bool once=false){if(S.satellite)return;if(once){if(Discover.firsts.Contains(key))return;if(Discover.firsts.Count>=256){int old=Discover.firsts.FindIndex(k=>k.StartsWith("scenario-"));Discover.firsts.RemoveAt(old<0?0:old);}Discover.firsts.Add(key);}Discover.chronicle.Add(new ChronicleEntry{time=S.time,key=key,text=text});if(Discover.chronicle.Count>160)Discover.chronicle.RemoveAt(0);}
  public bool ModuleUnlocked(int project,int choice){if(project<0||project>2||choice<0||choice>3||S.satellite)return false;return choice==0||Meta.megas[project].level>=(choice==1?1:choice==2?2:4);}
  public bool SelectModule(int project,int choice){if(!ModuleUnlocked(project,choice)||S.time<Discover.moduleReady[project])return false;SettleUniverses();Discover.modules[project]=choice;Discover.moduleReady[project]=S.time+3600;Chronicle("module-"+project+"-"+choice,ExpansionContent.MegaNames[project]+": "+DiscoveryContent.ModuleNames[project][choice]);InvalidateTickCache();Revision++;return true;}
  int Module(int p)=>ModuleUnlocked(p,Discover.modules[p])?Discover.modules[p]:0;
  double DiscoveryOutput(bool primary){double f=1;if(Module(0)==1&&primary)f*=1.12;if(Module(0)==2&&primary)f*=.93;if(Module(1)==1&&!primary)f*=.96;if(Module(1)==2&&primary)f*=.92;if(Module(2)==1&&!primary)f*=1.12;if(Module(2)==2&&!primary)f*=.96;
   if(Discover.role==1)f*=primary?1.12:.92;if(Discover.role==2&&!primary)f*=1.12;if(Discover.role==3)f*=.9;return f;}
  double DiscoveryInputs=>(Module(1)==1?.9:1)*(Module(2)==1?1.08:Module(2)==3?.95:1)*(Discover.role==2?1.06:1)*(Constraint==3?2:1);
  double DiscoveryWork(int project){double f=(project==0&&Module(0)==3?.97:1)*(project==1?(Module(1)==2?1.05:Module(1)==3?.97:1):1)*(Module(2)==3?.99:1);int active=0;foreach(var u in Meta.universes)if(u.role==3&&u.supportUntil>S.time+1e-7)active++;return f*(1+.02*active);}
  double DiscoveryTransferRetention=>Math.Max(.1,Math.Min(.98,(ActiveFocus=="Matter"?.9:ActiveFocus=="Time"?.7:.8)+(Module(0)==1?-.05:0)+(Module(1)==3?.08:0)));
  double DiscoveryBoundary(double dt){foreach(var u in Meta.universes)if(u.supportUntil>S.time+1e-7)dt=Math.Min(dt,u.supportUntil-S.time);return dt;}
  public bool SetUniverseRole(int id,int role){if(id<=0||role<0||role>3||!HasLayer(1))return false;var u=Meta.universes.Find(x=>x.id==id);if(u==null)return false;SettleUniverses();u.role=role;u.supportUntil=0;Universe(id).Discover.role=role;Universe(id).InvalidateTickCache();Chronicle("role-"+id,"Universe "+id+": "+DiscoveryContent.Roles[role]);Revision++;return true;}
  public bool ChooseRoute(int id,int choice){if(id<0||id>=8||choice<1||choice>2||!Discover.signals[id]||Discover.routes[id]!=0)return false;Discover.routes[id]=choice;Chronicle("route-"+id,Data.expeditions[id].name+": "+(choice==1?"Survey route":"Recovery route"),true);Revision++;return true;}
  public string RouteDescription(int choice)=>choice==1?"Survey: duration -15%, reward -15%; archive the anomaly.":choice==2?"Recovery: reward +20%, duration +20%; preserve an artifact.":"Standard route; the first reward reveals a permanent route choice.";
  void DiscoveryEvents(){
   if(S.satellite)return;
   for(int p=0;p<3;p++)for(int m=1;m<=3;m++)if(ModuleUnlocked(p,m))Chronicle("blueprint-"+p+"-"+m,"Blueprint discovered: "+DiscoveryContent.ModuleNames[p][m],true);
   if(CampaignComplete)Chronicle("finale","Your three connected universes form a stable cosmos. The campaign is complete; new scenarios are optional.",true);
   if(Discover.scenario>0&&!Discover.scenarioComplete&&S.lifetime[3].Value>=BigDouble.Pow10(6+Discover.seed%3)){
    Discover.scenarioComplete=true;Discover.scenarioFinished=S.time;Chronicle("scenario-finished",DiscoveryContent.Scenarios[Discover.scenario]+" complete.",true);Revision++;}
  }
  public string ChallengeReadiness(int index){if(index<0||index>=12)return "Unknown challenge";var d=Data.challenges[index];if(!HasLayer(0)||!StageOpen(d.stage))return "Locked: discover this chapter after Prestige.";return "Independent trial: "+DiscoveryContent.Rules[1+index%3]+" Target "+d.target+" "+Data.resources[d.resource]+". A six-hour trial estimates readiness without changing your campaign.";}
  public static State ScenarioState(int scenario,int seed){if(scenario<1||scenario>3||seed<0||seed>999999)throw new ArgumentException("Invalid scenario");var state=new State();state.discovery.scenario=scenario;state.discovery.seed=seed;state.profileId="scenario-"+scenario+"-"+seed;return state;}
 }
}
