using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using CelestialMaker.Core;
using BreakInfinity;

partial class Program {
 static Simulation CampaignFixture(){
  var s=Mature();s.S.time=12345;
  s.Meta.genesisUpgrades=ExpansionContent.Genesis.Select(u=>u.id).ToList();
  s.Meta.ascensionUpgrades=ExpansionContent.Ascension.Select(u=>u.id).ToList();
  foreach(var m in s.Meta.megas)m.level=6;
  s.CreateUniverse(1);s.CreateUniverse(2);return s;
 }
 static void PacingTests(){
  var p=new Simulation(data);p.S.lifetime[0]=BigDouble.Parse("1e9");var energy=p.PrestigeReward(0);
  p.S.lifetime[1]=BigDouble.Parse("1e9")/ProgressionBalance.ParticleEnergyEquivalent;
  Check("Prestige uses max production without double-counting Energy and Particles",p.PrestigeReward(0)==energy&&Near(p.PrestigeProduction,BigDouble.Parse("1e9")));
  p.S.lifetime[0]=BigDouble.Zero;Check("Particle strategy earns equivalent Prestige",p.PrestigeReward(0)==energy);
  p.S.lifetime[1]=BigDouble.Zero;p.S.resources[0]=p.S.resources[1]=BigDouble.Parse("1e20");Check("imported stock cannot award Prestige",p.PrestigeReward(0)==0);
  p.S.lifetime[5]=BigDouble.Parse("1e6");p.Meta.prestigeEarned=new BigDouble(7);Check("Ascension below eight points rejected",p.PrestigeReward(1)==0);p.Meta.prestigeEarned=new BigDouble(8);Check("Ascension exact eight-point boundary",p.PrestigeReward(1)==1);
  p.S.lifetime[9]=BigDouble.Parse("1e10");foreach(var m in p.Meta.megas)m.level=1;p.Meta.ascensionEarned=new BigDouble(11);Check("Genesis below twelve points rejected",p.PrestigeReward(2)==0);p.Meta.ascensionEarned=new BigDouble(12);Check("Genesis exact twelve-point boundary",p.PrestigeReward(2)==1);

  var build=Mature();build.Meta.autoBuild=true;Check("first construction starts",build.StartMega(0));Advance(build,120);
  Check("planner funds other projects while the first is building",build.Meta.megas.All(m=>m.building));
  Advance(build,ProgressionBalance.ConstructionBaseSeconds+600);Check("planner holds stage one before Endless Horizon",build.Meta.megas.All(m=>m.level==1&&!m.building));
  Check("manual construction can exceed planner's early target",build.StartMega(1));
  var late=CampaignFixture();foreach(var m in late.Meta.megas)m.level=5;late.Meta.autoBuild=true;Advance(late,120);
  Check("planner starts all final stages after Endless Horizon",late.Meta.megas.All(m=>m.building));

  var final=CampaignFixture();final.Meta.megas[2].level=5;Advance(final,1);Check("campaign does not complete at final system unlock",!final.CampaignComplete);
  final.Meta.megas[2].level=6;final.Meta.ascensionUpgrades.RemoveAt(8);Advance(final,1);Check("campaign requires every Ascension node",!final.CampaignComplete);
  final.Meta.ascensionUpgrades.Add(ExpansionContent.Ascension[8].id);final.Meta.genesisUpgrades.RemoveAt(2);Advance(final,1);Check("campaign requires every Genesis foundation",!final.CampaignComplete);
  final.Meta.genesisUpgrades.Add("g_horizon");final.Meta.universes.RemoveAt(1);Advance(final,1);Check("campaign requires three universes",!final.CampaignComplete);
  final.CreateUniverse(2);final.Advance(0);double completedAt=final.Meta.campaignCompletedAt;Check("campaign records actual completion time",final.CampaignComplete&&completedAt==final.S.time);
  var income=final.Flow(0);Advance(final,100);Check("completion is awarded once and has no production bonus",final.Meta.records.Count(id=>id=="campaign-complete")==1&&completedAt==final.Meta.campaignCompletedAt&&Near(income,final.Flow(0)));
  final=new Simulation(data,Clone(final.S));Check("completion timestamp survives save and reload",final.CampaignComplete&&final.Meta.campaignCompletedAt==completedAt);
  Check("optional Genesis keeps completion record",final.Rebirth(2)&&final.CampaignComplete&&final.Meta.campaignCompletedAt==completedAt&&!final.CampaignRequirementsMet);

  var boundary=CampaignFixture();boundary.Meta.megas[2].level=5;boundary.Meta.megas[2].building=true;boundary.Meta.megas[2].work=boundary.MegaWork(2)-15;
  var fixedRun=new Simulation(data,Clone(boundary.S));var offlineRun=new Simulation(data,Clone(boundary.S));
  for(int i=0;i<1200;i++)FixedStepClock.Advance(fixedRun,1d/60);
  offlineRun.BeginOffline(20);while(offlineRun.S.offlineRemaining>0)offlineRun.ProcessOffline(32);
  Check("60 Hz and offline complete construction at the same boundary",fixedRun.CampaignComplete&&offlineRun.CampaignComplete&&Math.Abs(fixedRun.Meta.campaignCompletedAt-offlineRun.Meta.campaignCompletedAt)<.02&&Math.Abs(offlineRun.Meta.campaignCompletedAt-boundary.S.time-10)<.01);
  var slowRun=new Simulation(data,Clone(boundary.S));for(int i=0;i<100;i++)FixedStepClock.Advance(slowRun,.2);while(slowRun.S.liveRemaining>=FixedStepClock.Step)FixedStepClock.Advance(slowRun,0);
  Check("five background ticks per second preserve 60 Hz progression",slowRun.CampaignComplete&&Math.Abs(slowRun.Meta.campaignCompletedAt-fixedRun.Meta.campaignCompletedAt)<.02&&Enumerable.Range(0,12).All(i=>Near(slowRun.S.resources[i].Value,fixedRun.S.resources[i].Value)));
  var old=Clone(boundary.S);old.expansion.megas[2].work=21600*Math.Pow(3,5)*.5;double priorWork=old.expansion.megas[2].work;var upgraded=new Simulation(data,SaveMigrations.Upgrade(old));
  Check("existing schema-two construction work is retained",upgraded.Meta.megas[2].work==priorWork);Advance(upgraded,1);Check("old work above new goal completes safely",upgraded.CampaignComplete&&upgraded.Meta.megas[2].level==6);
  int count=0;
  foreach(string file in Directory.GetFiles(Path.Combine(root,"Tools/Expansion/Fixtures/Compatibility"),"balance-*-save.json")){var state=JsonSerializer.Deserialize<State>(File.ReadAllText(file),J);string before=JsonSerializer.Serialize(state,J);var migrated=new Simulation(data,state);Check("v2 balance save loads without modifying its state / "+Path.GetFileName(file),before==JsonSerializer.Serialize(migrated.S,J));count++;}
  Check("schema-two compatibility fixtures exercised",count>=3);
 }
}
