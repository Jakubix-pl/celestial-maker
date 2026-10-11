using CelestialMaker.Core;

namespace CelestialMaker {
 public sealed partial class GameUI {
  // Discovery is not the current balance: spending or a higher reset must not
  // hide a currency the save already knows. Reuse persisted progression history.
  bool ResetCurrencyVisible(int layer)=>Sim.HasLayer(layer)||S.prestige[layer].Value>0||
   Sim.Meta.historicEarned[layer].Value>0||Sim.Meta.historicResets[layer]>0||
   (layer==0&&Sim.Meta.prestigeEarned.Value>0)||(layer==1&&Sim.Meta.ascensionEarned.Value>0)||
   Sim.PrestigeReward(layer)>=1;
  static bool ResourceVisible(Simulation sim,int resource)=>sim.StageOpen(resource/2)||sim.S.resources[resource].Value>0||sim.S.lifetime[resource].Value>0;
  string VisibleResetRequirement(int layer){
   if(layer>0&&!ResetCurrencyVisible(layer-1))return layer==1?
    "Reach Stars and develop Prestige to discover this reset.":
    "Reach Universes, complete a stage of each megastructure and develop Ascension to discover this reset.";
   return Sim.ResetRequirement(layer);
  }
 }
}
