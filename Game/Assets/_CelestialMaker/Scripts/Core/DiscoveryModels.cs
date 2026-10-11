using System;
using System.Collections.Generic;
namespace CelestialMaker.Core {
 [Serializable] public sealed class DiscoveryState {
  public int schema=1;
  public int[] modules=new int[3],routes=new int[8];
  public bool[] signals=new bool[8],lessons=new bool[3];
  public double[] moduleReady=new double[3];
  public List<GoalOrder> goals=new List<GoalOrder>();
  public bool goalsEnabled;
  public int goalIndex;
  public string goalStatus="No goal sequence",challengeReview="";
  public List<ChronicleEntry> chronicle=new List<ChronicleEntry>();
  public List<string> firsts=new List<string>();
  public int role,scenario,seed;
  public bool scenarioComplete;
  public double scenarioFinished;
 }
 [Serializable] public sealed class GoalOrder {
  // 0 = construct a stage; 1 = hold a stock amount.
  public int kind,project,level=4,resource;
  public Number amount;
  public bool holdResets=true;
  public string after="Save";
 }
 [Serializable] public sealed class ChronicleEntry {public double time;public string key,text;}
 public static class DiscoveryContent {
  public static readonly string[][] ModuleNames={
   new[]{"Standard","Collector petals","Relay lattice","Survey array"},
   new[]{"Standard","Closed crucible","Assembly line","Recovery mesh"},
   new[]{"Standard","Refinement lens","Transit bridge","Stable geometry"}};
  public static readonly string[][] ModuleEffects={
   new[]{"Original production and transport.","Primary output +12%; transfer retention -5 percentage points.","Transferred fraction +50%; primary output -7%.","Expeditions 20% faster; Dyson construction 3% slower."},
   new[]{"Original conversion and construction.","Inputs -10%; refined output -4%.","Forge construction +5%; primary output -8%.","Transfer retention +8 percentage points; Forge construction -3%."},
   new[]{"Original refinement and transport.","Refined output +12%; inputs +8%.","Transfer interval -20%; refined output -4%.","Inputs -5%; all construction -1%."}};
  public static readonly string[] Roles={"Balanced","Energy supplier","Refinery","Construction support"};
  public static readonly string[] RoleEffects={"Original parallel economy.","Primary output +12%; refined output -8%. Energy exports +15%.","Refined output +12%; input consumption +6%.","Local output -10%. An Energy delivery to Home supports construction by +2% for one transfer interval. Two suppliers give at most +4%."};
  public static readonly string[] Anomalies={"A hollow star answers your signal.","An isotope cloud repeats a forgotten pattern.","The asteroid contains an abandoned observatory.","A magnetic arc forms a natural relay.","An old nova shelters a stable pocket of matter.","The halo contains a map of possible skies.","The rift echoes a universe that chose differently.","At the horizon, cause and consequence trade places."};
  public static readonly string[] Lessons={"Protect a construction reserve","Trace an input bottleneck","Understand a reset"};
  public static readonly string[] Scenarios={"Campaign","Finite machinery","Closed sky","Hungry cosmos"};
  public static readonly string[] Rules={"Standard experiment.","Maximum 25 units per process. Distribute production across the chain.","Resonance links and expeditions are unavailable. Build a self-sufficient chain.","Inputs cost twice as much. Reduce consumption or increase upstream supply."};
 }
}
