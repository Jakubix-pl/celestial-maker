using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Reflection;
using UnityEngine;
using TMPro;
using CelestialMaker.Core;
using BreakInfinity;
namespace CelestialMaker {
 public sealed partial class ObservatoryHeadlessCheck {
  IEnumerator DiscoveryRuntime(){
   var session=GameSession.Instance;var sim=session.Sim;sim.StopAllAutomation();sim.S.settings.textScale=1;Call("BuildPc");yield return null;
   Call("LessonsPc");yield return null;Click(DiscoveryContent.Lessons[2]);long old=sim.S.resets[0];for(int i=0;i<3;i++){Click("Perform practice step");yield return null;}Check(sim.Discover.lessons[2]&&sim.S.resets[0]==old,"Practice reset completes without resetting the live save");Call("ClosePcModal");
   sim.S.resets[1]=sim.S.resets[2]=1;sim.Meta.megas[0].level=2;Call("ModulesPc",0);yield return null;Click("Select "+DiscoveryContent.ModuleNames[0][2]);Check(sim.Discover.modules[0]==2,"Release module selector applies relay tradeoff");Call("ClosePcModal");
   Call("GoalSequencePc");yield return null;Click("Add goal");Click("Enable sequence");Check(sim.CurrentGoal!=null&&sim.GoalHoldsResets,"Release goal editor enables persistent construction order");Call("ClosePcModal");sim.ClearGoals();
   sim.Discover.signals[0]=true;Page("Expeditions");yield return null;Click("Signals and route discoveries");Click(sim.RouteDescription(1));Click("Commit route");Check(sim.Discover.routes[0]==1,"Release expedition route confirmation reaches Core");
   sim.CreateUniverse(1);Page("Parallel universes");yield return null;Click("Specialization / "+DiscoveryContent.Roles[0]);Click("Assign "+DiscoveryContent.Roles[3]);Click("Assign role");Check(sim.Meta.universes[0].role==3,"Release universe specialization reaches Core");
   sim.StopAllAutomation();Call("PlanLaboratoryPc");yield return null;double liveBefore=sim.S.time;old=sim.S.resets[0];Click("Compare next 6 hours");
   double deadline=Time.realtimeSinceStartupAsDouble+180;PlanExperiment a=null,b=null;
   while(Time.realtimeSinceStartupAsDouble<deadline){a=(PlanExperiment)typeof(GameUI).GetField("pcLabA",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(UI);b=(PlanExperiment)typeof(GameUI).GetField("pcLabB",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(UI);if(a!=null&&a.Done&&b.Done&&!(bool)typeof(GameUI).GetField("pcLabRunning",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(UI))break;if(a!=null&&!(bool)typeof(GameUI).GetField("pcLabRunning",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(UI))Click("Continue calculation");yield return null;}
   Check(a!=null&&a.Done&&b.Done&&a.simulation.S.offlineReport.fixedTicks==1296000&&b.simulation.S.offlineReport.fixedTicks==1296000,"Release laboratory finishes both literal six-hour copies: "+a?.simulation.S.offlineReport.fixedTicks+" / "+b?.simulation.S.offlineReport.fixedTicks);Check(sim.S.time-liveBefore<200&&sim.S.resets[0]==old,"Laboratory grants no simulated time or reset to live campaign");File.WriteAllText(Path.Combine(output,"laboratory.txt"),a.Summary()+"\n\n"+b.Summary());Call("ClosePcModal");
   sim.S.lifetime[0]=BigDouble.Parse("1e12");sim.S.lifetime[1]=BigDouble.Parse("1e12");Page("Challenges");Call("ChallengeTrialPc",0);yield return null;
   deadline=Time.realtimeSinceStartupAsDouble+35;while(Time.realtimeSinceStartupAsDouble<deadline&&!UI.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("READY /")||t.text.StartsWith("NOT REACHED")||t.text.StartsWith("PARTIAL /")))yield return null;
   Check(sim.S.activeChallenge==""&&UI.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.StartsWith("READY /")||t.text.StartsWith("NOT REACHED")||t.text.StartsWith("PARTIAL /")),"Readiness trial reports an outcome without entering live challenge");Call("ClosePcModal");
   sim.Meta.records.Add("campaign-complete");sim.Meta.campaignCompletedAt=sim.S.time;session.Save();string campaign=SaveStore.Pathname;byte[] original=File.ReadAllBytes(campaign);string profile=sim.S.profileId;
   session.OpenScenario(2,17);yield return null;yield return null;yield return null;Check(SaveStore.ScenarioKey=="2-17"&&session.Sim.Discover.scenario==2&&SaveStore.Pathname!=campaign,"Optional scenario switches to a separate release save");
   // OpenScenario first saves the campaign again; that checkpoint is the isolation baseline.
   original=File.ReadAllBytes(campaign);session.Sim.S.resources[0]=new BigDouble(4321);session.Save();string scenarioFile=SaveStore.Pathname;Check(original.SequenceEqual(File.ReadAllBytes(campaign)),"Saving a scenario preserves campaign bytes");
   session.ReturnFromScenario();yield return null;yield return null;yield return null;Check(SaveStore.ScenarioKey==""&&session.Sim.S.profileId==profile&&session.Sim.Discover.scenario==0,"Returning restores the original campaign identity");
   session.OpenScenario(2,17);yield return null;yield return null;yield return null;Check(session.Sim.S.resources[0].Value>=4321&&SaveStore.Pathname==scenarioFile,"Scenario slot resumes its own saved progress");session.ReturnFromScenario();yield return null;yield return null;yield return null;
   var priorSim=session.Sim;string corrupt=SaveStore.BasePath+".scenario-3-991.json";File.WriteAllText(corrupt,"deliberately invalid fixture");session.OpenScenario(3,991);Check(ReferenceEquals(priorSim,session.Sim)&&SaveStore.ScenarioKey==""&&File.ReadAllText(corrupt)=="deliberately invalid fixture","Corrupt scenario preserves its file and live campaign");
   bool rejected=false;try{SaveStore.CheckRoute(Simulation.ScenarioState(2,17));}catch(InvalidOperationException){rejected=true;}Check(rejected,"Scenario imports cannot overwrite a campaign slot");
   session.Save();var saved=SaveStore.Load();Check(saved.discovery.modules[0]==2&&saved.discovery.routes[0]==1&&saved.discovery.lessons[2]&&saved.expansion.universes[0].role==3,"Discovery choices survive actual player save and scenario return");
  }
 }
}
