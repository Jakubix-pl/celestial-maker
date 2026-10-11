using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using CelestialMaker.Core;
using BreakInfinity;
partial class Program {
 static void SystemAudit(){
  var rows=new List<object>();
  for(int project=0;project<3;project++)for(int choice=1;choice<=3;choice++){
   var basis=Mature();foreach(var mega in basis.Meta.megas){mega.level=4;mega.building=true;}basis.S.owned[1]=10;
   var s=new Simulation(data,Clone(basis.S));bool selected=s.SelectModule(project,choice);
   var primary=s.Gross(0)/basis.Gross(0);var refined=s.Flow(1)/basis.Flow(1);
   var input=(s.Consumption(1)/s.Flow(1))/(basis.Consumption(1)/basis.Flow(1));
   double work=basis.MegaSeconds(project)/s.MegaSeconds(project);bool correct=false;
   if(project==0&&choice==1)correct=Near(primary,1.12)&&Math.Abs(s.TransferRetention-.75)<1e-8;
   if(project==0&&choice==2)correct=Near(primary,.93)&&Math.Abs(s.TransferFraction(0)-.075)<1e-8;
   if(project==0&&choice==3){basis.StartVoyage(0);s.StartVoyage(0);correct=Math.Abs(s.S.voyages[0].finish/basis.S.voyages[0].finish-.8)<1e-8&&Math.Abs(work-.97)<1e-8;}
   if(project==1&&choice==1)correct=Near(input,.9)&&Near(refined,.96);
   if(project==1&&choice==2)correct=Near(primary,.92)&&Math.Abs(work-1.05)<1e-8;
   if(project==1&&choice==3)correct=Math.Abs(s.TransferRetention-.88)<1e-8&&Math.Abs(work-.97)<1e-8;
   if(project==2&&choice==1)correct=Near(input,1.08)&&Near(refined,1.12);
   if(project==2&&choice==2)correct=Math.Abs(s.TransferInterval-2880)<1e-8&&Near(refined,.96);
   if(project==2&&choice==3)correct=Near(input,.95)&&Math.Abs(work-.99)<1e-8;
   Check("engineering benefit and cost match actual engine "+project+"/"+choice,selected&&correct);
  }
  var campaign=JsonSerializer.Deserialize<State>(File.ReadAllText(Path.Combine(root,"Tools/Expansion/Fixtures/late-campaign.json")),J);
  for(int id=0;id<12&&!arguments.Contains("--scenarios-only");id++){
   var s=new Simulation(data,Clone(campaign));s.StopAllAutomation();Array.Clear(s.S.reserves,0,12);
   Check("challenge available on measured completed campaign "+id,s.EnterChallenge(id));
   var originalResources=(Number[])s.S.challengeResources.Clone();var originalOwned=(long[])s.S.challengeOwned.Clone();
   s.S.automation.buy=true;s.Meta.guidedAutomation=true;double started=s.S.time;
   // Purchase normally; keep the target once its producer chain is established.
   while(s.S.activeChallenge!=""&&s.S.time-started<21600){
    var c=data.challenges[id];if(s.Gross(c.resource)>0&&s.S.resources[c.resource].Value>BigDouble.Parse(c.target)*.01)s.S.reserves[c.resource]=BigDouble.Parse(c.target);
    Replay(s,60);
   }
   bool completed=s.S.completedChallenges.Contains(data.challenges[id].id);
   Check("challenge earned through production "+id,completed&&s.S.activeChallenge=="");
   Check("challenge restores original owned economy "+id,s.S.owned.SequenceEqual(originalOwned));
   rows.Add(new{kind="challenge",id,completed,hours=(s.S.time-started)/3600,review=s.Discover.challengeReview});
   Console.WriteLine(JsonSerializer.Serialize(rows.Last()));
  }
  for(int scenario=1;scenario<=3;scenario++)for(int seed=0;seed<3;seed++){
   var s=new Simulation(data,Simulation.ScenarioState(scenario,seed));s.Meta.guidedAutomation=true;
   int resumes=0,turn=0;double nextReload=300;
   while(!s.Discover.scenarioComplete&&s.S.time<172800){
    // Scripted player purchases research and resets through public game operations.
    foreach(var u in data.research)s.Purchase(u);
    foreach(var u in data.upgrades)s.Purchase(u);
    for(int i=0;i<4;i++)s.PrestigeUpgrade(0,i);
    if(s.S.resets[0]==0&&s.PrestigeReward(0)>=1)s.Rebirth(0);
    if(scenario==3){
     // Hungry cosmos needs upstream investment. A player cycles five-minute
     // Energy, Particle, Atom savings and full-chain phases using process pauses.
     s.Meta.guidedAutomation=false;s.S.automation.buy=false;
     for(int i=0;i<8;i++)s.Buy(i,Math.Max(1,s.MaxBuy(i)/4));
     for(int i=0;i<24;i++)s.S.paused[i]=data.generators[i].output>3||(turn%4<3&&data.generators[i].input==turn%4);
     s.InvalidateTickCache();turn++;
    }
    Replay(s,300);
    if(s.S.time+1e-6>=nextReload){s=new Simulation(data,Clone(s.S));resumes++;nextReload+=3600;}
   }
   Check("fresh scenario reaches target through production "+scenario+"/"+seed,s.Discover.scenarioComplete&&s.S.lifetime[3].Value>=BigDouble.Pow10(6+seed));
   Check("fresh scenario retains identity through reloads "+scenario+"/"+seed,s.S.profileId=="scenario-"+scenario+"-"+seed&&s.Constraint==scenario&&resumes>0);
   rows.Add(new{kind="scenario",scenario,seed,complete=s.Discover.scenarioComplete,hours=s.Discover.scenarioFinished/3600,elapsedHours=s.S.time/3600,resumes,matter=s.S.lifetime[3].ToString()});Console.WriteLine(JsonSerializer.Serialize(rows.Last()));
   File.WriteAllText(Path.Combine(root,"Logs/Discovery/scenario-"+scenario+"-"+seed+".json"),JsonSerializer.Serialize(s.S,J));
  }
  File.WriteAllText(Path.Combine(root,"Logs/Discovery/system-audit.json"),JsonSerializer.Serialize(new{pass,fail,rows},J));Environment.ExitCode=fail==0?0:1;
 }
}
