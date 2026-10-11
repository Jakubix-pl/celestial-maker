using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialMaker.Core;
namespace CelestialMaker.EditorTools {
 public static partial class ObservatoryBatch {
  static void DiscoveryChecks(){
   Check("Discovery goals and modules survive Unity save serialization",()=>{Fixture(true,1,false,1920,1080);var s=session.Sim;s.QueueGoal(new GoalOrder{project=1,level=6,after="Save"});s.EnableGoals();s.SelectModule(0,2);s.Discover.signals[0]=true;s.ChooseRoute(0,2);s.SetUniverseRole(1,3);s.SaveOperations(0,"Discovery");var saved=SaveStore.Parse(JsonUtility.ToJson(s.S));Require(saved.discovery.goalsEnabled&&saved.discovery.goals.Count==1&&saved.discovery.modules[0]==2&&saved.discovery.routes[0]==2&&saved.expansion.universes[0].role==3&&saved.evolution.operations.Last().goals.Count==1,"Discovery choices lost");});
   Check("Goal editor queues and enables reviewed actions",()=>{Fixture(true,1,false,1280,720);Call("GoalSequencePc");Click("Add goal");Require(session.Sim.Discover.goals.Count==1,"Goal callback failed");Click("Enable sequence");Require(session.Sim.GoalHoldsResets,"Goal reset hold failed");Click("Clear sequence");Click("Cancel");Require(session.Sim.Discover.goals.Count==1,"Cancelled clear lost goal");Call("ClosePcModal");});
   Check("Practice UI uses isolated economy and records completion only",()=>{Fixture(false,1,false,1280,720);var before=session.Sim.S.resources[0];Call("LessonsPc");Click(DiscoveryContent.Lessons[0]);for(int n=0;n<3;n++)Click("Perform practice step");Require(session.Sim.Discover.lessons[0]&&session.Sim.S.resources[0].Value==before.Value,"Lesson modified campaign resources");Call("ClosePcModal");});
   Check("Module selector changes the real engineering configuration",()=>{Fixture(true,1,false,1920,1080);session.Sim.Discover.modules[0]=0;session.Sim.Discover.moduleReady[0]=0;Call("ModulesPc",0);Click("Select "+DiscoveryContent.ModuleNames[0][1]);Require(session.Sim.Discover.modules[0]==1,"Module action failed");Require(!Button("Select "+DiscoveryContent.ModuleNames[0][2]).interactable,"Module cooldown missing");Call("ClosePcModal");});
   Check("Signal route requires confirmation and persists choice",()=>{Fixture(true,1,false,1920,1080);session.Sim.Discover.signals[0]=true;Set("page","Expeditions");Call("ShowPc");Click("Signals and route discoveries");Click(session.Sim.RouteDescription(1));Click("Cancel");Require(session.Sim.Discover.routes[0]==0,"Cancel chose route");Call("ClosePcModal");Click("Signals and route discoveries");Click(session.Sim.RouteDescription(2));Click("Commit route");Require(session.Sim.Discover.routes[0]==2,"Route not persisted");});
   Check("Scenario save routing never aliases campaign or checkpoint folder",()=>{var key=SaveStore.ScenarioKey;try{SaveStore.ScenarioKey="";string campaign=SaveStore.Pathname,points=SaveStore.CheckpointFolder;SaveStore.ScenarioKey="2-17";Require(SaveStore.Pathname!=campaign&&SaveStore.CheckpointFolder!=points,"Scenario aliases campaign");}finally{SaveStore.ScenarioKey=key;}});
   foreach(bool light in new[]{false,true})foreach(float scale in new[]{1f,2f})foreach(int width in new[]{1280,1920}){
    foreach(string method in new[]{"GoalSequencePc","PlanLaboratoryPc","LessonsPc","ChroniclePc"}){string m=method;Check("Discovery dialog "+m+" / "+light+" / "+scale+" / "+width,()=>{Fixture(true,scale,light,width,720);Call(m);Settle();Geometry(m);Require(Get<RectTransform>("pcModal").GetComponentsInChildren<ScrollRect>().Length==1,"Nested modal scroll");Call("ClosePcModal");});}
    Check("Engineering large text "+light+" / "+scale+" / "+width,()=>{Fixture(true,scale,light,width,720);Call("ModulesPc",1);Settle();Geometry("engineering");Call("ClosePcModal");});
    Check("Stock input clips safely "+light+" / "+scale+" / "+width,()=>{Fixture(true,scale,light,width,720);Set("pcGoalKind",1);Call("GoalSequencePc");var input=Get<RectTransform>("pcModal").GetComponentInChildren<TMP_InputField>();Require(input.textViewport.GetComponent<RectMask2D>(),"No input clipping mask");Settle();Geometry("stock goal");Call("ClosePcModal");Set("pcGoalKind",0);});
   }
  }
  static void PopulateDiscoveryLab(){
   var a=new PlanExperiment(GameSession.Content,JsonUtility.FromJson<State>(JsonUtility.ToJson(session.Sim.S)),1d/60);
   var b=new PlanExperiment(GameSession.Content,JsonUtility.FromJson<State>(JsonUtility.ToJson(session.Sim.S)),1d/60);
   b.simulation.ConfigureDevelopmentPlan("Matter");while(!a.Done||!b.Done){a.Step(4096);b.Step(4096);}
   Set("pcLabA",a);Set("pcLabB",b);Set("pcLabName","Matter plan");Set("pcLabChoice",1);Set("pcLabJob",new PlanComparison(a,b));
  }
  static void RenderDiscovery(){
   foreach(bool light in new[]{false,true})foreach(float scale in new[]{1f,2f}){
    Fixture(true,scale,light,scale==1?1920:1280,scale==1?1080:720);string key="discovery-"+(light?"dawn":"night")+"-"+(int)(scale*100);
    Set("page","Automation");Call("ShowPc");Shot(key+"-automation");
    foreach(string method in new[]{"GoalSequencePc","PlanLaboratoryPc","LessonsPc","ChroniclePc"}){if(method=="PlanLaboratoryPc")PopulateDiscoveryLab();Call(method);Shot(key+"-"+method);ModalBottom();Shot(key+"-"+method+"-bottom");Call("ClosePcModal");}
    Call("ModulesPc",0);Shot(key+"-modules");ModalBottom();Shot(key+"-modules-bottom");Call("ClosePcModal");
    session.Sim.Discover.signals[0]=true;Set("page","Expeditions");Call("ShowPc");Click("Signals and route discoveries");Shot(key+"-signals");Call("ClosePcModal");
    Set("page","Parallel universes");Call("ShowPc");Shot(key+"-roles");Set("page","Challenges");Call("ShowPc");Shot(key+"-challenges");
   }
  }
 }
}
