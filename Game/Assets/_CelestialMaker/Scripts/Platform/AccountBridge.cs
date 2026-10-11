using System;
using UnityEngine;
namespace CelestialMaker {
 // Offline migration adapter only. No HTTP, billing SDK or purchase operations.
 public sealed class AccountBridge:MonoBehaviour {
  public static AccountBridge Instance;public Entitlements Owned=new Entitlements();
  void Awake(){Instance=this;try{string raw=PlayerPrefs.GetString("celestial-verified-entitlements","");if(raw!="")Owned=JsonUtility.FromJson<Entitlements>(raw)??new Entitlements();}catch{Owned=new Entitlements();}}
  public void ApplyCached(){
   var sim=GameSession.Instance?.Sim;if(sim==null)return;var e=sim.Meta;
   if(!e.legacyEntitlementsConverted){
    if(Owned.themes||Owned.supporter){if(!e.cosmetics.Contains("theme-1"))e.cosmetics.Add("theme-1");if(!e.cosmetics.Contains("theme-2"))e.cosmetics.Add("theme-2");}
    e.legacySupporter=Owned.supporter;e.legacyAccount=Owned.accountHash??"";e.legacyBoostRecords=Math.Max(0,Owned.unusedBoosts)+(Owned.intervals?.Length??0);e.legacyEntitlementsConverted=true;
   }
   sim.Cosmetics=sim.CosmeticsUnlocked;
  }
  [Serializable]public class Entitlements{public bool themes,supporter;public int unusedBoosts;public string accountHash;public Core.BoostWindow[] intervals;}
 }
}
