using System;

namespace CelestialMaker.Core {
 public sealed partial class Simulation {
  bool ticking,tickCache;
  double stableSeconds,clockCompensation;
  int stableRevision;
  public void InvalidateTickCache(){tickCache=false;}
  void PrepareTickWindow(){
   ComputeRates();stableSeconds=NextBoundary(double.PositiveInfinity);
   // Journal and story changes also invalidate a stable production interval.
   stableSeconds=Math.Min(stableSeconds,Math.Max(1e-7,Meta.nextHistory-S.time));
   stableSeconds=Math.Min(stableSeconds,Math.Max(1e-7,Meta.nextAutomation-S.time));
   foreach(var story in Data.stories)if(!S.discoveredStories.Contains(story.id))stableSeconds=Math.Min(stableSeconds,Boundary(storyTargets[story.id]-S.lifetime[story.resource].Value,gross[story.resource]));
   if(!S.satellite)foreach(var world in Meta.universes){var other=Universe(world.id);other.InvalidateTickCache();}
   stableRevision=Revision;tickCache=true;
  }
  // Executes a real production integration for every quantum. Only unchanged
  // rate calculations are cached; time, stock, lifetime and construction are
  // updated every tick. Boundary ticks use the authoritative event resolver.
  public void AdvanceTick(double duration=FixedStepClock.Step){
   if(duration<=0||duration>FixedStepClock.Step+1e-7||double.IsNaN(duration))throw new ArgumentException("Invalid tick duration");
   bool prior=ticking;ticking=true;
   try{
    if(tickCache&&stableRevision==Revision&&duration<stableSeconds-1e-7){Integrate(duration,true);stableSeconds-=duration;stableRevision=Revision;}
    else{
     double pending=duration;int rounds=0;do{pending=Advance(pending,512);if(++rounds>400)throw new InvalidOperationException("Tick failed to settle at "+S.time+" pending="+pending);}while(pending>1e-7);
     PrepareTickWindow();
    }
   }finally{ticking=prior;}
  }
 }
}
