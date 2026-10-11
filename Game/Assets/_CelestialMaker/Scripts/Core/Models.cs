using System;
using System.Collections.Generic;
using System.Globalization;
using BreakInfinity;

namespace CelestialMaker.Core {
 [Serializable] public struct Number {
  public static string DisplayNotation="Scientific";
  public double mantissa; public long exponent;
  public Number(BigDouble x) { if(BigDouble.IsNaN(x)||BigDouble.IsInfinity(x)||Math.Abs(x.Exponent)>1000000000) throw new ArithmeticException("Invalid game number"); mantissa=x.Mantissa; exponent=x.Exponent; }
  public BigDouble Value => new BigDouble(mantissa,exponent);
  public static implicit operator Number(BigDouble x)=>new Number(x);
  public override string ToString(){if(BigDouble.Abs(Value)<10000)return Value.ToDouble().ToString("0.##",CultureInfo.InvariantCulture);long e=DisplayNotation=="Engineering"?(long)Math.Floor(exponent/3d)*3:exponent;return (mantissa*Math.Pow(10,exponent-e)).ToString("0.00",CultureInfo.InvariantCulture)+"e"+e;}
 }
 [Serializable] public class Generator { public string id,name,cost,unlock; public int stage,output,input,costResource; public double growth,rate,consume; }
 [Serializable] public class Upgrade { public string id,name,kind,cost,description,requires; public int stage,target,costResource; public double effect; }
 [Serializable] public class Challenge { public string id,name,target,description; public int stage,resource,slowResource; public double slow,reward; }
 [Serializable] public class Achievement { public string id,name,target; public int resource; public double reward; }
 [Serializable] public class Expedition { public string id,name,cost,reward; public int resource,stage; public double duration; }
 [Serializable] public class Story { public string id,name,target,text; public int resource; }
 [Serializable] public class Content { public string[] resources; public Generator[] generators; public Upgrade[] upgrades,research,talents; public Challenge[] challenges; public Achievement[] achievements; public Expedition[] expeditions; public Story[] stories; }
 [Serializable] public class Voyage { public int definition,level,route; public double finish; public bool claimed; }
 [Serializable] public class BoostWindow {public double start,end;}
 [Serializable] public class AutomationProfile { public string name="Balanced"; public bool buy,expeditions; public int[] priority=new int[24]; public Number[] reserves=new Number[12]; public double prestigeThreshold=2; public bool[] repeatPrestige=new bool[3]; }
 [Serializable] public class Settings { public ObservatoryPreferences observatory=new ObservatoryPreferences(); public int fps=60,quality=1,theme; public float music=.25f,sound=.5f,textScale=1; public bool lightUI,reducedMotion,confirmReset=true,tutorial=true; public string notation="Scientific"; }
 [Serializable] public class State {
  public DiscoveryState discovery=new DiscoveryState(); public EvolutionState evolution=new EvolutionState(); public ResearchPlan researchPlan=new ResearchPlan(); public int version=2; public ExpansionState expansion=new ExpansionState(); public bool satellite; public int universeKind; public string profileId=Guid.NewGuid().ToString("N");
  public Number[] resources=new Number[12],lifetime=new Number[12],reserves=new Number[12],prestige=new Number[3];
  public long[] owned=new long[24]; public bool[] paused=new bool[24]; // Legacy field retained for save compatibility; production never reads it.
  public bool[] autobuyers=new bool[6], discoveredProcesses=new bool[24];
  public int[] resets=new int[3],links=new int[6]; public int universeLaw;
  public List<string> purchases=new List<string>(),talents=new List<string>(),achievements=new List<string>(),completedChallenges=new List<string>();
  public List<string> discoveredStories=new List<string>(),prestigePurchases=new List<string>();
  public List<Voyage> voyages=new List<Voyage>(); public int[] expeditionLevels=new int[8];
  public AutomationProfile automation=new AutomationProfile(); public List<AutomationProfile> profiles=new List<AutomationProfile>(); public Settings settings=new Settings();
  public OfflineReport offlineReport;
  public double time,utc,offlineRemaining,liveRemaining; public string activeChallenge=""; public Number[] challengeResources; public long[] challengeOwned; public List<string> challengePurchases; public bool[] challengePaused;
  // Expansion v2 records converted legacy cosmetics without paid production effects.
  public State(){owned[0]=1; resources[0]=new BigDouble(10); for(int i=0;i<24;i++)automation.priority[i]=i; for(int i=0;i<6;i++)links[i]=-1;}
 }
 public static class ResetRules {
  public static readonly string[] Names={"Prestige","Ascension","Genesis"};
  public static readonly string[] Currencies={"Prestige Points","Ascension Points","Genesis Points"};
  public static readonly string[] Reset={"Resources, processes, ordinary upgrades and research","Run economy, unspent Prestige Points, ALL Prestige upgrades, links, active challenges and advanced automation","Run economy, Prestige and Ascension currencies/upgrades, talents, laws and megastructure progress; all parallel economies restart"};
  public const string Kept="Settings, achievements, challenge records, stories, earned cosmetics, history, discovered processes and strategy presets; same/higher tier upgrades. Purchased autobuyers survive Prestige, but reset on Ascension and Genesis.";
 }
}

