using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;
using CelestialMaker.Core;

partial class Program {
 static bool CampaignReady(Simulation s)=>s.CampaignComplete;
 static void Pacing(){
  string folder=Path.Combine(root,"Logs/Pacing400");Directory.CreateDirectory(folder);
  double hours=double.Parse(arguments.FirstOrDefault(a=>a.StartsWith("--hours="))?.Substring(8)??"600",System.Globalization.CultureInfo.InvariantCulture);
  string tag=arguments.FirstOrDefault(a=>a.StartsWith("--tag="))?.Substring(6)??"current";
  var rows=new List<object>();
  foreach(string style in new[]{"active","background","check-in"}.Where(x=>!arguments.Any(a=>a.StartsWith("--style="))||arguments.Contains("--style="+x)))
  foreach(string branch in new[]{"Energy","Matter","Time"}.Where(x=>!arguments.Any(a=>a.StartsWith("--strategy="))||arguments.Contains("--strategy="+x))){
   var s=new Simulation(data);var watch=Stopwatch.StartNew();double nextVisit=0,interval=style=="active"?900:style=="background"?3600:21600;int visits=0,resumes=0;double[] first={-1,-1,-1};double horizon=-1,trees=-1,completion=-1;var seen=new HashSet<string>();var milestones=new List<object>();var contentSeen=new HashSet<string>();double lastUnlock=0,longestGap=0;int manualActions=0;
   using var diagnostic=arguments.Contains("--diagnostic")?new System.Threading.Timer(_=>{try{Console.WriteLine($"WATCH {branch} {s.S.time/3600:0.000000}h");File.WriteAllText(Path.Combine(folder,tag+"-watch.json"),JsonSerializer.Serialize(s.S,J));}catch{}},null,10000,10000):null;
   while(s.S.time<hours*3600-1e-6){
    if(s.S.time+1e-6>=nextVisit){
     nextVisit+=style!="check-in"&&s.S.time<7200?900:interval;visits++;int revision=s.Revision;ChoosePacing(s,branch);if(arguments.Contains("--discovery"))ChooseDiscovery(s,branch);manualActions+=Math.Max(0,s.Revision-revision);
     bool allG=ExpansionContent.Genesis.All(u=>s.HasG(u.id)),allA=ExpansionContent.Ascension.All(u=>s.HasA(u.id));
     if(horizon<0&&allG)horizon=s.S.time/3600;if(trees<0&&allG&&allA)trees=s.S.time/3600;
     bool prepareGenesis=!allG&&s.Meta.ascensionEarned.Value>=ProgressionBalance.AscensionForGenesis;
     if(!(allG&&allA))for(int layer=2;layer>=0;layer--){if(layer==2&&allG||layer<2&&prepareGenesis||layer==1&&allG&&allA||layer==0&&s.Meta.prestigeEarned.Value>=ProgressionBalance.PrestigeForAscension)continue;if(s.PrestigeReward(layer)>=1&&s.Rebirth(layer)){if(first[layer]<0)first[layer]=s.S.time/3600;milestones.Add(new{layer,hours=s.S.time/3600});break;}}
    }
    if(!arguments.Contains("--without-plan")&&s.HasLayer(0)&&s.Evolution.developmentPlan=="")s.ConfigureDevelopmentPlan(branch);
    double span=Math.Min(arguments.Contains("--offline-resume")?interval:3600,Math.Min(nextVisit-s.S.time,hours*3600-s.S.time));
    if(arguments.Contains("--offline-resume")){s.BeginOffline(span);double nextDiagnostic=watch.Elapsed.TotalSeconds+15;while(s.S.offlineRemaining>0){s.ProcessOffline(32768);if(watch.Elapsed.TotalSeconds>=nextDiagnostic){nextDiagnostic=watch.Elapsed.TotalSeconds+15;Console.WriteLine($"TICK {branch} {s.S.time/3600:0.000}h / remaining {s.S.offlineRemaining:0}s");File.WriteAllText(Path.Combine(folder,tag+"-live.json"),JsonSerializer.Serialize(s.S,J));}}s.SettleUniverses();s=new Simulation(data,Clone(s.S));resumes++;if(resumes%10==0)Console.WriteLine($"PROGRESS {branch} {s.S.time/3600:0}h, visit/reload {resumes}");}
    else Advance(s,span);
    foreach(string id in s.S.purchases)seen.Add(id);
    var discovered=s.S.purchases.Concat(s.Meta.records).Concat(s.Meta.ascensionUpgrades).Concat(s.Meta.genesisUpgrades).Concat(s.S.achievements).ToArray();bool newContent=false;foreach(var id in discovered)if(contentSeen.Add(id))newContent=true;
    if(newContent){longestGap=Math.Max(longestGap,s.S.time-lastUnlock);lastUnlock=s.S.time;}
    if(CampaignReady(s)){completion=s.Meta.campaignCompletedAt/3600;break;}
   }
   s.SettleUniverses();var row=new{version=ProgressionBalance.Version,discovery=arguments.Contains("--discovery"),modules=s.Discover.modules,roles=s.Meta.universes.Select(u=>u.role).ToArray(),style,strategy=branch,firstPrestigeHours=first[0],firstAscensionHours=first[1],firstGenesisHours=first[2],horizonHours=horizon,allTreesHours=trees,completionHours=completion,elapsedHours=s.S.time/3600,megaLevels=s.Meta.megas.Select(m=>m.level).ToArray(),research=seen.Count,visits,genesisResets=s.Meta.historicResets[2],resumes,offline=arguments.Contains("--offline-resume"),longestDiscoveryGapHours=Math.Max(longestGap,s.S.time-lastUnlock)/3600,manualActions,resetRecoverySamples=s.Evolution.runs.Count(r=>r.recoveredAt>=0),cpuSeconds=watch.Elapsed.TotalSeconds};rows.Add(row);Console.WriteLine(JsonSerializer.Serialize(row));
   if(arguments.Contains("--verify-target")&&(completion<360||completion>440))Environment.ExitCode=1;
   File.WriteAllText(Path.Combine(folder,tag+"-"+style+"-"+branch+"-save.json"),JsonSerializer.Serialize(s.S,J));File.WriteAllText(Path.Combine(folder,tag+"-"+style+"-"+branch+"-resets.json"),JsonSerializer.Serialize(milestones,J));File.WriteAllText(Path.Combine(folder,tag+".json"),JsonSerializer.Serialize(rows,J));
  }
 }
 static void ChooseDiscovery(Simulation s,string branch){
  int[] choices=branch=="Energy"?new[]{1,1,1}:branch=="Matter"?new[]{2,2,1}:new[]{2,2,2};
  for(int p=0;p<3;p++)if(s.Discover.modules[p]!=choices[p]&&s.ModuleUnlocked(p,choices[p]))s.SelectModule(p,choices[p]);
  foreach(var u in s.Meta.universes){int role=u.id==1?3:1;if(u.role!=role)s.SetUniverseRole(u.id,role);if(s.HasG("g_horizon")&&!s.Meta.transfers.Any(t=>t.from==u.id&&t.enabled))s.ScheduleTransfer(u.id,0,0);}
 }
 static void ChoosePacing(Simulation s,string branch){
  Choose(s,branch,true);
  if(s.Evolution.focus!=branch&&s.FocusUnlocked(branch)&&s.S.time>=s.Evolution.focusReadyAt)s.SelectFocus(branch);
  if(s.HasA("a_time_2")){foreach(var research in data.research)s.QueueResearch(research.id);s.S.researchPlan.automatic=true;}
  if(ExpansionContent.Genesis.All(u=>s.HasG(u.id)))foreach(var u in ExpansionContent.Ascension)s.BuyMeta(u.id);
  bool finalBuild=ExpansionContent.Genesis.All(u=>s.HasG(u.id))&&ExpansionContent.Ascension.All(u=>s.HasA(u.id));
  if(finalBuild||!ExpansionContent.Genesis.All(u=>s.HasG(u.id))&&s.Meta.ascensionEarned.Value>=ProgressionBalance.AscensionForGenesis)Array.Clear(s.S.automation.repeatPrestige,0,3);
 }
}
