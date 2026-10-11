using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using CelestialMaker.Core;
using BreakInfinity;
partial class Program {
 static void OnboardingTests(){
  var fresh=new Simulation(data);var before=JsonSerializer.Serialize(fresh.S,J);var tip=fresh.FirstSteps();Check("first steps are read-only and grant no money or automation",tip.step==0&&tip.process==0&&before==JsonSerializer.Serialize(fresh.S,J)&&!fresh.AnyAutobuyer);
  var oldRun=new State{version=1};oldRun.prestige[0]=new BigDouble(7);var restored=new Simulation(data,SaveMigrations.Upgrade(oldRun));Check("legacy earned points do not qualify as a fresh starter run",!restored.StarterSupplyProtection&&restored.S.prestige[0].Value==7);
  fresh.S.resources[0]=new BigDouble(10000);fresh.Buy(0,2);Check("guide recognizes purchases outside its own button",fresh.FirstSteps().step==1);fresh.Buy(1,1);fresh.Buy(2,1);Check("guide directs first discovery before growth",fresh.FirstSteps().upgrade.id=="r0");fresh.Purchase(data.research[0]);var growth=fresh.FirstSteps();Check("growth suggestions fund owned Energy producers",growth.step==4&&(growth.process==0||growth.process==2||growth.upgrade!=null&&growth.upgrade.costResource==0));
  var protectedRun=new Simulation(data);protectedRun.S.owned[1]=100;protectedRun.S.owned[4]=1;protectedRun.S.resources[0]=BigDouble.Zero;protectedRun.S.resources[1]=BigDouble.Zero;protectedRun.RefreshRates();Check("first-run consumers retain at least twenty percent of incoming currency",protectedRun.StarterSupplyProtection&&protectedRun.CachedRate(0)>=protectedRun.CachedGross(0)*.2-1e-12&&protectedRun.CachedRate(1)>=protectedRun.CachedGross(1)*.2-1e-12&&protectedRun.CachedFlow(1)>0);
  for(int i=0;i<3600;i++)FixedStepClock.Advance(protectedRun,1d/60);Check("starter buffer stores actual production without grants",Near(protectedRun.S.resources[0].Value,protectedRun.S.lifetime[0].Value*.2)&&protectedRun.S.prestige.All(x=>x.Value==0));protectedRun.Meta.historicResets[0]=1;protectedRun.S.resources[0]=BigDouble.Zero;protectedRun.RefreshRates();Check("historical Prestige permanently ends starter protection",!protectedRun.StarterSupplyProtection&&protectedRun.CachedRate(0)==0);
  fresh.S.lifetime[0]=BigDouble.Pow10(ProgressionBalance.FirstPrestigeEnergyLog);Check("tutorial presents a ready reset rather than performing it",fresh.FirstSteps().step==5&&fresh.S.resets[0]==0);fresh.Rebirth(0);Check("post-Prestige guide requires paid buyer",fresh.FirstSteps().step==6&&!fresh.AnyAutobuyer);fresh.UnlockAutobuyer(0);Check("paid buyer completes guide without freebies",fresh.FirstSteps().step==7&&fresh.S.prestige[0].Value==0);
  var cap=new Simulation(data);cap.S.owned[0]=100;Check("production milestone is not a maxed process",!cap.ProcessMaxed(0));cap.S.owned[0]=1000000000;Check("true ownership cap is maxed",cap.ProcessMaxed(0)&&cap.MaxBuy(0)==0);cap=new Simulation(data);cap.Discover.scenario=1;cap.S.owned[0]=25;Check("constrained cap is maxed and has no starter buffer",cap.ProcessMaxed(0)&&!cap.StarterSupplyProtection);
 }
 static void OnboardingAudit(){
  string tag=arguments.FirstOrDefault(x=>x.StartsWith("--tag="))?.Substring(6)??"current";var rows=new List<object>();string folder=Path.Combine(root,"Logs/Onboarding");Directory.CreateDirectory(folder);
  foreach(string policy in new[]{"legacy-tip","balanced-5m","balanced-15m","guided-5m","guided-15m"}){
   var s=new Simulation(data);var events=new List<object>();double firstLoom=-1,firstResearch=-1,firstPrestige=-1,lastAction=0,longestWait=0,lastBeforeReset=0,longestBeforeReset=0;int actions=0;double next=0,visit=policy.EndsWith("15m")?900:policy=="legacy-tip"?60:300;var metrics=new List<object>();
   while(s.S.time<7200-1e-7){
    if(s.S.time+1e-7>=next){next+=visit;int before=s.Revision;
     if(policy=="legacy-tip"){if(s.S.owned[1]==0)s.Buy(1,1);else if(s.S.owned[5]==0)s.Buy(5,1);else foreach(var u in data.research)s.Purchase(u);}
     else if(policy.StartsWith("guided")){for(int clicks=0;clicks<8;clicks++){var hint=s.FirstSteps();bool ok=hint.process>=0?s.Buy(hint.process,hint.step==4?Math.Max(1,s.MaxBuy(hint.process)):1):hint.upgrade!=null?s.Purchase(hint.upgrade):hint.step==5?s.Rebirth(0):hint.step==6?s.UnlockAutobuyer(0):false;if(!ok)break;if(hint.step==6)s.S.automation.buy=true;}}
     else{for(int i=0;i<24;i++)s.Buy(i,Math.Max(1,s.MaxBuy(i)/4));foreach(var u in data.upgrades)s.Purchase(u);foreach(var u in data.research)s.Purchase(u);if(s.PrestigeReward(0)>=1)s.Rebirth(0);if(s.HasLayer(0)){s.UnlockAutobuyer(0);s.S.automation.buy=true;s.Meta.guidedAutomation=true;}}
     if(s.Revision>before){if(firstPrestige<0){longestBeforeReset=Math.Max(longestBeforeReset,s.S.time-lastBeforeReset);lastBeforeReset=s.S.time;}longestWait=Math.Max(longestWait,s.S.time-lastAction);lastAction=s.S.time;actions+=s.Revision-before;events.Add(new{minutes=s.S.time/60,owned=s.S.owned.Take(8).ToArray(),research=s.S.purchases.Count});}
    }
    FixedStepClock.Advance(s,1d/60);if(firstLoom<0&&s.S.owned[1]>0)firstLoom=s.S.time/60;if(firstResearch<0&&s.S.purchases.Any(id=>id.StartsWith("r")))firstResearch=s.S.time/60;if(firstPrestige<0&&s.HasLayer(0))firstPrestige=s.S.time/60;
    if(metrics.Count==0||s.S.time>=metrics.Count*300){s.RefreshRates();metrics.Add(new{minutes=s.S.time/60,energy=s.S.resources[0].ToString(),energyNet=new Number(s.CachedRate(0)).ToString(),particleNet=new Number(s.CachedRate(1)).ToString(),matterFlow=new Number(s.CachedFlow(5)).ToString()});}
   }
   longestWait=Math.Max(longestWait,s.S.time-lastAction);if(firstPrestige<0)longestBeforeReset=Math.Max(longestBeforeReset,s.S.time-lastBeforeReset);rows.Add(new{policy,version=ProgressionBalance.Version,firstLoomMinutes=firstLoom,firstResearchMinutes=firstResearch,firstPrestigeMinutes=firstPrestige,longestActionGapMinutes=longestWait/60,longestPrePrestigeActionGapMinutes=longestBeforeReset/60,actions,owned=s.S.owned.Take(8).ToArray(),research=s.S.purchases.Count,metrics,events});File.WriteAllText(Path.Combine(folder,tag+"-"+policy+"-save.json"),JsonSerializer.Serialize(s.S,J));
  }
  File.WriteAllText(Path.Combine(folder,tag+".json"),JsonSerializer.Serialize(rows,J));Console.WriteLine(JsonSerializer.Serialize(rows.Select(r=>r)));
 }
}
