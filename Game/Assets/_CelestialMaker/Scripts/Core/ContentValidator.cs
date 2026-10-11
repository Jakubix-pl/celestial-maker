using System;using System.Collections.Generic;using BreakInfinity;
namespace CelestialMaker.Core {
 public static class ContentValidator {
  public static void Validate(Content d){if(d==null||d.resources?.Length!=12||d.generators?.Length!=24)throw new ArgumentException("Content schema mismatch");var ids=new HashSet<string>();
   foreach(var g in d.generators){if(!ids.Add(g.id)||string.IsNullOrWhiteSpace(g.name)||g.stage<0||g.stage>5||g.output<0||g.output>=12||g.input>=g.output||g.input< -1||g.costResource<0||g.costResource>=12||g.growth<=1||double.IsNaN(g.rate)||g.rate<=0||g.input>=0&&g.consume<=0||BigDouble.Parse(g.cost)<=0)throw new ArgumentException("Invalid or cyclic process: "+g.id);}
   foreach(var list in new[]{d.upgrades,d.research,d.talents})foreach(var u in list){if(!ids.Add(u.id)||u.effect<=0||BigDouble.Parse(u.cost)<=0)throw new ArgumentException("Invalid upgrade "+u.id);}
   foreach(var r in d.research)if(r.requires!=""&&Array.Find(d.research,x=>x.id==r.requires)==null)throw new ArgumentException("Research prerequisite missing");
   foreach(var c in d.challenges)if(!ids.Add(c.id)||c.resource<0||c.resource>=12||c.slow<=0||BigDouble.Parse(c.target)<=0)throw new ArgumentException("Invalid challenge");
   foreach(var a in d.achievements)if(!ids.Add(a.id)||a.resource<0||a.resource>=12||BigDouble.Parse(a.target)<=0)throw new ArgumentException("Invalid achievement");
   foreach(var e in d.expeditions)if(!ids.Add(e.id)||e.duration<=0||BigDouble.Parse(e.cost)<=0||BigDouble.Parse(e.reward)<=0)throw new ArgumentException("Invalid expedition");
   foreach(var story in d.stories)if(!ids.Add(story.id)||string.IsNullOrWhiteSpace(story.text))throw new ArgumentException("Invalid story");
  }
 }
}
