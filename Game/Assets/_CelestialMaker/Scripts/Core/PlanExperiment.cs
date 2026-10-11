using System;
using System.Linq;
using BreakInfinity;
namespace CelestialMaker.Core {
 // Caller supplies independent serialized copies. No callbacks to the live save.
 public sealed class PlanExperiment {
  public readonly Simulation simulation;
  public readonly double started,horizon;
  public double idleSeconds;public string idleReason="No blocked goal observed";
  double nextSample;
  public PlanExperiment(Content data,State isolated,double hours=6){
   if(hours<=0||hours>24||double.IsNaN(hours))throw new ArgumentException("Invalid experiment horizon");
   simulation=new Simulation(data,isolated);isolated.offlineRemaining=isolated.liveRemaining=0;isolated.offlineReport=null;isolated.utc=0;
   started=isolated.time;nextSample=started+60;horizon=hours*3600;simulation.BeginOffline(horizon);
  }
  public bool Done=>simulation.S.offlineRemaining<=1e-7;
  public void Step(int ticks=240,double milliseconds=0){
   simulation.ProcessOffline(ticks,milliseconds);
   if(simulation.S.time>=nextSample){string reason=BlockedReason();if(reason!=""){idleSeconds+=simulation.S.time-(nextSample-60);idleReason=reason;}nextSample=simulation.S.time+60;}
  }
  string BlockedReason(){
   var s=simulation;var goal=s.CurrentGoal;
   if(goal!=null){if(s.Discover.goalStatus.StartsWith("LOCKED"))return s.Discover.goalStatus;
    if(goal.kind==1&&s.S.resources[goal.resource].Value<goal.amount.Value&&s.Rate(goal.resource)<=0)return s.GoalName(goal)+": net production is zero or negative. "+s.Diagnose()[0].detail;
    if(goal.kind==0&&s.Meta.megas[goal.project].building)return "";
   }
   foreach(string key in new[]{"research","build","buy"}){var rule=s.ExplainRule(key);if(rule.state=="SAVING"||rule.state=="BLOCKED"||rule.state=="LOCKED"||(key=="buy"&&rule.state=="WAITING"))return key+": "+rule.detail;}
   return "";
  }
  public string Summary(){var s=simulation;var r=s.S.offlineReport;return (Done?"Complete":"Partial")+" / "+OfflineReport.Duration(s.S.time-started)+"\nStock: "+string.Join("; ",Enumerable.Range(0,12).Where(i=>s.StageOpen(i/2)).Select(i=>s.Data.resources[i]+" "+s.S.resources[i]))+"\nSpent: "+string.Join("; ",Enumerable.Range(0,12).Where(i=>r.spent[i].Value>0).Select(i=>s.Data.resources[i]+" "+r.spent[i]))+"\nResets "+string.Join(" / ",r.resets)+"\nProjects "+s.Meta.megas[0].level+" / "+s.Meta.megas[1].level+" / "+s.Meta.megas[2].level+"\nConstruction work: "+string.Join(" / ",Enumerable.Range(0,3).Select(i=>(100*s.Meta.megas[i].work/s.MegaWork(i)).ToString("0.0")+"%"))+"\n"+s.Discover.goalStatus+(s.CurrentGoal!=null&&s.CurrentGoal.kind==1?" / stock "+s.S.resources[s.CurrentGoal.resource]+" of "+s.CurrentGoal.amount:"")+"\nBlocked-rule samples: "+OfflineReport.Duration(idleSeconds)+" / "+idleReason;}
 }
 public sealed class PracticeLesson {
  public readonly Simulation simulation;public readonly int lesson;public int step;public int TotalSteps=>lesson==3?5:3;public bool Complete=>step>=TotalSteps;
  public string Title=>lesson==3?"Prestige-funded autobuyers":DiscoveryContent.Lessons[lesson];
  public PracticeLesson(Content data,int id){if(id<0||id>3)throw new ArgumentException("Invalid lesson");lesson=id;simulation=new Simulation(data);simulation.S.automation.buy=false;simulation.Meta.guidedAutomation=false;simulation.S.resources[0]=new BigDouble(1000);simulation.S.owned[0]=5;simulation.S.owned[1]=1;if(id>=2)simulation.S.lifetime[0]=BigDouble.Pow10(ProgressionBalance.PrestigeEnergyLog);if(id==3){simulation.Rebirth(0);simulation.S.resources[0]=new BigDouble(2000);}}
  public string Instruction=>Complete?"Lesson complete. Return to your unchanged campaign.":lesson==0?new[]{"Observe the current Energy stock.","Protect 800 Energy from purchases and consumption.","Run 60 seconds and inspect the protected stock."}[step]:lesson==1?new[]{"Inspect an economy without an Energy producer or stock.","Inspect the input-limited Particle flow.","Restore the producer and compare the Particle flow."}[step]:lesson==2?new[]{"Inspect the available Prestige reward.","Perform the reset on this practice economy.","Inspect retained points and rebuilt processes."}[step]:new[]{"Inspect the earned point and its passive production bonus.","Spend one Prestige Point to unlock the Particles chapter buyer.","Run 120 seconds and observe automatic purchases. Practice stock was supplied on this separate copy.","Perform another Prestige and check whether the paid buyer survives.","Try an eligible Ascension and inspect the buyer and point reset. Only this practice copy is changed."}[step];
  public string Act(){var s=simulation;string result="";if(Complete)return Instruction;
   if(lesson==0){if(step==0)result="Energy stock: "+s.S.resources[0];if(step==1){s.S.reserves[0]=new BigDouble(800);result="Protected 800 Energy. Buy Max cannot spend it.";}if(step==2){for(int i=0;i<3600;i++)s.AdvanceTick();result="Energy after 60s: "+s.S.resources[0]+" / protected "+s.S.reserves[0];}}
   if(lesson==1){if(step==0){s.S.owned[0]=0;s.S.resources[0]=BigDouble.Zero;result="No Energy producer is installed; its stock is empty.";}if(step==1)result="Particle flow without Energy: "+new Number(s.Flow(1))+" /s";if(step==2){s.S.owned[0]=5;result="Particle flow restored: "+new Number(s.Flow(1))+" /s";}}
   if(lesson==2){if(step==0)result="Prestige reward: "+new Number(s.PrestigeReward(0))+". The run economy will reset.";if(step==1)result=s.Rebirth(0)?"Reset completed in practice.":"Reset unavailable";if(step==2)result="Points retained: "+s.S.prestige[0]+" / starting dynamos: "+s.S.owned[0];}
   if(lesson==3){
    if(step==0)result="Earned points: "+s.S.prestige[0]+" / unspent Prestige bonus: x"+new Number(BigDouble.Pow(s.S.prestige[0].Value+1,.25))+". Each paid package covers four processes in all universes.";
    if(step==1){s.UnlockAutobuyer(0);s.S.automation.buy=true;result="Point balance: "+s.S.prestige[0]+" / paid buyer: "+s.S.autobuyers[0]+" / unspent bonus: x"+new Number(BigDouble.Pow(s.S.prestige[0].Value+1,.25))+". Ownership and the On/Off rule are separate.";}
    if(step==2){for(int i=0;i<7200;i++)s.AdvanceTick();result="Owned processes: "+string.Join(" / ",s.S.owned.Take(4))+". Purchases also run during offline tick replay; inputs and reserves still apply.";}
    if(step==3){s.S.lifetime[0]=BigDouble.Pow10(ProgressionBalance.PrestigeEnergyLog);s.Rebirth(0);result="Prestige completed / paid buyer kept: "+s.S.autobuyers[0]+". You do not pay for it again after Prestige.";}
    if(step==4){s.S.lifetime[5]=new BigDouble(1e9);s.Meta.prestigeEarned=new BigDouble(ProgressionBalance.PrestigeForAscension);s.Rebirth(1);result="Ascension completed / paid buyer: "+s.S.autobuyers[0]+" / Prestige Points: "+s.S.prestige[0]+". Ascension and Genesis clear packages. Earn points again, or let an enabled Development Plan rebuild automation.";}
   }
   step++;return result;
  }
 }
}
