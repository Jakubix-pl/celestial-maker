using System;
namespace CelestialMaker.Core {
 public static class FixedStepClock {
  public const double Step=1d/60;
  public static int Advance(Simulation sim,double elapsed,int maxSteps=8){
   if(double.IsNaN(elapsed)||double.IsInfinity(elapsed)||elapsed<0)throw new ArgumentException("Invalid elapsed time");
   sim.S.liveRemaining+=elapsed;int steps=0;
   sim.InvalidateTickCache();
   while(sim.S.liveRemaining+1e-10>=Step&&steps<maxSteps){sim.AdvanceTick();sim.S.liveRemaining=Math.Max(0,sim.S.liveRemaining-Step);steps++;}
   // Backlog is retained. The runtime moves a large backlog into the same
   // resumable tick replay used for offline time; no analytical shortcut.
   return steps;
  }
 }
}
