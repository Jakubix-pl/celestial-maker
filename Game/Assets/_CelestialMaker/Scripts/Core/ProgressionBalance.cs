using System;
using System.Linq;
using BreakInfinity;

namespace CelestialMaker.Core {
 public static class ProgressionBalance {
  public const string Version="2.8.2";
  public const double PrestigeEnergyLog=6.2;
  public const double FirstPrestigeEnergyLog=5.4;
  public const int PrestigeForAscension=8;
  public const int AscensionForGenesis=12;
  public const double ParticleEnergyEquivalent=.35;
  public const double ConstructionFirstStageSeconds=14400;
  public const double ConstructionBaseSeconds=24400;
  public const double ConstructionGrowth=2.06;
  public const int PrestigeStarterDynamos=3,AscensionStarterDynamos=12,CosmicSeedDynamos=24;
 }
 public sealed partial class Simulation {
  public BigDouble PrestigeProduction=>BigDouble.Max(S.lifetime[0].Value,S.lifetime[1].Value*ProgressionBalance.ParticleEnergyEquivalent);
  public bool AllAscensionNodes=>ExpansionContent.Ascension.All(u=>HasA(u.id));
  public bool AllGenesisNodes=>ExpansionContent.Genesis.All(u=>HasG(u.id));
  public bool CampaignRequirementsMet=>!S.satellite&&Meta.megas.All(m=>m.level>=6)&&AllGenesisNodes&&AllAscensionNodes&&Meta.universes.Count==2;
  public bool CampaignComplete=>Meta.records.Contains("campaign-complete");
  public bool PreparingGenesis=>!AllGenesisNodes&&Meta.ascensionEarned.Value>=ProgressionBalance.AscensionForGenesis;
  void CheckCampaignCompletion(){if(CampaignComplete||!CampaignRequirementsMet)return;Record("campaign-complete");Meta.campaignCompletedAt=S.time;Revision++;}
 }
}
