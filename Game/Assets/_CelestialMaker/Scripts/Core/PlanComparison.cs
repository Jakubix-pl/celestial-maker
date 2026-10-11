using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
namespace CelestialMaker.Core {
 // Owns independent simulations. The UI reads immutable published snapshots;
 // no Unity APIs, live simulation references or save callbacks enter this worker.
 public sealed class PlanComparison {
  public readonly PlanExperiment first,second;
  CancellationTokenSource cancellation;
  volatile bool running;
  volatile string snapshot,error="";
  public bool Running=>running;
  public string Error=>error;
  public string Snapshot=>snapshot;
  public PlanComparison(PlanExperiment first,PlanExperiment second){this.first=first;this.second=second;Publish();}
  void Publish(){snapshot="CURRENT PLAN\n"+first.Summary()+"\n\nVARIANT\n"+second.Summary();}
  public bool Start(double seconds=30){
   if(running||first.Done&&second.Done)return false;
   cancellation?.Dispose();cancellation=new CancellationTokenSource();var token=cancellation.Token;error="";running=true;
   Task.Run(()=>{
    try{var timer=Stopwatch.StartNew();double next=0;
     while(!token.IsCancellationRequested&&(!first.Done||!second.Done)&&timer.Elapsed.TotalSeconds<seconds){
      if(!first.Done)first.Step(8192,25);if(!second.Done)second.Step(8192,25);
      if(timer.Elapsed.TotalSeconds>=next){Publish();next=timer.Elapsed.TotalSeconds+.2;}
     }
     Publish();
    }catch(Exception e){error=e.Message;}
    finally{running=false;}
   });return true;
  }
  public void Cancel(){cancellation?.Cancel();}
 }
}
