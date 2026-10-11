using System;
using BreakInfinity;
namespace CelestialMaker.Core {
 public sealed class StarterTip {
  public int step,process=-1;public Upgrade upgrade;public string title,detail,action,route="Production";
 }
 public sealed partial class Simulation {
  // Keep first-run purchase currencies flowing without pausing any consumer.
  // All saved production is real output; no resources or points are granted.
  public bool StarterSupplyProtection=>!S.satellite&&Discover.scenario==0&&S.activeChallenge==""&&S.resets[0]==0&&S.resets[1]==0&&S.resets[2]==0&&Meta.historicResets[0]==0&&Meta.historicResets[1]==0&&Meta.historicResets[2]==0&&S.prestige[0].Value==0&&Meta.prestigeEarned.Value==0&&Meta.ascensionEarned.Value==0&&Meta.historicEarned[0].Value==0&&Meta.historicEarned[1].Value==0&&Meta.historicEarned[2].Value==0;
  public long ProcessCap=>Constraint==1?25:1000000000;
  public bool ProcessMaxed(int id)=>id>=0&&id<24&&S.owned[id]>=ProcessCap;
  public StarterTip FirstSteps(){
   if(!StarterSupplyProtection){if(HasLayer(0)&&!AnyAutobuyer)return new StarterTip{step=6,title="Unlock your first autobuyer",detail="Spend 1 earned Prestige Point on the Particles package. It buys four processes automatically. The package stays through Prestige; higher resets clear it.",action="Open Automation",route="Automation"};return new StarterTip{step=7,title="First connections complete",detail="Keep growing production. Automation has optional Energy, Matter and Time plans, plus a safe autobuyer lesson.",action="Open Automation",route=HasLayer(0)?"Automation":"Overview"};}
   if(PrestigeReward(0)>=1)return new StarterTip{step=5,title="Your first Prestige is ready",detail="Review the reset before confirming. You trade this run's resources and processes for an earned Prestige Point, then unlock an autobuyer.",action="Review Prestige",route="Prestige"};
   if(S.owned[0]<3)return new StarterTip{step=0,process=0,title="Grow your Energy supply",detail="Build up to 3 Vacuum Dynamos. Production runs continuously; each new Dynamo makes the next purchase easier.",action="Buy Vacuum Dynamo"};
   if(S.owned[1]<1)return new StarterTip{step=1,process=1,title="Discover Particles",detail="Buy a Particle Loom. It turns Energy into Particles. During your first run, consumers leave at least 20% of incoming supply available for purchases.",action="Buy Particle Loom"};
   if(S.owned[2]<1)return new StarterTip{step=2,process=2,title="Add a second Energy source",detail="The Zero-point Array makes Energy without an input. It helps feed the Loom while you save for research.",action="Buy Zero-point Array"};
   var first=FindUpgrade("r0");if(!S.purchases.Contains(first.id))return new StarterTip{step=3,upgrade=first,title="Make your first discovery",detail="Quantum memory 1 improves every process by 12%. Later ranks require the previous discovery. Yellow bars show funding; green means you can buy.",action="Research Quantum memory 1",route="Research"};
   var tip=new StarterTip{step=4,title="Grow toward your first Prestige",detail="Keep investing in Energy. This suggestion compares the Energy capacity gained per cost; it spends nothing until you choose Buy. Lifetime production counts even after you spend it."};
   RefreshRates();BigDouble best=0;
   foreach(int id in new[]{0,2}){if(ProcessMaxed(id))continue;var value=CachedProductionMultiplier(id)*Data.generators[id].rate/Cost(id);if(value>best){best=value;tip.process=id;tip.upgrade=null;}}
   foreach(var u in Data.upgrades){if(u.stage!=0||S.purchases.Contains(u.id)||(u.target!=0&&u.target!=2)||S.owned[u.target]==0)continue;var value=CachedFlow(u.target)*(u.effect-1)/BigDouble.Parse(u.cost);if(value>best){best=value;tip.process=-1;tip.upgrade=u;}}
   foreach(var u in Data.research){if(u.stage!=0||S.purchases.Contains(u.id)||(u.requires!=""&&!S.purchases.Contains(u.requires)))continue;var value=CachedGross(0)*(u.effect-1)/BigDouble.Parse(u.cost);if(value>best){best=value;tip.process=-1;tip.upgrade=u;}}
   tip.action=tip.upgrade!=null?"Acquire "+tip.upgrade.name:tip.process>=0?"Buy Max / "+Data.generators[tip.process].name:"Open Production";return tip;
  }
 }
}
