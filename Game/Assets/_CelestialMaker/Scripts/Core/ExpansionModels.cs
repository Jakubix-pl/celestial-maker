using System;
using System.Collections.Generic;
namespace CelestialMaker.Core {
 [Serializable] public class ExpansionState {
  public Number prestigeEarned,ascensionEarned;
  public Number[] historicEarned=new Number[3];
  public int[] historicResets=new int[3];
  public List<string> ascensionUpgrades=new List<string>(),genesisUpgrades=new List<string>(),laws=new List<string>(),cosmetics=new List<string>(),records=new List<string>();
  public MegaProject[] megas={new MegaProject(),new MegaProject(),new MegaProject()};
  public StrategyProfile[] strategies={new StrategyProfile{name="Energy"},new StrategyProfile{name="Matter"},new StrategyProfile{name="Time"}};
  public List<UniverseSave> universes=new List<UniverseSave>();
  public List<TransferRule> transfers=new List<TransferRule>();
  public double lawReadyAt,nextAutomation=60,nextHistory=60,prestigeStarted,ascensionStarted,genesisStarted,campaignCompletedAt;
  public int activeUniverse;public bool guidedAutomation=true,autoResearch,autoBuild;
  public string policy="Balanced",migration="native-v2",legacyAccount="";
  public int legacyBoostRecords;public bool legacySupporter,legacyEntitlementsConverted;
  public double transferred;public int completedTransfers;
 }
 [Serializable] public class MegaProject {public int level;public bool building;public double work;}
 [Serializable] public class StrategyProfile {public string focus="Balanced";public string name="Strategy";public List<string> upgrades=new List<string>(),laws=new List<string>();public int[] links={-1,-1,-1,-1,-1,-1};public string policy="Balanced";}
 [Serializable] public class TransferRule {public int from,to,resource;public bool enabled=true;public double next;public string blocked="";}
 [Serializable] public class UniverseSave {
  public int role; public double supportUntil; public bool guided=true,researchAll; public ResearchPlan researchPlan=new ResearchPlan(); public int id,kind;public string name="Parallel universe";
  public Number[] resources=new Number[12],lifetime=new Number[12],reserves=new Number[12];public long[] owned=new long[24];public bool[] paused=new bool[24];
  public List<string> purchases=new List<string>(),laws=new List<string>(),achievements=new List<string>(),stories=new List<string>();
  public AutomationProfile automation=new AutomationProfile();public double time,nextAutomation,lawReadyAt;
  public UniverseSave(){owned[0]=1;resources[0]=new BreakInfinity.BigDouble(10);for(int i=0;i<24;i++)automation.priority[i]=i;automation.buy=true;}
 }
 public sealed class MetaUpgrade {
  public readonly string id,name,description,requires,branch;public readonly int cost,tier;
  public MetaUpgrade(string id,string name,string branch,int tier,int cost,string requires,string description){this.id=id;this.name=name;this.branch=branch;this.tier=tier;this.cost=cost;this.requires=requires;this.description=description;}
 }
 public sealed class RealityLaw {
  public readonly string id,name,description;public readonly int family;public readonly double primary,refined,consumption,automation,expedition;
  public RealityLaw(string id,string name,int family,string description,double primary=1,double refined=1,double consumption=1,double automation=1,double expedition=1){this.id=id;this.name=name;this.family=family;this.description=description;this.primary=primary;this.refined=refined;this.consumption=consumption;this.automation=automation;this.expedition=expedition;}
 }
 public static class ExpansionContent {
  public static readonly MetaUpgrade[] Ascension={
   new MetaUpgrade("a_energy_1","Radiant foundation","Energy",1,1,"","Primary output ×1.8; refined output ×0.9."),
   new MetaUpgrade("a_energy_2","Stellar network","Energy",2,3,"a_energy_1","Primary output ×1.7. Dyson construction ×1.5."),
   new MetaUpgrade("a_energy_3","Sustained ignition","Energy",3,8,"a_energy_2","Primary output ×2; inputs ×1.15. Extra law slot."),
   new MetaUpgrade("a_matter_1","Conserved matter","Matter",1,1,"","Input consumption ×0.65; primary output ×0.9."),
   new MetaUpgrade("a_matter_2","Catalytic lattice","Matter",2,3,"a_matter_1","Refined output ×1.8. Forge construction ×1.5."),
   new MetaUpgrade("a_matter_3","Perfect conversion","Matter",3,8,"a_matter_2","Refined output ×2; input consumption ×0.8."),
   new MetaUpgrade("a_time_1","Patient orchestration","Time",1,1,"","Automation interval ×0.5; expedition duration ×0.7; output ×0.95."),
   new MetaUpgrade("a_time_2","Temporal workshop","Time",2,3,"a_time_1","All construction ×1.5. Automated research unlocks."),
   new MetaUpgrade("a_time_3","Causal routing","Time",3,8,"a_time_2","Expeditions ×0.5 duration; all production output ×1.5."),
  };
  public static readonly MetaUpgrade[] Genesis={
   new MetaUpgrade("g_seed","Persistent seed","Genesis",1,1,"","Start with 8 dynamos after every reset; production ×2."),
   new MetaUpgrade("g_memory","Cosmic memory","Genesis",2,3,"g_seed","Ascension recovery output ×3. Preserve a level in each completed megastructure."),
   new MetaUpgrade("g_horizon","Endless horizon","Genesis",3,2,"g_memory","Production ×3; megastructures can reach level 6."),
  };
  public static readonly RealityLaw[] Laws={
   new RealityLaw("flux_radiance","Radiance",0,"Primary ×1.6 / refined ×0.8",1.6,.8),
   new RealityLaw("flux_density","Density",0,"Refined ×1.6 / inputs ×1.35",1,1.6,1.35),
   new RealityLaw("flux_balance","Equilibrium",0,"Inputs ×0.6 / all output ×0.9",.9,.9,.6),
   new RealityLaw("time_burst","Burst cycles",1,"Automation twice as often / primary ×0.85",.85,1,1,.5),
   new RealityLaw("time_patience","Long cycles",1,"Output ×1.3 / automation interval ×3",1.3,1.3,1,3),
   new RealityLaw("time_voyage","Far journeys",1,"Expeditions twice as fast / refined ×0.85",1,.85,1,1,.5),
   new RealityLaw("space_open","Open geometry",2,"Primary ×1.4 / inputs ×1.25",1.4,1,1.25),
   new RealityLaw("space_fold","Folded geometry",2,"Refined ×1.35 / primary ×0.85",.85,1.35),
   new RealityLaw("space_shelter","Sheltered geometry",2,"Inputs ×0.7 / automation interval ×2",1,1,.7,2)
  };
  public static readonly string[] MegaNames={"Dyson Swarm","Neutron Forge","Reality Loom"};
  public static readonly string[] MegaEffects={"Primary output +25% per level; level 1 accelerates expeditions.","Inputs reduced by 10% per level; level 1 unlocks particle exchange.","Refined output +20% per level; level 1 unlocks a parallel universe."};
  public static readonly int[][] MegaResources={new[]{0,3,6},new[]{2,5,8},new[]{1,7,10}};
 }
}
