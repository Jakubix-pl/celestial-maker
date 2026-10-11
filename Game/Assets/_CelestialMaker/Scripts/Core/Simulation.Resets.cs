using System;
using System.Linq;
using BreakInfinity;
namespace CelestialMaker.Core {
 public sealed partial class Simulation {
  public BigDouble PrestigeReward(int layer){
   if(layer<0||layer>2||S.satellite)return BigDouble.Zero;
   if(layer==0){double log=BigDouble.Log10(PrestigeProduction);return log<ProgressionBalance.PrestigeEnergyLog?BigDouble.Zero:BigDouble.Floor(BigDouble.Pow10((log-ProgressionBalance.PrestigeEnergyLog)/3)+1e-10);}
   if(layer==1){if(!StageOpen(3))return BigDouble.Zero;return BigDouble.Floor(BigDouble.Sqrt(Meta.prestigeEarned.Value/ProgressionBalance.PrestigeForAscension)+1e-10);}
   if(!StageOpen(5)||Meta.megas.Any(m=>m.level<1))return BigDouble.Zero;
   return BigDouble.Floor(BigDouble.Pow(Meta.ascensionEarned.Value/ProgressionBalance.AscensionForGenesis,1d/3)+1e-10)*(1+S.resets[2]);
  }
  public string ResetRequirement(int layer)=>layer==0?"Produce 1.59e6 Energy or 4.53e6 Particles in this run. Prestige uses the larger of Energy and 0.35 × Particles, never their sum. Reward follows a cube-root curve.":layer==1?$"Reach Stars and earn {ProgressionBalance.PrestigeForAscension} Prestige Points since Ascension, including spent points. Reward = floor(sqrt(earned / {ProgressionBalance.PrestigeForAscension})).":$"Reach Universes, finish a stage of each megastructure and earn {ProgressionBalance.AscensionForGenesis} Ascension Points since Genesis. Reward = floor(cuberoot(earned / {ProgressionBalance.AscensionForGenesis})) × (1 + completed Genesis resets).";
  public double RecoveryEstimate(int layer)=>layer==0?Math.Pow((S.prestige[0].Value+PrestigeReward(0)+1).ToDouble()/(S.prestige[0].Value+1).ToDouble(),.25):layer==1?4:6;
  public bool Rebirth(int layer){
   if(S.activeChallenge!=""||S.satellite||layer<0||layer>2)return false;var reward=PrestigeReward(layer);if(reward<1)return false;
   SettleUniverses();if(BeforeReset!=null&&!BeforeReset(layer))return false;RememberReset(layer,reward);Grant(12+layer,reward);S.prestige[layer]=S.prestige[layer].Value+reward;Meta.historicEarned[layer]=Meta.historicEarned[layer].Value+reward;Meta.historicResets[layer]++;S.resets[layer]++;
   if(layer==0)Meta.prestigeEarned=Meta.prestigeEarned.Value+reward;if(layer==1)Meta.ascensionEarned=Meta.ascensionEarned.Value+reward;
   if(Report!=null){Report.resets[layer]++;Report.Capture(this);for(int r=0;r<12;r++)Report.Add(Report.resetLoss,r,S.resources[r].Value);for(int i=0;i<layer;i++)Report.Add(Report.resetLoss,12+i,S.prestige[i].Value);}
   for(int i=0;i<layer;i++){S.prestige[i]=BigDouble.Zero;S.resets[i]=0;int lower=i;S.prestigePurchases.RemoveAll(x=>x.StartsWith("p"+lower+"_"));}
   Array.Fill(S.discoveredProcesses,true);
   S.resources=new Number[12];S.lifetime=new Number[12];S.reserves=new Number[12];S.owned=new long[24];S.owned[0]=HasG("g_seed")?8:HasLayer(1)?6:1;S.resources[0]=new BigDouble(10);Grant(0,10);S.paused=new bool[24];S.purchases.Clear();
   RestoreResearchPrerequisites();
   S.voyages.Clear();S.expeditionLevels=new int[8];Meta.prestigeStarted=S.time;
   if(layer>0){
    Meta.prestigeEarned=BigDouble.Zero;Meta.ascensionStarted=S.time;for(int i=0;i<6;i++)S.links[i]=-1;
    Array.Clear(S.autobuyers,0,6);S.automation=new AutomationProfile();for(int i=0;i<24;i++)S.automation.priority[i]=i;Meta.autoResearch=false;
    S.challengeResources=null;S.challengeOwned=null;S.challengePurchases=null;S.challengePaused=null;
    ResetUniverses();
   }
   if(layer>1){
    Meta.ascensionEarned=BigDouble.Zero;Meta.genesisStarted=S.time;Meta.ascensionUpgrades.Clear();S.talents.Clear();Meta.laws.Clear();Meta.lawReadyAt=S.time;
    foreach(var mega in Meta.megas){mega.level=HasG("g_memory")&&mega.level>0?1:0;mega.work=0;mega.building=false;}
   }
   Record("reset-"+layer);Meta.nextAutomation=S.time+AutomationInterval;Revision++;return true;
  }
 }
}
