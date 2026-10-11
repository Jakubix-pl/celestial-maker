using System;
using System.Linq;
using System.Collections.Generic;
using BreakInfinity;

namespace CelestialMaker.Core {
 public sealed partial class Simulation {
  public readonly Content Data; public State S {get;private set;}
  readonly BigDouble[] rates=new BigDouble[12],gross=new BigDouble[12],flows=new BigDouble[24],consumptions=new BigDouble[24];
  readonly BigDouble[] installedMultipliers=new BigDouble[24],protectedStocks=new BigDouble[12];
  static readonly string[,] PrestigeIds={{"p0_0","p0_1","p0_2","p0_3"},{"p1_0","p1_1","p1_2","p1_3"},{"p2_0","p2_1","p2_2","p2_3"}};BigDouble expansionPrimary=1,expansionRefined=1;double expansionInput=1;
  readonly bool[] linkedSources=new bool[6];bool unlinkedCalculation;
  static readonly BigDouble[] StageTargets={1,100,10000,1000000,100000000,10000000000};readonly Dictionary<string,BigDouble> achievementTargets=new Dictionary<string,BigDouble>(),storyTargets=new Dictionary<string,BigDouble>();
  readonly Dictionary<string,Upgrade> upgradeIndex=new Dictionary<string,Upgrade>(),talentIndex=new Dictionary<string,Upgrade>();
  readonly Dictionary<string,Challenge> challengeIndex=new Dictionary<string,Challenge>();
  readonly Dictionary<string,Achievement> achievementIndex=new Dictionary<string,Achievement>();
  public double PremiumUntil; public bool Cosmetics; public int Revision;public Action<int,long,double> OnBought; public Func<int,bool> BeforeReset;
  public readonly List<BoostWindow> BoostWindows=new List<BoostWindow>();
  OfflineReport Report=>S.offlineReport!=null&&S.offlineReport.active?S.offlineReport:null;
  void Spend(int resource,BigDouble amount){Report?.Add(Report.spent,resource,amount);}
  void Grant(int resource,BigDouble amount){Report?.Add(Report.granted,resource,amount);}
  public void BeginOffline(double absentSeconds){
   if(double.IsNaN(absentSeconds)||double.IsInfinity(absentSeconds)||absentSeconds<0)throw new ArgumentException("Invalid absence");
   absentSeconds+=S.liveRemaining;S.liveRemaining=0;
   if(S.offlineRemaining<=1e-7&&absentSeconds<=1e-7)return;
   if(Report==null||Report.requestedSeconds<=0){S.offlineReport=new OfflineReport{requestedSeconds=S.offlineRemaining,legacyRemainder=S.offlineRemaining>0};S.offlineReport.Capture(this,true);}
   S.offlineRemaining+=absentSeconds;S.offlineReport.requestedSeconds+=absentSeconds;
  }
  public void ProcessOffline(int budget=8192,double maxMilliseconds=0){
   if(budget<1||double.IsNaN(maxMilliseconds)||maxMilliseconds<0)throw new ArgumentException("Invalid offline budget");
   if(S.offlineRemaining<=1e-7)return;if(Report==null)BeginOffline(0);
   InvalidateTickCache();foreach(var world in Meta.universes)Universe(world.id).InvalidateTickCache();
   var report=Report;long started=System.Diagnostics.Stopwatch.GetTimestamp();double initial=S.offlineRemaining,advanced=0;int count=0;
   while(initial-advanced>1e-7&&count<budget){
    double duration=Math.Min(FixedStepClock.Step,initial-advanced);AdvanceTick(duration);advanced+=duration;count++;
    if(duration>=FixedStepClock.Step-1e-7)report.fixedTicks++;
    if(maxMilliseconds>0&&(count&31)==0&&(System.Diagnostics.Stopwatch.GetTimestamp()-started)*1000d/System.Diagnostics.Stopwatch.Frequency>=maxMilliseconds)break;
   }
   S.offlineRemaining=Math.Max(0,initial-advanced);report.processedSeconds+=advanced;report.Capture(this);SettleUniverses();
   if(S.offlineRemaining<=1e-7){S.offlineRemaining=0;report.processedSeconds=report.requestedSeconds;report.active=false;}
  }
  public Simulation(Content data,State state=null){Data=data;ContentValidator.Validate(Data);foreach(var u in Data.upgrades)upgradeIndex.Add(u.id,u);foreach(var u in Data.research)upgradeIndex.Add(u.id,u);foreach(var u in Data.talents)talentIndex.Add(u.id,u);foreach(var c in Data.challenges)challengeIndex.Add(c.id,c);foreach(var a in Data.achievements){achievementIndex.Add(a.id,a);achievementTargets.Add(a.id,BigDouble.Parse(a.target));}foreach(var story in Data.stories)storyTargets.Add(story.id,BigDouble.Parse(story.target));S=SaveMigrations.Upgrade(state??new State());Validate();}
  public void Validate(){
   ValidateResearchPlan(S.researchPlan);ValidateExpansion();ValidateEvolution();ValidateDiscovery();
   var prefs=S.settings?.observatory;if(prefs==null||prefs.favorites==null||prefs.favorites.Count>256||prefs.favorites.Any(x=>x==null||x.Length>128)||prefs.researchFilter<0||prefs.researchFilter>2||prefs.upgradeFilter<0||prefs.upgradeFilter>2||prefs.productionStage< -1||prefs.productionStage>5||prefs.availability<0||prefs.availability>2||prefs.goalWorld<0||prefs.goalWorld>2||prefs.goal==null||prefs.goal.Length>128||prefs.researchSearch==null||prefs.researchSearch.Length>80||prefs.upgradeSearch==null||prefs.upgradeSearch.Length>80)throw new ArgumentException("Invalid Observatory preferences");if(S.version!=2||S.resources==null||S.resources.Length!=12||S.owned==null||S.owned.Length!=24||S.lifetime.Length!=12||S.reserves.Length!=12||S.paused.Length!=24||S.prestige.Length!=3||S.resets.Length!=3||S.links.Length!=6||S.expeditionLevels.Length!=8||S.settings==null||S.automation==null)throw new ArgumentException("Incompatible save schema");
   if(double.IsNaN(S.time)||double.IsInfinity(S.time)||S.time<0||double.IsNaN(S.utc)||double.IsInfinity(S.utc)||double.IsNaN(S.offlineRemaining)||double.IsInfinity(S.offlineRemaining)||S.offlineRemaining<0||double.IsNaN(S.liveRemaining)||double.IsInfinity(S.liveRemaining)||S.liveRemaining<0)throw new ArgumentException("Invalid save time");
   foreach(var arr in new[]{S.resources,S.lifetime,S.reserves,S.prestige})foreach(var n in arr)if(double.IsNaN(n.mantissa)||double.IsInfinity(n.mantissa)||n.mantissa<0||Math.Abs(n.exponent)>1000000000)throw new ArgumentException("Invalid save value");
   foreach(var n in S.owned)if(n<0||n>1000000000)throw new ArgumentException("Invalid process count");
   foreach(var v in S.voyages)if(v.route<0||v.route>2||v.definition<0||v.definition>=8||v.level<0||v.level>100000||double.IsNaN(v.finish)||double.IsInfinity(v.finish))throw new ArgumentException("Invalid expedition");
   foreach(var id in S.purchases)if(FindUpgrade(id)==null)throw new ArgumentException("Unknown upgrade");
   foreach(var id in S.talents)if(talentIndex[id]==null)throw new ArgumentException("Unknown talent");
   if(S.purchases.Count!=new HashSet<string>(S.purchases).Count||S.talents.Count!=new HashSet<string>(S.talents).Count||S.achievements.Count!=new HashSet<string>(S.achievements).Count)throw new ArgumentException("Duplicate save entries");
   foreach(var id in S.achievements)if(Array.Find(Data.achievements,x=>x.id==id)==null)throw new ArgumentException("Unknown achievement");
   ValidateProfile(S.automation);foreach(var profile in S.profiles)ValidateProfile(profile);
   foreach(int link in S.links)if(link< -1||link>5)throw new ArgumentException("Invalid resonance target");
   if(S.offlineReport!=null)ValidateReport(S.offlineReport);
   foreach(var id in S.completedChallenges)if(Array.Find(Data.challenges,x=>x.id==id)==null)throw new ArgumentException("Unknown challenge record");
   foreach(var n in S.resets)if(n<0)throw new ArgumentException("Invalid prestige count");
   if(S.universeLaw<0||S.universeLaw>2||S.settings.quality<0||S.settings.quality>1||S.settings.fps!=30&&S.settings.fps!=60&&S.settings.fps!=120||S.settings.theme<0||S.settings.theme>2||float.IsNaN(S.settings.textScale)||S.settings.textScale<.8f||S.settings.textScale>2f||float.IsNaN(S.settings.sound)||float.IsNaN(S.settings.music)||S.settings.sound<0||S.settings.sound>1||S.settings.music<0||S.settings.music>1)throw new ArgumentException("Invalid game settings");
   if(S.activeChallenge!=""&&challengeIndex[S.activeChallenge]==null)throw new ArgumentException("Unknown challenge");
  }
  static void ValidateProfile(AutomationProfile p){if(p==null||p.reserves==null||p.reserves.Length!=12||p.priority==null||p.priority.Length!=24||new HashSet<int>(p.priority).Count!=24||p.repeatPrestige==null||p.repeatPrestige.Length!=3||double.IsNaN(p.prestigeThreshold)||double.IsInfinity(p.prestigeThreshold)||p.prestigeThreshold<1)throw new ArgumentException("Invalid automation profile");foreach(int i in p.priority)if(i<0||i>=24)throw new ArgumentException("Invalid automation priority");}
  static void ValidateReport(OfflineReport r){
   if(r.fixedTicks<0)throw new ArgumentException("Invalid offline tick count");
   foreach(var arr in new[]{r.before,r.after,r.produced,r.spent,r.resetLoss,r.granted}){if(arr==null||arr.Length!=15)throw new ArgumentException("Invalid offline report");foreach(var n in arr)if(double.IsNaN(n.mantissa)||double.IsInfinity(n.mantissa)||n.mantissa<0||Math.Abs(n.exponent)>1000000000)throw new ArgumentException("Invalid report value");}
   foreach(double time in new[]{r.requestedSeconds,r.processedSeconds,r.boostSeconds})if(double.IsNaN(time)||double.IsInfinity(time)||time<0)throw new ArgumentException("Invalid report time");
   if(r.knownBefore?.Length!=15||r.knownAfter?.Length!=15||r.stagesBefore?.Length!=6||r.stagesAfter?.Length!=6||r.automaticUnits?.Length!=24||r.resets?.Length!=3||r.finishedExpeditions?.Length!=8||r.claimedExpeditions?.Length!=8||r.discoveries==null||r.stories==null||r.challenges==null||r.processedSeconds>r.requestedSeconds+.1)throw new ArgumentException("Invalid report aggregate");
  }
  public bool HasLayer(int layer){for(int i=layer;i<3;i++)if(S.resets[i]>0)return true;return false;}
  public static bool Meets(BigDouble value,BigDouble target)=>value>=target||value>0&&target>0&&Math.Abs((value/target).ToDouble()-1)<=1e-12;
  public bool StageOpen(int stage)=>stage==0||Meets(S.lifetime[Math.Max(0,stage*2-1)].Value,StageTargets[stage]);
  public bool Open(Generator g)=>StageOpen(g.stage);
  public Upgrade FindUpgrade(string id)=>upgradeIndex.TryGetValue(id,out var result)?result:null;
  public int TalentCount(int specialization){int count=0;foreach(string id in S.talents)if(talentIndex[id].target==specialization)count++;return count;}
  public double TalentMultiplier(int specialization){double multiplier=1;foreach(string id in S.talents){var talent=talentIndex[id];if(talent.target==specialization)multiplier*=talent.effect;}return multiplier;}
  public BigDouble Cost(int i,long count=1){var g=Data.generators[i]; return BigDouble.Parse(g.cost)*BigDouble.Pow(g.growth,S.owned[i])*(BigDouble.Pow(g.growth,count)-1)/(g.growth-1);}
  public long MaxBuy(int i){var g=Data.generators[i];var available=BigDouble.Max(0,S.resources[g.costResource].Value-EffectiveReserve(g.costResource));var ratio=available*(g.growth-1)/(BigDouble.Parse(g.cost)*BigDouble.Pow(g.growth,S.owned[i]))+1;var n=(long)Math.Max(0,Math.Min(1000000000-S.owned[i],Math.Floor(BigDouble.Log(ratio,g.growth))));if(n>0&&!Meets(available,Cost(i,n)))n--;if(n<1000000000-S.owned[i]&&Meets(available,Cost(i,n+1)))n++;return Constraint==1?Math.Min(n,Math.Max(0,25-S.owned[i])):n;}
  public bool Buy(int i,long count){if(i<0||i>=24||!Open(Data.generators[i]))return false; count=Math.Min(count,MaxBuy(i));if(count<=0)return false;var r=Data.generators[i].costResource;var cost=Cost(i,count);Spend(r,cost);S.resources[r]=BigDouble.Max(0,S.resources[r].Value-cost);S.owned[i]+=count;Revision++;OnBought?.Invoke(i,count,S.time);return true;}
  public long NextMilestone(int i){long n=S.owned[i];return n<10?10-n:n<25?25-n:n<50?50-n:100-n%100;}
  public bool Purchase(Upgrade u){if(u==null||!StageOpen(u.stage)||S.purchases.Contains(u.id)||u.requires!=""&&!S.purchases.Contains(u.requires))return false;var c=BigDouble.Parse(u.cost);if(S.resources[u.costResource].Value<c)return false;Spend(u.costResource,c);S.resources[u.costResource]=S.resources[u.costResource].Value-c;S.purchases.Add(u.id);S.researchPlan.queue.Remove(u.id);Revision++;return true;}
  public bool Talent(Upgrade u){if(!HasLayer(1)||S.talents.Contains(u.id)||u.requires!=""&&!S.talents.Contains(u.requires))return false;var cost=BigDouble.Parse(u.cost);if(S.prestige[1].Value<cost)return false;Spend(13,cost);S.prestige[1]=S.prestige[1].Value-cost;S.talents.Add(u.id);Revision++;return true;}
  public void Respec(){foreach(var id in S.talents){var u=talentIndex[id];Grant(13,BigDouble.Parse(u.cost));S.prestige[1]=S.prestige[1].Value+BigDouble.Parse(u.cost);}S.talents.Clear();Revision++;}
  public bool Link(int stage,int target){int slots=1+S.resets[1]+S.resets[2];if(Constraint==2||!HasLayer(0)||stage<0||stage>=6||target< -1||target>=6||target==stage)return false;int used=0;for(int i=0;i<6;i++)if(S.links[i]>=0&&i!=stage)used++;if(target>=0&&used>=slots)return false;S.links[stage]=target;Revision++;return true;}
  public bool PrestigeUpgrade(int layer,int node){if(layer<0||layer>2||node<0||node>3||S.resets[layer]==0)return false;string id="p"+layer+"_"+node;var cost=BigDouble.Pow(4,node);if(S.prestigePurchases.Contains(id)||S.prestige[layer].Value<cost)return false;Spend(12+layer,cost);S.prestige[layer]=S.prestige[layer].Value-cost;S.prestigePurchases.Add(id);Revision++;return true;}
  public void SetLaw(int law){if(law>=0&&law<3&&SetLaws(new[]{ExpansionContent.Laws[law].id}))S.universeLaw=law;}
  void ComputeInstalledMultipliers(){
   for(int i=0;i<24;i++)installedMultipliers[i]=1;
   foreach(string id in S.purchases){var u=FindUpgrade(id);if(u.target<0){for(int i=0;i<24;i++)installedMultipliers[i]*=u.effect;}else installedMultipliers[u.target]*=u.effect;}
   BigDouble common=1;foreach(string id in S.talents){var u=talentIndex[id];if(u.target==0)common*=u.effect;}
   foreach(string id in S.achievements)common*=achievementIndex[id].reward;foreach(string id in S.completedChallenges)common*=challengeIndex[id].reward;
   for(int j=0;j<3;j++)common*=BigDouble.Pow(S.prestige[j].Value+1,.25+.15*j);
   for(int i=0;i<24;i++){var g=Data.generators[i];var m=installedMultipliers[i]*common*(1+Math.Floor(S.owned[i]/25.0)*.15);m*=g.output%2==0?expansionPrimary:expansionRefined;
    for(int layer=0;layer<3;layer++)for(int n=0;n<4;n++)if(S.prestigePurchases.Contains(PrestigeIds[layer,n])&&g.stage/2==layer)m*=1.4+n*.1;
    if(S.activeChallenge!=""){var c=challengeIndex[S.activeChallenge];if(g.output==c.slowResource)m*=c.slow;}installedMultipliers[i]=m;
   }
  }
  BigDouble Multiplier(int i){var g=Data.generators[i];var m=installedMultipliers[i];
   if(!unlinkedCalculation&&Constraint!=2)for(int j=0;j<6;j++)if(S.links[j]==g.stage&&linkedSources[j])m*=1+.4*TalentMultiplier(1);
   return m;
  }
  public BigDouble Flow(int i){ComputeRates();return flows[i];}
  public void RefreshRates(){ComputeRates();}
  public BigDouble CachedFlow(int i)=>flows[i];
  public BigDouble CachedRate(int i)=>rates[i];
  public BigDouble CachedConsumption(int i)=>consumptions[i];
  public BigDouble ProductionMultiplier(int i){ComputeRates();return CachedProductionMultiplier(i);}
  public BigDouble CachedProductionMultiplier(int i)=>Multiplier(i)*(Constraint!=2&&S.links[Data.generators[i].stage]>=0&&linkedSources[Data.generators[i].stage]?.85:1);
  public BigDouble Consumption(int i){ComputeRates();return consumptions[i];}
  public BigDouble Rate(int i){ComputeRates();return rates[i];}
  void ComputeRates(){expansionPrimary=ExpansionMultiplier(0);expansionRefined=ExpansionMultiplier(1);expansionInput=InputFactor();ComputeInstalledMultipliers();unlinkedCalculation=true;ComputeAllocation();Array.Clear(linkedSources,0,6);for(int i=0;i<24;i++)if(flows[i]>0)linkedSources[Data.generators[i].stage]=true;unlinkedCalculation=false;ComputeAllocation();}
  void ComputeAllocation(double horizon=0){Array.Clear(rates,0,12);Array.Clear(gross,0,12);Array.Clear(flows,0,24);Array.Clear(consumptions,0,24);
   // Resource graph is acyclic. All producers of an input are accounted for
   // before its consumers are allocated; generator display order is irrelevant.
   for(int i=0;i<24;i++){var g=Data.generators[i];if(S.paused[i]||S.owned[i]==0)continue;flows[i]=Multiplier(i)*g.rate*S.owned[i];}
   for(int resource=0;resource<12;resource++){for(int i=0;i<24;i++)if(Data.generators[i].output==resource){int stage=Data.generators[i].stage;if(!unlinkedCalculation&&Constraint!=2&&S.links[stage]>=0&&linkedSources[stage])flows[i]*=.85;gross[resource]+=flows[i];}var protectedStock=protectedStocks[resource]=EffectiveReserve(resource);// Ignore floating-point dust smaller than one minimum integration of incoming flow.
    bool stored=S.resources[resource].Value>protectedStock&&!Meets(protectedStock,S.resources[resource].Value)&&S.resources[resource].Value-protectedStock>gross[resource]*1e-7;bool filling=!Meets(S.resources[resource].Value,protectedStock);var available=filling?BigDouble.Zero:gross[resource];if(stored&&horizon>0)available+=(S.resources[resource].Value-protectedStock)/horizon;BigDouble consumed=0;
    for(int i=0;i<24;i++){var g=Data.generators[i];if(g.input!=resource||flows[i]==0)continue;double coefficient=g.consume*expansionInput;var demand=flows[i]*coefficient;var used=stored&&horizon<=0?demand:BigDouble.Min(demand,BigDouble.Max(0,available));if(!stored||horizon>0)available-=used;flows[i]=used/coefficient;consumptions[i]=used;consumed+=used;}
    rates[resource]=filling?gross[resource]:stored||horizon>0?gross[resource]-consumed:available;
   }
  }
  double Boundary(BigDouble distance,BigDouble rate){if(distance<=0||rate<=0)return double.PositiveInfinity;var t=(distance/rate).ToDouble();return double.IsNaN(t)?double.PositiveInfinity:Math.Max(1e-7,t);}
  // Advances between production depletion, achievement, purchase, voyage and boost boundaries.
  // No loop over seconds; callers provide a bounded event budget and preserve unconsumed time.
  public double Advance(double elapsed,int eventBudget=512){InvalidateTickCache();if(double.IsNaN(elapsed)||double.IsInfinity(elapsed)||elapsed<0)throw new ArgumentException("Invalid duration");int events=0;double remaining=elapsed;
   while(remaining>1e-7&&events++<eventBudget){ResolveEvents();ComputeRates();double dt=NextBoundary(remaining);
    // Bound a sub-quantum depletion by the stock actually available in this integration.
    if(dt<=1.01e-7)ComputeAllocation(dt);Integrate(dt);remaining=Math.Max(0,remaining-dt);
   }ResolveEvents();return remaining;
  }
  double NextBoundary(double maximum){double dt=maximum;if(Meta.autoBuild||Meta.autoResearch||S.researchPlan.automatic&&S.researchPlan.queue.Count>0||S.automation.buy||S.automation.expeditions||Array.Exists(S.automation.repeatPrestige,x=>x))dt=Math.Min(dt,Math.Max(1e-7,Meta.nextAutomation-S.time));if(!S.satellite){for(int m=0;m<3;m++)dt=Math.Min(dt,MegaSeconds(m));dt=Math.Min(dt,NextTransferBoundary());}
    for(int stage=1;stage<6;stage++)if(!StageOpen(stage))dt=Math.Min(dt,Boundary(StageTargets[stage]-S.lifetime[stage*2-1].Value,gross[stage*2-1]));
    if(S.activeChallenge!=""){var goal=challengeIndex[S.activeChallenge];dt=Math.Min(dt,Boundary(BigDouble.Parse(goal.target)-S.resources[goal.resource].Value,rates[goal.resource]));}
    foreach(var v in S.voyages)if(!v.claimed&&v.finish>S.time+1e-7)dt=Math.Min(dt,v.finish-S.time);
    for(int r=0;r<12;r++)if(rates[r]<0)dt=Math.Min(dt,Boundary(S.resources[r].Value-EffectiveReserve(r),-rates[r]));
    for(int r=0;r<12;r++)if(rates[r]>0&&!Meets(S.resources[r].Value,EffectiveReserve(r)))dt=Math.Min(dt,Boundary(EffectiveReserve(r)-S.resources[r].Value,rates[r]));
    foreach(var a in Data.achievements)if(!S.achievements.Contains(a.id))dt=Math.Min(dt,Boundary(achievementTargets[a.id]-S.lifetime[a.resource].Value,gross[a.resource]));
    return Math.Max(1e-7,DiscoveryBoundary(dt));
  }
  void Integrate(double dt,bool stable=false){
    dt=Math.Max(1e-7,dt);var report=Report;
    for(int r=0;r<12;r++){var delta=rates[r]*dt;var raw=S.resources[r].Value+delta;
     // Subtraction of very large flows can round a boundary below its reserve.
     // Keep protected stock exact and account only for the actual consumed stock.
     var floor=rates[r]<0?BigDouble.Min(S.resources[r].Value,protectedStocks[r]):BigDouble.Zero;var next=BigDouble.Max(floor,raw);
     if(report!=null){report.Add(report.produced,r,gross[r]*dt);var spent=BigDouble.Max(0,gross[r]-rates[r])*dt;report.Add(report.spent,r,BigDouble.Max(0,spent-BigDouble.Max(0,next-raw)));}S.resources[r]=next;S.lifetime[r]=S.lifetime[r].Value+gross[r]*dt;
    }
    if(report!=null)foreach(var voyage in S.voyages)if(!voyage.claimed&&voyage.finish>S.time+1e-7&&voyage.finish<=S.time+dt+1e-7)report.finishedExpeditions[voyage.definition]++;
    AdvanceExpansion(dt);AdvanceUniverses(dt);double increment=dt-clockCompensation;double sum=S.time+increment;clockCompensation=(sum-S.time)-increment;S.time=sum;if(!stable)ResolveTransfers();if(!ticking)report?.Capture(this);Revision++;
  }
  void ResolveEvents(){ExpansionEvents();foreach(var a in Data.achievements)if(!S.achievements.Contains(a.id)&&Meets(S.lifetime[a.resource].Value,achievementTargets[a.id])){S.achievements.Add(a.id);Report?.discoveries.Add(a.id);}
   foreach(var story in Data.stories)if(!S.discoveredStories.Contains(story.id)&&Meets(S.lifetime[story.resource].Value,storyTargets[story.id])){S.discoveredStories.Add(story.id);Report?.stories.Add(story.id);}
   if(S.activeChallenge!=""){var c=challengeIndex[S.activeChallenge];if(Meets(S.resources[c.resource].Value,BigDouble.Parse(c.target))){if(!S.completedChallenges.Contains(c.id)){S.completedChallenges.Add(c.id);Report?.challenges.Add(c.id);}ExitChallenge();}}
  }
  public bool EnterChallenge(int i){if(i<0||i>=12||S.activeChallenge!=""||!HasLayer(0)||!StageOpen(Data.challenges[i].stage))return false;S.challengeResources=(Number[])S.resources.Clone();S.challengeOwned=(long[])S.owned.Clone();S.challengePurchases=new List<string>(S.purchases);S.challengePaused=(bool[])S.paused.Clone();S.resources=new Number[12];S.resources[0]=new BigDouble(10);S.owned=new long[24];S.owned[0]=1;S.purchases=new List<string>();S.paused=new bool[24];S.activeChallenge=Data.challenges[i].id;Revision++;return true;}
  public void ExitChallenge(){if(S.activeChallenge=="")return;var challenge=challengeIndex[S.activeChallenge];RefreshRates();Discover.challengeReview=challenge.name+" / "+(Meets(S.resources[challenge.resource].Value,BigDouble.Parse(challenge.target))?"Completed":"Stopped")+" / reached "+S.resources[challenge.resource]+" of "+challenge.target+" / net "+new Number(rates[challenge.resource])+" per second. "+ConstraintDescription;Chronicle("challenge-review",challenge.name+": trial ended");if(Report!=null)for(int r=0;r<12;r++){var change=S.challengeResources[r].Value-S.resources[r].Value;if(change>0)Grant(r,change);else Report.Add(Report.resetLoss,r,-change);}S.resources=S.challengeResources;S.owned=S.challengeOwned;S.purchases=S.challengePurchases;S.paused=S.challengePaused;S.challengeResources=null;S.challengeOwned=null;S.challengePurchases=null;S.challengePaused=null;S.activeChallenge="";Revision++;}
  public bool StartVoyage(int i){if(i<0||i>=8||Constraint==2||!HasLayer(1)||!StageOpen(Data.expeditions[i].stage)||S.voyages.Exists(x=>x.definition==i&&!x.claimed))return false;var d=Data.expeditions[i];int level=S.expeditionLevels[i];var cost=BigDouble.Parse(d.cost)*BigDouble.Pow(2,level);if(S.resources[0].Value<cost)return false;Spend(0,cost);S.resources[0]=S.resources[0].Value-cost;S.voyages.RemoveAll(x=>x.claimed&&x.definition==i);S.voyages.Add(new Voyage{definition=i,level=level,route=Discover.routes[i],finish=S.time+d.duration*ExpeditionFactor*(Discover.routes[i]==1?.85:Discover.routes[i]==2?1.2:1)/TalentMultiplier(2)});Revision++;return true;}
  public bool Claim(Voyage v){if(v.claimed||v.finish>S.time+1e-7)return false;v.claimed=true;var d=Data.expeditions[v.definition];var reward=BigDouble.Parse(d.reward)*BigDouble.Pow(2,v.level)*(v.route==1?.85:v.route==2?1.2:1);Discover.signals[v.definition]=true;Chronicle("signal-"+v.definition,DiscoveryContent.Anomalies[v.definition],true);Grant(d.resource,reward);if(Report!=null)Report.claimedExpeditions[v.definition]++;S.resources[d.resource]=S.resources[d.resource].Value+reward;S.lifetime[d.resource]=S.lifetime[d.resource].Value+reward;S.expeditionLevels[v.definition]++;Revision++;return true;}
 }
}












