using System;
using System.Collections.Generic;
using System.Linq;
using BreakInfinity;
namespace CelestialMaker.Core {
 public sealed partial class Simulation {
  public ExpansionState Meta=>S.expansion;
  readonly Dictionary<int,Simulation> parallel=new Dictionary<int,Simulation>();
  public readonly Queue<ProductionSample> History=new Queue<ProductionSample>();
  public struct ProductionSample {public double time;public Number energy,net;}
  public bool HasA(string id)=>Meta.ascensionUpgrades.Contains(id);
  public bool HasG(string id)=>Meta.genesisUpgrades.Contains(id);
  public bool CosmeticsUnlocked=>S.achievements.Count>=8||Meta.cosmetics.Contains("theme-1")||Meta.cosmetics.Contains("theme-2");
  public bool ThemeUnlocked(int theme)=>theme==0||Meta.cosmetics.Contains("theme-"+theme)||S.achievements.Count>=(theme==1?8:24);
  public int LawSlots=>Math.Min(3,HasLayer(2)?3:HasA("a_energy_3")?3:HasLayer(1)?2:0);
  public bool AdvancedAutomation=>HasLayer(0)||HasLayer(1);
  public BigDouble Gross(int resource){RefreshRates();return gross[resource];}
  public BigDouble CachedGross(int resource)=>gross[resource];
  public BigDouble CachedSpend(int resource)=>BigDouble.Max(0,gross[resource]-rates[resource]);
  public double GoalSeconds(int resource,BigDouble target){RefreshRates();return Boundary(target-S.resources[resource].Value,rates[resource]);}
  public string[] MultiplierContributions(int id){RefreshRates();BigDouble ordinary=1,discoveries=1,challenge=1,prestige=1;foreach(string key in S.purchases){var u=FindUpgrade(key);if(u.target<0||u.target==id)ordinary*=u.effect;}foreach(string key in S.achievements)discoveries*=achievementIndex[key].reward;foreach(string key in S.completedChallenges)challenge*=challengeIndex[key].reward;for(int i=0;i<3;i++)prestige*=BigDouble.Pow(S.prestige[i].Value+1,.25+.15*i);var g=Data.generators[id];var possible=CachedProductionMultiplier(id)*g.rate*S.owned[id];return new[]{"Installed upgrades ×"+new Number(ordinary),"Discoveries ×"+new Number(discoveries)+" / challenges ×"+new Number(challenge),"Unspent meta currency ×"+new Number(prestige),"Trees, cosmic projects and laws ×"+new Number(ExpansionMultiplier(id)),"Combined production multiplier ×"+new Number(CachedProductionMultiplier(id)),S.paused[id]?"Process paused":g.input>=0&&flows[id]<possible*.999999?"Bottleneck: "+Data.resources[g.input]+" / input or protected stock":"No input bottleneck"};}

