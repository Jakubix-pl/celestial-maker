using System;
using System.Collections.Generic;
using System.Linq;
using BreakInfinity;

namespace CelestialMaker.Core {
 [Serializable] public sealed class ObservatoryPreferences {
  public int researchFilter,upgradeFilter,productionStage=-1,availability,goalWorld;
  public bool researchFavorites,upgradeFavorites,autobuyerGuideSeen,readyNotifications=true;
  public string researchSearch="",upgradeSearch="",goal="";
  public List<string> favorites=new List<string>();
 }
 [Serializable] public sealed class ResearchPlan {
  public List<string> queue=new List<string>();
  public bool automatic;
 }
 public sealed partial class Simulation {
  public BigDouble ResearchAvailable(int resource)=>BigDouble.Max(0,S.resources[resource].Value-EffectiveReserve(resource));
  public bool CanResearch(Upgrade u)=>u!=null&&StageOpen(u.stage)&&!S.purchases.Contains(u.id)&&(string.IsNullOrEmpty(u.requires)||S.purchases.Contains(u.requires))&&S.resources[u.costResource].Value>=BigDouble.Parse(u.cost);
  // Estimates use the current net rate, with no assumed future purchases or resets.
  public double PurchaseEta(Upgrade u){
   if(u==null||!StageOpen(u.stage)||!string.IsNullOrEmpty(u.requires)&&!S.purchases.Contains(u.requires))return double.PositiveInfinity;
   var missing=BigDouble.Max(0,BigDouble.Parse(u.cost)-S.resources[u.costResource].Value);
   if(missing<=0)return 0;var rate=CachedRate(u.costResource);if(rate<=0)return double.PositiveInfinity;
   return (missing/rate).ToDouble();
  }
  public bool QueueResearch(string id){
   if(!Data.research.Any(u=>u.id==id)||S.purchases.Contains(id))return false;
   var additions=new List<string>();var visiting=new HashSet<string>();
   bool Add(string key){if(S.purchases.Contains(key)||S.researchPlan.queue.Contains(key)||additions.Contains(key))return true;if(!visiting.Add(key))return false;var u=Data.research.FirstOrDefault(x=>x.id==key);if(u==null)return false;if(!string.IsNullOrEmpty(u.requires)&&!Add(u.requires))return false;additions.Add(key);return true;}
   if(!Add(id))return false;S.researchPlan.queue.AddRange(additions);return additions.Count>0;
  }
  public void RemoveQueuedResearch(string id){
   // Remove dependants too, so a manually edited plan never silently skips a prerequisite.
   var removed=new HashSet<string>{id};bool changed;do{changed=false;foreach(var key in S.researchPlan.queue){var u=FindUpgrade(key);if(u!=null&&removed.Contains(u.requires)&&removed.Add(key))changed=true;}}while(changed);
   S.researchPlan.queue.RemoveAll(removed.Contains);
  }
  public bool MoveQueuedResearch(string id,int direction){
   var q=S.researchPlan.queue;int old=q.IndexOf(id),next=old+direction;if(old<0||next<0||next>=q.Count||Math.Abs(direction)!=1)return false;
   var candidate=new List<string>(q);candidate.RemoveAt(old);candidate.Insert(next,id);var seen=new HashSet<string>(S.purchases);
   foreach(var key in candidate){var u=FindUpgrade(key);if(!string.IsNullOrEmpty(u.requires)&&!seen.Contains(u.requires))return false;seen.Add(key);}S.researchPlan.queue=candidate;return true;
  }
  public void RestoreResearchPrerequisites(){
   var intent=S.researchPlan.queue.ToArray();S.researchPlan.queue.Clear();S.researchPlan.automatic=false;foreach(var id in intent)QueueResearch(id);
  }
  void ValidateResearchPlan(ResearchPlan plan){
   if(plan?.queue==null||plan.queue.Count>Data.research.Length||plan.queue.Distinct().Count()!=plan.queue.Count||plan.queue.Any(id=>!Data.research.Any(u=>u.id==id)))throw new ArgumentException("Invalid research plan");
  }
  void RunResearchPlan(){
   var plan=S.researchPlan;if(!plan.automatic||!HasA("a_time_2"))return;
   while(plan.queue.Count>0){var u=FindUpgrade(plan.queue[0]);if(S.purchases.Contains(u.id)){plan.queue.RemoveAt(0);continue;}
    if(!CanResearch(u)||ResearchAvailable(u.costResource)<BigDouble.Parse(u.cost)||!Purchase(u))break;
   }
  }
 }
}
