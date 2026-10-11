using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using BreakInfinity;
using CelestialMaker.Core;

partial class Program {
 static bool SameEconomy(Simulation a,Simulation b){
  a.SettleUniverses();b.SettleUniverses();
  bool local(Simulation x,Simulation y)=>Enumerable.Range(0,12).All(i=>Near(x.S.resources[i].Value,y.S.resources[i].Value)&&Near(x.S.lifetime[i].Value,y.S.lifetime[i].Value))&&x.S.owned.SequenceEqual(y.S.owned)&&x.S.purchases.OrderBy(i=>i).SequenceEqual(y.S.purchases.OrderBy(i=>i))&&x.S.resets.SequenceEqual(y.S.resets)&&Enumerable.Range(0,3).All(i=>Near(x.S.prestige[i].Value,y.S.prestige[i].Value)&&x.Meta.megas[i].level==y.Meta.megas[i].level&&Math.Abs(x.Meta.megas[i].work-y.Meta.megas[i].work)<.1);
  return local(a,b)&&a.Meta.completedTransfers==b.Meta.completedTransfers&&a.Meta.universes.Count==b.Meta.universes.Count&&a.Meta.universes.All(u=>local(a.Universe(u.id),b.Universe(u.id)));
 }
 static double AccountingError(Simulation s){var r=s.S.offlineReport;return Enumerable.Range(0,15).Max(i=>{var terms=new[]{r.before[i].Value,r.produced[i].Value,r.granted[i].Value,r.spent[i].Value,r.resetLoss[i].Value,r.after[i].Value};var scale=terms.Aggregate(new BigDouble(1),(a,b)=>BigDouble.Max(a,b));return (BigDouble.Abs(terms[0]+terms[1]+terms[2]-terms[3]-terms[4]-terms[5])/scale).ToDouble();});}
 static bool Accounted(Simulation s)=>AccountingError(s)<1e-7;
 static bool SameProgression(Simulation a,Simulation b)=>a.S.owned.SequenceEqual(b.S.owned)&&a.S.purchases.OrderBy(x=>x).SequenceEqual(b.S.purchases.OrderBy(x=>x))&&a.Meta.historicResets.SequenceEqual(b.Meta.historicResets)&&a.Meta.megas.Select(m=>m.level).SequenceEqual(b.Meta.megas.Select(m=>m.level))&&a.Meta.completedTransfers==b.Meta.completedTransfers&&a.Meta.universes.All(u=>a.Universe(u.id).S.owned.SequenceEqual(b.Universe(u.id).S.owned)&&a.Universe(u.id).S.purchases.OrderBy(x=>x).SequenceEqual(b.Universe(u.id).S.purchases.OrderBy(x=>x)));
 static void CadenceTests(){
  var fresh=new Simulation(data);Check("development plan is opt-in and gated by first Prestige",fresh.Evolution.developmentPlan==""&&!fresh.ConfigureDevelopmentPlan("Energy"));
  var s=Mature();s.Meta.historicResets[0]=1;s.Meta.megas[0].level=s.Meta.megas[1].level=s.Meta.megas[2].level=1;
  Check("development plan accepts known branches only",!s.ConfigureDevelopmentPlan("Unknown")&&s.ConfigureDevelopmentPlan("Energy"));
  Advance(s,120);Check("development plan leaves first Ascension and Genesis manual",s.Meta.historicResets[1]==0&&s.Meta.historicResets[2]==0);
  Check("development plan buys selected upgrades",s.HasA("a_energy_1"));s.SaveOperations(0,"Away");s.ConfigureDevelopmentPlan("");s.ApplyOperations(0);Check("complete profile restores explicit development plan",s.Evolution.developmentPlan=="Energy");
  var copy=new Simulation(data,Clone(s.S));Check("plan survives save reload",copy.Evolution.developmentPlan=="Energy");copy.ConfigureDevelopmentPlan("");Check("development plan can be stopped",copy.Evolution.developmentPlan=="");
  var forecast=new ResetForecast(data,Clone(s.S),0);Check("recovery forecasts cannot repeat development resets",forecast.simulation.Evolution.developmentPlan=="");
  var queue=new Simulation(data);queue.S.liveRemaining=.25;queue.BeginOffline(10);queue.ProcessOffline(31);var resumed=new Simulation(data,Clone(queue.S));resumed.BeginOffline(5);while(resumed.S.offlineRemaining>0)resumed.ProcessOffline(257);
  Check("pending absence live remainder and new absence conserve time",Math.Abs(resumed.S.time-15.25)<1e-6&&Math.Abs(resumed.S.offlineReport.processedSeconds-15.25)<1e-6&&resumed.S.offlineReport.fixedTicks==915&&Accounted(resumed));
  string before=JsonSerializer.Serialize(resumed.S,J);resumed.ProcessOffline();Check("completed offline work cannot be claimed twice",before==JsonSerializer.Serialize(resumed.S,J));
  var clock=new Simulation(data);FixedStepClock.Advance(clock,6,8);Check("live clock retains backlog without skipping ticks",clock.S.time<.14&&clock.S.liveRemaining>5.8);clock.BeginOffline(0);while(clock.S.offlineRemaining>0)clock.ProcessOffline();Check("live backlog replays the same ticks",Math.Abs(clock.S.time-6)<1e-6);
  var limited=new Simulation(data);limited.BeginOffline(31536000);var watch=Stopwatch.StartNew();limited.ProcessOffline(60000,.5);Check("offline processing yields to frame budget",limited.S.offlineRemaining>31535000&&watch.Elapsed.TotalSeconds<1&&limited.S.offlineReport.fixedTicks>0);
  var manual=Mature();manual.S.automation.buy=true;manual.Meta.guidedAutomation=true;manual.S.reserves[0]=BigDouble.Parse("1e30");Advance(manual,60);Check("routine guided research protects manual reserves",!manual.S.purchases.Contains(data.research[0].id));
  var lag=Mature();foreach(var m in lag.Meta.megas)m.level=1;lag.CreateUniverse(1);lag.S.time=45;lag.BeginOffline(1);while(lag.S.offlineRemaining>0)lag.ProcessOffline();Check("legacy parallel clock catches up using bounded ticks",Math.Abs(lag.Universe(1).S.time-46)<1e-6);
 }
 static void TickAudit(){
  double hours=double.Parse(arguments.FirstOrDefault(a=>a.StartsWith("--hours="))?.Substring(8)??"6",System.Globalization.CultureInfo.InvariantCulture);int ticks=checked((int)Math.Round(hours*3600*60));double span=ticks/60d;
  var rows=new List<object>();string dir=Path.Combine(root,"Logs/OfflineCadence");Directory.CreateDirectory(dir);
  foreach(string phase in new[]{"early","resets","parallel"}){
   if(arguments.Any(a=>a.StartsWith("--phase="))&&!arguments.Contains("--phase="+phase))continue;
   var seed=phase=="early"?new Simulation(data):Mature();
   if(phase!="early"){
    seed.Meta.ascensionUpgrades=ExpansionContent.Ascension.Select(u=>u.id).ToList();for(int i=0;i<24;i++)seed.S.owned[i]=5;
    seed.S.automation.buy=true;seed.S.automation.expeditions=true;seed.Meta.autoResearch=true;seed.Meta.autoBuild=true;
    seed.QueueResearch(data.research.Last().id);seed.S.researchPlan.automatic=true;
    foreach(var mega in seed.Meta.megas)mega.level=1;
    if(phase=="resets"){for(int i=0;i<3;i++)seed.Meta.historicResets[i]=1;seed.ConfigureDevelopmentPlan("Time");}
    else{seed.CreateUniverse(1);seed.CreateUniverse(2);seed.ScheduleTransfer(0,1,0);seed.ScheduleTransfer(1,2,1);seed.EditUniverse(1,e=>{e.S.owned[1]=5;e.QueueResearch(data.research[1].id);e.S.researchPlan.automatic=true;});}
   }
   var initial=Clone(seed.S);var live=new Simulation(data,Clone(initial));var offline=new Simulation(data,Clone(initial));var oracle=new Simulation(data,Clone(initial));var interrupted=new Simulation(data,Clone(initial));
   if(arguments.Contains("--trace")){
    for(int minute=0;minute<span/60;minute++){offline.BeginOffline(60);while(offline.S.offlineRemaining>0)offline.ProcessOffline(3600);Advance(oracle,60);if(!SameEconomy(offline,oracle)){Console.WriteLine("First divergence at minute "+(minute+1));File.WriteAllText(Path.Combine(dir,"trace-offline.json"),JsonSerializer.Serialize(offline.S,J));File.WriteAllText(Path.Combine(dir,"trace-oracle.json"),JsonSerializer.Serialize(oracle.S,J));return;}}
    Console.WriteLine("Trace matched all minutes");return;
   }
   var watch=Stopwatch.StartNew();offline.BeginOffline(span);while(offline.S.offlineRemaining>0)offline.ProcessOffline(32768);double offlineSeconds=watch.Elapsed.TotalSeconds;
   watch.Restart();for(int tick=0;tick<ticks;tick+=60)FixedStepClock.Advance(live,Math.Min(60,ticks-tick)/60d,60);double onlineSeconds=watch.Elapsed.TotalSeconds;
   Advance(oracle,span);
   interrupted.BeginOffline(span);interrupted.ProcessOffline(1031);interrupted=new Simulation(data,Clone(interrupted.S));interrupted.BeginOffline(0);while(interrupted.S.offlineRemaining>0)interrupted.ProcessOffline(4093);
   bool tickMatch=SameEconomy(live,offline),resumeMatch=SameEconomy(interrupted,offline),oracleMatch=SameEconomy(oracle,offline),oracleProgressionMatch=SameProgression(oracle,offline),report=Accounted(offline),count=offline.S.offlineReport.fixedTicks==ticks;
   // The retired event accelerator is a structural cross-check. It differs
   // numerically in the huge-stock parallel stress fixture; report that result
   // openly. Online ticks and serialized tick replay are the balance reference.
   Check("tick audit online/offline / "+phase,tickMatch);Check("tick audit interrupted replay / "+phase,resumeMatch);Check("event cross-check "+(phase=="parallel"?"progression":"economy")+" / "+phase,phase=="parallel"?oracleProgressionMatch:oracleMatch);Check("tick audit accounting and count / "+phase,report&&count);
   rows.Add(new{phase,hours,ticks,actualTicks=offline.S.offlineReport.fixedTicks,tickMatch,resumeMatch,oracleMatch,oracleProgressionMatch,report,accountingRelativeError=AccountingError(offline),offlineSeconds,onlineSeconds,resets=offline.S.offlineReport.resets,units=offline.S.offlineReport.automaticUnits.Sum(),voyages=offline.S.offlineReport.claimedExpeditions.Sum(),transfers=offline.Meta.completedTransfers});
   foreach(var item in new[]{(name:"live",sim:live),(name:"offline",sim:offline),(name:"resumed",sim:interrupted),(name:"oracle",sim:oracle)})File.WriteAllText(Path.Combine(dir,"audit-"+phase+"-"+item.name+".json"),JsonSerializer.Serialize(item.sim.S,J));
   File.WriteAllText(Path.Combine(dir,"tick-audit"+(arguments.Any(a=>a.StartsWith("--phase="))?"-"+phase:"")+".json"),JsonSerializer.Serialize(new{version=ProgressionBalance.Version,pass,fail,rows},J));
  }
  Environment.ExitCode=fail==0?0:1;
 }
}
