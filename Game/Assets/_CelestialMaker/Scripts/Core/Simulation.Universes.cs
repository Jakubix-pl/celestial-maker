using System;
using System.Collections.Generic;
using System.Linq;
using BreakInfinity;
namespace CelestialMaker.Core {
 public sealed partial class Simulation {
  public int UniverseCapacity=>HasLayer(2)?3:HasLayer(1)&&Meta.megas[2].level>0?2:1;
  public bool CreateUniverse(int kind){
   if(S.satellite||kind<1||kind>2||Meta.universes.Count+1>=UniverseCapacity)return false;
   int id=Meta.universes.Count+1;Meta.universes.Add(new UniverseSave{id=id,kind=kind,name=kind==1?"Radiant parallel":"Dense parallel",time=S.time,nextAutomation=S.time+60});Record("universe-"+id);return true;
  }
  public Simulation Universe(int id){
   if(id==0)return this;var save=Meta.universes.Find(u=>u.id==id);if(save==null)return null;
   if(!parallel.TryGetValue(id,out var sim)){
    var state=new State{researchPlan=save.researchPlan,satellite=true,universeKind=save.kind,resources=save.resources,lifetime=save.lifetime,reserves=save.reserves,owned=save.owned,paused=save.paused,purchases=save.purchases,automation=save.automation,achievements=save.achievements,discoveredStories=save.stories,time=save.time};
    state.discovery.role=save.role;state.expansion.guidedAutomation=save.guided;state.expansion.autoResearch=save.researchAll;state.expansion.laws=save.laws;state.expansion.lawReadyAt=save.lawReadyAt;state.expansion.nextAutomation=save.nextAutomation;sim=new Simulation(Data,state);parallel[id]=sim;
   }
   // Meta benefits are shared, while stocks, installed processes, purchases,
   // automation, laws and local discoveries are separate economies.
   sim.S.prestige=(Number[])S.prestige.Clone();sim.S.resets=(int[])S.resets.Clone();sim.Meta.ascensionUpgrades=Meta.ascensionUpgrades;sim.Meta.genesisUpgrades=Meta.genesisUpgrades;
   sim.Meta.megas=Meta.megas.Select(m=>new MegaProject{level=m.level}).ToArray();return sim;
  }
  void SaveUniverse(int id,Simulation sim){
   var u=Meta.universes.Find(x=>x.id==id);u.guided=sim.Meta.guidedAutomation;u.researchAll=sim.Meta.autoResearch;u.researchPlan=sim.S.researchPlan;u.resources=sim.S.resources;u.lifetime=sim.S.lifetime;u.reserves=sim.S.reserves;u.owned=sim.S.owned;u.paused=sim.S.paused;u.purchases=sim.S.purchases;u.automation=sim.S.automation;u.achievements=sim.S.achievements;u.stories=sim.S.discoveredStories;u.time=sim.S.time;u.laws=sim.Meta.laws;u.lawReadyAt=sim.Meta.lawReadyAt;u.nextAutomation=sim.Meta.nextAutomation;
  }
  public void EditUniverse(int id,Action<Simulation> edit){var sim=Universe(id);if(sim==null)return;CatchUpUniverse(id,sim,S.time);edit(sim);if(id>0)SaveUniverse(id,sim);Revision++;}
  void CatchUpUniverse(int id,Simulation sim,double target){if(id==0)return;double pending=Math.Max(0,target-sim.S.time);int budget=0;while(pending>1e-7&&budget++<64)pending=sim.Advance(pending,256);if(pending>1e-7)throw new InvalidOperationException("Parallel event budget exhausted; resume from saved local clock");SaveUniverse(id,sim);}
  double NextTransferBoundary(){
   if(S.satellite)return double.PositiveInfinity;double next=double.PositiveInfinity;
   foreach(var rule in Meta.transfers)if(rule.enabled)next=Math.Min(next,Math.Max(1e-7,rule.next-S.time));
   // Analytical forecasts also stop at parallel decision boundaries. The live
   // clock and offline replay integrate each parallel economy on every tick.
   foreach(var u in Meta.universes)if(u.automation.buy||u.researchPlan.automatic&&u.researchPlan.queue.Count>0)next=Math.Min(next,Math.Max(1e-7,u.nextAutomation-S.time));
   return next;
  }
  void AdvanceUniverses(double dt){if(S.satellite)return;double end=S.time+dt;
   if(ticking){foreach(var u in Meta.universes){if(!parallel.TryGetValue(u.id,out var other)||!tickCache){other=Universe(u.id);other.InvalidateTickCache();}double pending=end-other.S.time;while(pending>1e-7){double before=other.S.time;other.AdvanceTick(Math.Min(FixedStepClock.Step,pending));if(other.S.time<=before)throw new InvalidOperationException("Parallel clock stalled at "+before+" target="+end+" pending="+pending);pending=end-other.S.time;}u.time=other.S.time;u.nextAutomation=other.Meta.nextAutomation;}return;}
   foreach(var u in Meta.universes.ToArray())if(end+1e-7>=u.nextAutomation||!u.automation.buy&&end-u.time>=60)CatchUpUniverse(u.id,Universe(u.id),end);
  }
  public void SettleUniverses(){if(S.satellite)return;foreach(var u in Meta.universes.ToArray())CatchUpUniverse(u.id,Universe(u.id),S.time);}
  public bool ScheduleTransfer(int from,int to,int resource){
   if(!HasLayer(1)||S.satellite||from==to||Universe(from)==null||Universe(to)==null||resource<0||resource>1||resource==1&&Meta.megas[1].level<1)return false;
   var existing=Meta.transfers.Find(t=>t.from==from);if(existing!=null){existing.to=to;existing.resource=resource;existing.enabled=true;existing.next=Math.Max(existing.next,S.time+TransferInterval);}
   else Meta.transfers.Add(new TransferRule{from=from,to=to,resource=resource,next=S.time+TransferInterval});return true;
  }
  void ResolveTransfers(){
   if(S.satellite)return;
   foreach(var rule in Meta.transfers.OrderBy(t=>t.from)){
    if(!rule.enabled||rule.next>S.time+1e-7)continue;rule.next=S.time+TransferInterval;
    var source=Universe(rule.from);var destination=Universe(rule.to);if(source==null||destination==null){rule.blocked="Universe unavailable";continue;}
    CatchUpUniverse(rule.from,source,S.time);CatchUpUniverse(rule.to,destination,S.time);
    var spare=BigDouble.Max(0,source.S.resources[rule.resource].Value-source.EffectiveReserve(rule.resource));var amount=BigDouble.Min(spare,spare*TransferFraction(rule.resource)*(rule.resource==0&&source.Discover.role==1?1.15:1));
    if(amount<1){rule.blocked="Insufficient unreserved stock for a transfer";continue;}
    source.S.resources[rule.resource]=source.S.resources[rule.resource].Value-amount;source.Spend(rule.resource,amount);
    destination.S.resources[rule.resource]=destination.S.resources[rule.resource].Value+amount*TransferRetention;destination.Grant(rule.resource,amount*TransferRetention);
    // Imported stock is NOT lifetime production, preventing reset-award loops.
    if(rule.from>0&&rule.to==0&&rule.resource==0){var supplier=Meta.universes.Find(u=>u.id==rule.from);if(supplier.role==3)supplier.supportUntil=S.time+TransferInterval;}source.InvalidateTickCache();destination.InvalidateTickCache();rule.blocked="";Meta.completedTransfers++;Record("exchange");if(rule.from>0)SaveUniverse(rule.from,source);if(rule.to>0)SaveUniverse(rule.to,destination);
   }
  }
  void ResetUniverses(){
   for(int i=0;i<Meta.universes.Count;i++){var u=Meta.universes[i];u.researchPlan.automatic=false;Meta.universes[i]=new UniverseSave{role=u.role,researchPlan=u.researchPlan,id=u.id,kind=u.kind,name=u.name,time=S.time,nextAutomation=S.time+60};}
   parallel.Clear();foreach(var u in Meta.universes)Universe(u.id).RestoreResearchPrerequisites();foreach(var t in Meta.transfers){t.enabled=false;t.next=S.time+TransferInterval;t.blocked="Re-enable after major reset";}
  }
 }
}