  void ValidateExpansion(){
   var e=Meta;if(e==null||e.megas?.Length!=3||e.strategies?.Length!=3||e.historicEarned?.Length!=3||e.historicResets?.Length!=3||e.universes==null||e.universes.Count>2||e.laws==null||e.transfers==null||e.transfers.Count>3||e.ascensionUpgrades==null||e.genesisUpgrades==null||e.cosmetics==null||e.records==null||e.completedTransfers<0||e.historicResets.Any(n=>n<0))throw new ArgumentException("Invalid expansion schema");
   foreach(var n in e.historicEarned.Concat(new[]{e.prestigeEarned,e.ascensionEarned}))if(n.Value<0||BigDouble.IsNaN(n.Value)||BigDouble.IsInfinity(n.Value))throw new ArgumentException("Invalid historic currency");
   foreach(double t in new[]{e.lawReadyAt,e.nextAutomation,e.nextHistory,e.prestigeStarted,e.ascensionStarted,e.genesisStarted,e.campaignCompletedAt})if(double.IsNaN(t)||double.IsInfinity(t)||t<0)throw new ArgumentException("Invalid expansion time");
   foreach(var m in e.megas)if(m==null||m.level<0||m.level>6||double.IsNaN(m.work)||double.IsInfinity(m.work)||m.work<0)throw new ArgumentException("Invalid megastructure");
   if(e.ascensionUpgrades.Distinct().Count()!=e.ascensionUpgrades.Count||e.genesisUpgrades.Distinct().Count()!=e.genesisUpgrades.Count)throw new ArgumentException("Duplicate meta upgrade");
   foreach(string id in e.ascensionUpgrades)if(!ExpansionContent.Ascension.Any(u=>u.id==id))throw new ArgumentException("Unknown Ascension upgrade");
   foreach(string id in e.genesisUpgrades)if(!ExpansionContent.Genesis.Any(u=>u.id==id))throw new ArgumentException("Unknown Genesis upgrade");
   if(!ValidLaws(e.laws,3))throw new ArgumentException("Invalid law loadout");
   foreach(var p in e.strategies)if(p==null||p.links?.Length!=6||!ValidLaws(p.laws,3))throw new ArgumentException("Invalid strategy profile");
   var ids=new HashSet<int>{0};foreach(var u in e.universes){
    if(u==null||u.id<1||u.id>e.universes.Count||!ids.Add(u.id)||u.resources?.Length!=12||u.lifetime?.Length!=12||u.reserves?.Length!=12||u.owned?.Length!=24||u.paused?.Length!=24||u.kind<1||u.kind>2||u.purchases==null||u.achievements==null||u.stories==null||!ValidLaws(u.laws,3))throw new ArgumentException("Invalid parallel economy");
    ValidateResearchPlan(u.researchPlan);ValidateProfile(u.automation);foreach(double t in new[]{u.time,u.nextAutomation,u.lawReadyAt})if(double.IsNaN(t)||double.IsInfinity(t)||t<0)throw new ArgumentException("Invalid parallel clock");foreach(var n in u.resources.Concat(u.lifetime).Concat(u.reserves))if(n.mantissa<0||double.IsNaN(n.mantissa)||double.IsInfinity(n.mantissa)||Math.Abs(n.exponent)>1000000000)throw new ArgumentException("Invalid parallel stock");if(u.owned.Any(n=>n<0||n>1000000000)||u.purchases.Any(id=>FindUpgrade(id)==null)||u.achievements.Any(id=>!achievementIndex.ContainsKey(id)))throw new ArgumentException("Invalid parallel progress");
   }
   var sources=new HashSet<int>();foreach(var r in e.transfers)if(r==null||!ids.Contains(r.from)||!ids.Contains(r.to)||!sources.Add(r.from)||r.from==r.to||r.resource<0||r.resource>1||double.IsNaN(r.next)||double.IsInfinity(r.next)||r.next<0)throw new ArgumentException("Invalid transfer schedule");
  }
  public bool BuyMeta(string id,bool genesis=false){
   var u=Array.Find(genesis?ExpansionContent.Genesis:ExpansionContent.Ascension,x=>x.id==id);if(u==null||!HasLayer(genesis?2:1))return false;
   var list=genesis?Meta.genesisUpgrades:Meta.ascensionUpgrades;int layer=genesis?2:1;
   if(list.Contains(id)||u.requires!=""&&!list.Contains(u.requires)||S.prestige[layer].Value<u.cost)return false;
   SettleUniverses();Spend(12+layer,u.cost);S.prestige[layer]=S.prestige[layer].Value-u.cost;list.Add(id);Revision++;return true;
  }
  public void RespecAscension(){SettleUniverses();foreach(string id in Meta.ascensionUpgrades){var u=Array.Find(ExpansionContent.Ascension,x=>x.id==id);S.prestige[1]=S.prestige[1].Value+u.cost;Grant(13,u.cost);}Meta.ascensionUpgrades.Clear();if(Meta.laws.Count>LawSlots)Meta.laws.RemoveRange(LawSlots,Meta.laws.Count-LawSlots);Revision++;}
  static bool ValidLaws(List<string> ids,int capacity){if(ids==null||ids.Count>capacity||ids.Distinct().Count()!=ids.Count)return false;var families=new HashSet<int>();foreach(var id in ids){var law=Array.Find(ExpansionContent.Laws,x=>x.id==id);if(law==null||!families.Add(law.family))return false;}return true;}
  public bool SetLaws(IEnumerable<string> ids){var next=ids.ToList();if(!HasLayer(1)||S.time+1e-7<Meta.lawReadyAt||!ValidLaws(next,LawSlots))return false;Meta.laws=next;Meta.lawReadyAt=S.time+600;Revision++;return true;}
  public void SaveStrategy(int slot){if(slot<0||slot>=3)return;var p=Meta.strategies[slot];p.upgrades=new List<string>(Meta.ascensionUpgrades);p.laws=new List<string>(Meta.laws);p.links=(int[])S.links.Clone();p.policy=Meta.policy;p.focus=Evolution.focus;}
  public bool LoadStrategy(int slot){
   if(slot<0||slot>=3||S.time<Meta.lawReadyAt)return false;var p=Meta.strategies[slot];if(!FocusNames.Contains(p.focus))return false;var budget=S.prestige[1].Value;foreach(var id in Meta.ascensionUpgrades)budget+=Array.Find(ExpansionContent.Ascension,u=>u.id==id).cost;
   var selected=new HashSet<string>();BigDouble cost=0;foreach(var id in p.upgrades){var u=Array.Find(ExpansionContent.Ascension,x=>x.id==id);if(u==null||!selected.Add(id)||u.requires!=""&&!selected.Contains(u.requires))return false;cost+=u.cost;}
   if(cost>budget||!ValidLaws(p.laws,HasLayer(2)||selected.Contains("a_energy_3")?3:HasLayer(1)?2:0))return false;
   int used=0;for(int i=0;i<6;i++){if(p.links[i]<-1||p.links[i]>5||p.links[i]==i)return false;if(p.links[i]>=0)used++;}if(used>1+S.resets[1]+S.resets[2])return false;
   RespecAscension();foreach(string id in p.upgrades)BuyMeta(id);SetLaws(p.laws);S.links=(int[])p.links.Clone();Meta.policy=p.policy;Evolution.focus=p.focus;Evolution.focusReadyAt=S.time+600;Revision++;return true;
  }
  double LawFactor(int property){double factor=1;foreach(string id in Meta.laws){RealityLaw l=null;foreach(var candidate in ExpansionContent.Laws)if(candidate.id==id){l=candidate;break;}factor*=property==0?l.primary:property==1?l.refined:property==2?l.consumption:property==3?l.automation:l.expedition;}return factor;}
  BigDouble ExpansionMultiplier(int generator){
   bool primary=Data.generators[generator].output%2==0;double m=1;
   if(HasLayer(1))m*=4;if(HasLayer(2))m*=6;
   if(HasA("a_energy_1"))m*=primary?1.8:.9;if(HasA("a_energy_2")&&primary)m*=1.7;if(HasA("a_energy_3")&&primary)m*=2;
   if(HasA("a_matter_1")&&primary)m*=.9;if(HasA("a_matter_2")&&!primary)m*=1.8;if(HasA("a_matter_3")&&!primary)m*=2;
   if(HasA("a_time_1"))m*=.95;if(HasA("a_time_3"))m*=1.5;
   if(HasG("g_seed"))m*=2;if(HasG("g_memory"))m*=3;if(HasG("g_horizon"))m*=3;
   m*=primary?1+.25*Meta.megas[0].level:1+.2*Meta.megas[2].level;
   m*=LawFactor(primary?0:1);
   if(S.satellite)m*=S.universeKind==1?(primary?1.5:.75):(primary?.75:1.5);
   if(Meta.records.Contains("law-triad"))m*=1.05;if(Meta.records.Contains("exchange-network"))m*=1.05;return m*DiscoveryOutput(primary);
  }
  double InputFactor(){double f=HasA("a_matter_1")?.65:1;if(HasA("a_matter_3"))f*=.8;if(HasA("a_energy_3"))f*=1.15;return f*Math.Pow(.9,Meta.megas[1].level)*LawFactor(2)*DiscoveryInputs;}
  int PlannedMega(){var goal=CurrentGoal;if(goal!=null&&goal.kind==0){int p=goal.project;return !Meta.megas[p].building&&Meta.megas[p].level<goal.level?p:-1;}int best=-1;for(int i=0;i<3;i++)if(!Meta.megas[i].building&&Meta.megas[i].level<(HasG("g_horizon")?6:1)&&(best<0||Meta.megas[i].level<Meta.megas[best].level))best=i;return best;}
  BigDouble EffectiveReserve(int resource){var reserve=BigDouble.Max(S.reserves[resource].Value,GoalReserve(resource));if(!S.satellite&&Meta.autoBuild&&HasLayer(1)){int p=PlannedMega();if(p>=0)for(int slot=0;slot<3;slot++)if(ExpansionContent.MegaResources[p][slot]==resource)return reserve+MegaCost(p,slot);}return reserve;}
  public double AutomationInterval=>Math.Max(5,60*(HasA("a_time_1")?.5:1)*LawFactor(3));
  double ExpeditionFactor=>(HasA("a_time_1")?.7:1)*(HasA("a_time_3")?.5:1)*(Meta.megas[0].level>0?.8:1)*LawFactor(4)*(Module(0)==3?.8:1);
  public BigDouble MegaCost(int project,int slot)=>BigDouble.Pow10(4+ExpansionContent.MegaResources[project][slot]*.65+Meta.megas[project].level*1.2);
  public double MegaWork(int project)=>ProgressionBalance.ConstructionBaseSeconds*Math.Pow(ProgressionBalance.ConstructionGrowth,Meta.megas[project].level);
  double MegaSpeed(int project)=>DiscoveryWork(project)*FocusWork(project)*(HasA("a_time_2")?1.5:1)*(project==0&&HasA("a_energy_2")||project==1&&HasA("a_matter_2")?1.5:1);
  public bool StartMega(int project){
   if(S.satellite||project<0||project>=3||!HasLayer(1))return false;var m=Meta.megas[project];if(m.building||m.level>=(HasG("g_horizon")?6:3))return false;
   var resources=ExpansionContent.MegaResources[project];for(int i=0;i<3;i++)if(!Meets(S.resources[resources[i]].Value-BigDouble.Max(S.reserves[resources[i]].Value,GoalReserve(resources[i])),MegaCost(project,i)))return false;
   for(int i=0;i<3;i++){var cost=MegaCost(project,i);Spend(resources[i],cost);S.resources[resources[i]]=BigDouble.Max(0,S.resources[resources[i]].Value-cost);}
   m.building=true;m.work=0;Revision++;return true;
  }
  public double MegaSeconds(int project)=>Meta.megas[project].building?Math.Max(0,MegaWork(project)-Meta.megas[project].work)/MegaSpeed(project):double.PositiveInfinity;
  void AdvanceExpansion(double dt){
   if(!S.satellite)for(int i=0;i<3;i++){var m=Meta.megas[i];if(!m.building)continue;m.work+=dt*MegaSpeed(i);if(m.work+1e-6>=MegaWork(i)){m.level++;m.building=false;m.work=0;Record("mega-"+i+"-"+m.level);}}
  }
  void Record(string id){if(!Meta.records.Contains(id))Meta.records.Add(id);}
  void ExpansionEvents(){TrackRecovery();CheckCampaignCompletion();if(Meta.laws.Count==3)Record("law-triad");if(Meta.completedTransfers>=3)Record("exchange-network");
   if(S.achievements.Count>=8&&!Meta.cosmetics.Contains("theme-1"))Meta.cosmetics.Add("theme-1");if(S.achievements.Count>=24&&!Meta.cosmetics.Contains("theme-2"))Meta.cosmetics.Add("theme-2");
   if(S.time+1e-7>=Meta.nextHistory){RefreshRates();History.Enqueue(new ProductionSample{time=S.time,energy=S.resources[0],net=new Number(rates[0])});while(History.Count>120)History.Dequeue();Meta.nextHistory=S.time+60;}
   if(S.time+1e-7<Meta.nextAutomation)return;Meta.nextAutomation=S.time+AutomationInterval;
   DiscoveryEvents();if(RunGoal())return;RunDevelopmentPurchases();RunResearchPlan();
   if(Meta.autoBuild&&!S.satellite&&HasLayer(1))StartMega(PlannedMega());if(S.automation.buy){if(Meta.guidedAutomation)GuidedPurchases();else for(int j=0;j<24;j++){int i=S.automation.priority[j];long old=S.owned[i];Buy(i,MaxBuy(i));if(Report!=null)Report.automaticUnits[i]+=S.owned[i]-old;}}
   if(Meta.autoResearch&&HasA("a_time_2")&&(!S.researchPlan.automatic||S.researchPlan.queue.Count==0))foreach(var u in Data.research)if(ResearchAvailable(u.costResource)>=BigDouble.Parse(u.cost))Purchase(u);
   if(HasLayer(1)&&S.automation.expeditions){foreach(var v in S.voyages)if(!v.claimed&&v.finish<=S.time+1e-7)Claim(v);for(int i=0;i<8;i++)if(ResearchAvailable(0)>=BigDouble.Parse(Data.expeditions[i].cost)*BigDouble.Pow(2,S.expeditionLevels[i]))StartVoyage(i);}
   if(GoalHoldsResets)return;
   if(Evolution.developmentPlan!=""&&!S.satellite){RunDevelopmentReset();return;}
   if(AdvancedAutomation)for(int i=2;i>=0;i--)if(S.resets[i]>0&&S.automation.repeatPrestige[i]&&(i!=0||Meta.prestigeEarned.Value<ProgressionBalance.PrestigeForAscension)&&PrestigeReward(i)>=S.automation.prestigeThreshold){Rebirth(i);break;}
  }
  void GuidedPurchases(){
   int missing=Array.FindIndex(Data.generators,g=>Open(g)&&S.owned[Array.IndexOf(Data.generators,g)]==0);
   int currency=missing>=0?Data.generators[missing].costResource:-1;
   var goal=missing<0?Array.Find(Data.upgrades,u=>StageOpen(u.stage)&&!S.purchases.Contains(u.id)):null;if(goal!=null)currency=goal.costResource;
   // Auto routing is opt-in and declares its ownership of pauses in the UI.
   for(int i=0;i<24;i++)S.paused[i]=currency>=0&&Data.generators[i].input==currency;
   if(missing>=0&&Buy(missing,1)){if(Report!=null)Report.automaticUnits[missing]++;Array.Clear(S.paused,0,24);}
   for(int j=0;j<24;j++){int i=S.automation.priority[j];if(Data.generators[i].costResource==currency)continue;long old=S.owned[i];Buy(i,Math.Max(1,MaxBuy(i)/4));if(Report!=null)Report.automaticUnits[i]+=S.owned[i]-old;}
   if(goal!=null&&ResearchAvailable(goal.costResource)>=BigDouble.Parse(goal.cost)&&Purchase(goal))Array.Clear(S.paused,0,24);
   // Basic routing restores routine research between visits; an explicit plan takes priority.
   if(HasLayer(0)&&S.researchPlan.queue.Count==0)foreach(var u in Data.research)if(ResearchAvailable(u.costResource)>=BigDouble.Parse(u.cost))Purchase(u);
   
  }
 }
}
