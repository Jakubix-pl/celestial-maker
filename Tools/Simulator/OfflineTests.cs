using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using BreakInfinity;
using CelestialMaker.Core;

static class OfflineTests {
 static JsonSerializerOptions json;static Content data;static Action<string,bool> check;
 static State Clone(State s)=>JsonSerializer.Deserialize<State>(JsonSerializer.Serialize(s,json),json);
 static void Finish(Simulation s,int budget=32){int rounds=0;while(s.S.offlineRemaining>0){s.ProcessOffline(budget);if(++rounds>100000)throw new Exception("Offline event processing did not converge");}}
 static bool Close(BigDouble a,BigDouble b,BigDouble scale)=>BigDouble.Abs(a-b)/BigDouble.Max(1,scale)<1e-9;
 static bool Balanced(OfflineReport r)=>Enumerable.Range(0,15).All(i=>Close(r.before[i].Value+r.produced[i].Value+r.granted[i].Value-r.spent[i].Value-r.resetLoss[i].Value,r.after[i].Value,BigDouble.Max(r.before[i].Value,r.produced[i].Value+r.granted[i].Value)));
 public static void Run(Content content,JsonSerializerOptions options,Action<string,bool> assertion,string root){
  data=content;json=options;check=assertion;
  foreach(double seconds in new[]{3600d,86400d,604800d,2592000d,31536000d}){
   var s=new Simulation(data);s.S.owned[1]=3;s.BeginOffline(seconds);Finish(s);
   check("offline aggregate "+seconds+"s",Balanced(s.S.offlineReport)&&Math.Abs(s.S.offlineReport.processedSeconds-seconds)<.01&&s.S.offlineRemaining==0);
  }
  var huge=new Simulation(data);huge.S.resources[0]=BigDouble.Parse("1e10000");huge.S.prestige[0]=BigDouble.Parse("1e9000");huge.BeginOffline(3600);Finish(huge);
  check("large report values retain Number pairs",Clone(huge.S).offlineReport.before[0].Value==BigDouble.Parse("1e10000")&&Balanced(huge.S.offlineReport));
  var depleted=new Simulation(data);depleted.S.owned[1]=100;depleted.BeginOffline(3600);Finish(depleted);
  check("depletion reports actual production and input spend",depleted.S.offlineReport.spent[0].Value>0&&depleted.S.offlineReport.produced[1].Value>0&&Balanced(depleted.S.offlineReport));
  check("new offline stage remains discoverable in report",depleted.S.offlineReport.stagesAfter[1]&&!depleted.S.offlineReport.stagesBefore[1]);
  check("undiscovered content stays hidden",!new Simulation(data).S.settings.Equals(null)&&!depleted.S.offlineReport.Describe(data,true).Contains("Genesis Shards"));
  var boosted=new Simulation(data);boosted.BoostWindows.Add(new BoostWindow{start=900,end=1800});boosted.BeginOffline(3600);Finish(boosted);
  check("boost time measures only its actual interval",Math.Abs(boosted.S.offlineReport.boostSeconds-900)<.0001&&Balanced(boosted.S.offlineReport));
  var challenge=new Simulation(data);challenge.S.resets[0]=1;challenge.EnterChallenge(0);challenge.PremiumUntil=10000;challenge.BeginOffline(30);Finish(challenge);
  check("challenge offline excludes premium",challenge.S.offlineReport.boostSeconds==0);
  var resetState=new State();resetState.resets[0]=1;resetState.automation.buy=true;resetState.automation.repeatPrestige[0]=true;resetState.lifetime[0]=BigDouble.Parse("1e9");resetState.resources[0]=BigDouble.Parse("1e8");
  var reset=new Simulation(data,Clone(resetState));reset.BeginOffline(86400);Finish(reset);
  var online=new Simulation(data,Clone(resetState));online.BeginOffline(86400);
  // Same aggregate and rules; incremental caller slices at the same event boundaries.
  for(int k=0;k<8640;k++){double pending=online.Advance(10);while(pending>1e-7)pending=online.Advance(pending);}
  check("autobuy and repeat-reset agree online/offline",Enumerable.Range(0,12).All(i=>Close(reset.S.resources[i].Value,online.S.resources[i].Value,reset.S.resources[i].Value))&&reset.S.owned.SequenceEqual(online.S.owned)&&reset.S.resets.SequenceEqual(online.S.resets));
  check("reset loss, prestige reward and autobuys aggregated",reset.S.offlineReport.resets[0]>0&&reset.S.offlineReport.granted[12].Value>0&&reset.S.offlineReport.automaticUnits.Sum()>0&&Balanced(reset.S.offlineReport));
  var expedition=new Simulation(data);expedition.S.resets[1]=1;expedition.S.resources[0]=BigDouble.Parse("1e10");for(int i=0;i<12;i++)expedition.S.lifetime[i]=BigDouble.Parse("1e20");expedition.S.automation.expeditions=true;expedition.BeginOffline(3600);Finish(expedition);
  check("offline voyages complete and claim once",expedition.S.offlineReport.finishedExpeditions.Sum()>0&&expedition.S.offlineReport.finishedExpeditions.Sum()==expedition.S.offlineReport.claimedExpeditions.Sum()&&Balanced(expedition.S.offlineReport));
  var partial=new Simulation(data,Clone(resetState));partial.BeginOffline(86400);partial.ProcessOffline(1);check("interrupted report retains pending work",partial.S.offlineRemaining>0&&partial.S.offlineReport.active);
  var resumed=new Simulation(data,SaveMigrations.Upgrade(Clone(partial.S)));resumed.BeginOffline(0);Finish(resumed,1);
  check("resume preserves resource/event report",Enumerable.Range(0,15).All(i=>Close(resumed.S.offlineReport.after[i].Value,reset.S.offlineReport.after[i].Value,reset.S.offlineReport.after[i].Value))&&resumed.S.offlineReport.resets.SequenceEqual(reset.S.offlineReport.resets)&&resumed.S.offlineReport.automaticUnits.SequenceEqual(reset.S.offlineReport.automaticUnits)&&Balanced(resumed.S.offlineReport));
  resumed.S.offlineReport.dismissed=true;var again=new Simulation(data,Clone(resumed.S));var stateBefore=JsonSerializer.Serialize(again.S,json);again.ProcessOffline();again.S.offlineReport.Describe(data,true);again.S.offlineReport.Describe(data,false);
  check("opening and dismissing report never grant progress",stateBefore==JsonSerializer.Serialize(again.S,json)&&again.S.offlineReport.dismissed);
  var legacy=new Simulation(data,new State{offlineRemaining=3600});legacy.BeginOffline(0);Finish(legacy);check("legacy pending cursor migrates honestly",legacy.S.offlineReport.legacyRemainder&&Balanced(legacy.S.offlineReport));
  File.WriteAllText(Path.Combine(root,"Logs/offline-report-sample.json"),JsonSerializer.Serialize(reset.S.offlineReport,json));
 }
}
