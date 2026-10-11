using System;
using System.Linq;
using System.Text.Json;
using BreakInfinity;
using CelestialMaker.Core;

partial class Program {
 static void PrestigeAutomationTests(){
  var dust=new Simulation(data,JsonSerializer.Deserialize<State>(System.IO.File.ReadAllText(System.IO.Path.Combine(root,"Tools/Expansion/Fixtures/routing-dust.json")),J));dust.S.offlineRemaining=0;dust.S.offlineReport=null;double dustStart=dust.S.time;var dustWatch=System.Diagnostics.Stopwatch.StartNew();Replay(dust,600);Check("guided routing dust replays ten minutes without stalling",Math.Abs(dust.S.time-dustStart-600)<.01&&dustWatch.Elapsed.TotalSeconds<10&&Accounted(dust));
  var fresh=new Simulation(data);fresh.S.automation.buy=true;fresh.Meta.guidedAutomation=true;fresh.S.resources[0]=new BigDouble(100000);
  Advance(fresh,120);Check("old enabled rule cannot grant a free autobuyer",fresh.S.owned.Sum()==1&&!fresh.AnyAutobuyer);
  Check("fresh game cannot buy a reset unlock",!fresh.UnlockAutobuyer(0));
  fresh.S.lifetime[0]=BigDouble.Pow10(ProgressionBalance.PrestigeEnergyLog);Check("first Prestige awards an affordable buyer",fresh.Rebirth(0)&&fresh.S.prestige[0].Value==1&&fresh.UnlockAutobuyer(0)&&fresh.S.prestige[0].Value==0);
  Check("unlock rejects duplicate invalid and unfunded purchase",!fresh.UnlockAutobuyer(0)&&!fresh.UnlockAutobuyer(-1)&&!fresh.UnlockAutobuyer(6)&&!fresh.UnlockAutobuyer(1));
  fresh.S.resources[0]=new BigDouble(100000);fresh.S.lifetime[1]=new BigDouble(10000);fresh.S.resources[1]=new BigDouble(100000);fresh.Meta.guidedAutomation=false;Advance(fresh,120);
  for(int i=0;i<4;i++){fresh.S.resources[0]=new BigDouble(100000);fresh.S.automation.priority=Enumerable.Range(0,24).OrderBy(n=>n==i?0:1).ToArray();Advance(fresh,60);}
  Check("only purchased chapter buys its own four processes",fresh.S.owned.Take(4).All(n=>n>0)&&fresh.S.owned.Skip(4).All(n=>n==0));
  var reload=new Simulation(data,Clone(fresh.S));Check("paid unlock persists through serialization",reload.S.autobuyers[0]&&reload.S.autobuyers.Count(x=>x)==1);
  reload.S.lifetime[0]=BigDouble.Pow10(ProgressionBalance.PrestigeEnergyLog);Check("Prestige preserves paid buyers",reload.Rebirth(0)&&reload.S.autobuyers[0]);
  reload.S.lifetime[5]=new BigDouble(1e9);reload.Meta.prestigeEarned=new BigDouble(8);Check("Ascension clears buyers and active purchase intent",reload.Rebirth(1)&&!reload.AnyAutobuyer&&!reload.S.automation.buy);
  Check("all buildings remain discovered after higher reset",reload.AllProcessesDiscovered&&reload.DiscoverProcesses()==(1<<24)-1);
  var legacy=new State();legacy.paused[0]=true;legacy.autobuyers=null;legacy.discoveredProcesses=null;legacy.prestige[0]=new BigDouble(19);legacy.owned[1]=2;legacy.automation.buy=true;
  var migrated=new Simulation(data,legacy);Check("legacy migration resumes production without spending or free unlocks",!migrated.S.paused.Any(x=>x)&&migrated.S.prestige[0].Value==19&&migrated.S.owned[1]==2&&!migrated.AnyAutobuyer);
  string round=JsonSerializer.Serialize(migrated.S,J);Check("new migration is idempotent",JsonSerializer.Serialize(SaveMigrations.Upgrade(Clone(migrated.S)),J)==round);
  migrated.S.paused[0]=migrated.S.paused[1]=true;Check("legacy pause flags have no production effect",migrated.Gross(0)>0&&migrated.Gross(1)>0);
  var reveal=new Simulation(data);double rate=reveal.Rate(0).ToDouble();var cost=reveal.Cost(1);reveal.S.resources[0]=cost-rate*30.01;reveal.DiscoverProcesses();Check("building hidden beyond 30 second horizon",!reveal.ProcessDiscovered(1));
  reveal.S.resources[0]=cost-rate*30;reveal.DiscoverProcesses();Check("building revealed at exact 30 second boundary",reveal.ProcessDiscovered(1));
  reveal.S.resources[0]=BigDouble.Zero;reveal.S.owned[0]=0;reveal.DiscoverProcesses();Check("discovered building does not flicker after spending",reveal.ProcessDiscovered(1));
  var empty=new Simulation(data);empty.S.owned[0]=0;empty.DiscoverProcesses();Check("zero income cannot reveal unaffordable building",!empty.ProcessDiscovered(1));
  empty.S.resources[1]=new BigDouble(1e10);empty.DiscoverProcesses();Check("stock alone cannot reveal unreachable locked chapter",!empty.ProcessDiscovered(4));
  var nav=new Simulation(data);Check("initial navigation contains only unlocked categories",Enumerable.Range(0,6).Where(nav.SectionUnlocked).SequenceEqual(new[]{0,1}));
  nav.S.lifetime[0]=BigDouble.Pow10(ProgressionBalance.PrestigeEnergyLog);Check("development appears when first reset is ready",nav.SectionUnlocked(2)&&!nav.SectionUnlocked(3));nav.Rebirth(0);Check("first reset discovers automation and buildings",nav.SectionUnlocked(3)&&nav.DiscoverProcesses()==(1<<24)-1);
  var routing=new Simulation(data);routing.S.resets[0]=1;routing.S.prestige[0]=new BigDouble(1);routing.UnlockAutobuyer(0);routing.S.automation.buy=routing.Meta.guidedAutomation=true;routing.S.owned[1]=100;routing.S.resources[0]=BigDouble.Zero;
  Check("guided saving keeps both input and output flowing",routing.Rate(0)>0&&routing.Gross(1)>0&&!routing.S.paused.Any(x=>x));
  var offline=new Simulation(data,Clone(routing.S));var online=new Simulation(data,Clone(routing.S));offline.BeginOffline(600);while(offline.S.offlineRemaining>0)offline.ProcessOffline(257);for(int i=0;i<36000;i++)online.AdvanceTick();Check("paid autobuy online and literal offline ticks agree",SameEconomy(offline,online)&&Accounted(offline));
  var world=Mature();world.S.prestige[0]=new BigDouble(1);world.UnlockAutobuyer(0);world.Meta.megas[2].level=1;world.CreateUniverse(1);Check("parallel economy shares only paid home unlocks",world.Universe(1).AutobuyerUnlocked(0)&&!world.Universe(1).AutobuyerUnlocked(4)&&!world.Universe(1).UnlockAutobuyer(1));
  var bootstrap=Mature();bootstrap.Meta.historicResets[0]=1;bootstrap.S.prestige[0]=BigDouble.Zero;bootstrap.ConfigureDevelopmentPlan("Energy");Advance(bootstrap,60);Advance(bootstrap,bootstrap.AutomationInterval+1);Check("Genesis preparation allows paid automation recovery",bootstrap.Meta.historicResets[0]>1&&bootstrap.S.autobuyers.All(x=>x));
  var hold=Mature();hold.Meta.historicResets[0]=1;hold.ConfigureDevelopmentPlan("Energy");hold.QueueGoal(new GoalOrder{kind=1,resource=0,amount=new Number(BigDouble.Parse("1e30")),holdResets=true});hold.EnableGoals();Advance(hold,120);Check("goal reset hold overrides automation funding recovery",hold.Meta.historicResets[0]==1&&!hold.AnyAutobuyer);
 }
}
