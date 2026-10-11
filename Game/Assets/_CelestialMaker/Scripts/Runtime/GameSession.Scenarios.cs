using System;
using UnityEngine;
using CelestialMaker.Core;
namespace CelestialMaker {
 public sealed partial class GameSession {
  State FreshState(){var current=Sim?.S??Saved;var state=SaveStore.ScenarioKey!=""&&current!=null?Simulation.ScenarioState(current.discovery.scenario,current.discovery.seed):new State();state.settings=JsonUtility.FromJson<Settings>(JsonUtility.ToJson(CurrentSettings));return state;}
  public void OpenScenario(int scenario,int seed){
   if(Sim==null||!Sim.CampaignComplete||SaveStore.ScenarioKey!=""||!WebWriter||Application.platform==RuntimePlatform.WebGLPlayer)return;
   // Validate arguments before touching the current save or routing.
   var fresh=Simulation.ScenarioState(scenario,seed);Save();if(SaveFailed)return;
   string previous=SaveStore.ScenarioKey;var priorSim=Sim;var priorSaved=Saved;
   try{SaveStore.ScenarioKey=scenario+"-"+seed;var state=SaveStore.Load();if(state==null&&System.IO.File.Exists(SaveStore.Pathname))throw new InvalidOperationException("Existing scenario could not be recovered; its file was preserved");state=state??fresh;
    if(state.discovery.scenario!=scenario||state.discovery.seed!=seed)throw new ArgumentException("Scenario identity differs from its slot");
    state.settings=JsonUtility.FromJson<Settings>(JsonUtility.ToJson(CurrentSettings));Saved=state;Begin(state);Save();if(SaveFailed)throw new InvalidOperationException(Message);
   }catch(Exception e){SaveStore.ScenarioKey=previous;Sim=priorSim;Saved=priorSaved;Message="Scenario could not open: "+e.Message;}
  }
  public void ReturnFromScenario(){
   if(Sim==null||SaveStore.ScenarioKey==""||!WebWriter||Application.platform==RuntimePlatform.WebGLPlayer)return;Save();if(SaveFailed)return;
   var completed=Sim.Discover.scenarioComplete;string title=DiscoveryContent.Scenarios[Sim.Discover.scenario]+" / seed "+Sim.Discover.seed;string prior=SaveStore.ScenarioKey;var priorSim=Sim;var priorSaved=Saved;
   try{SaveStore.ScenarioKey="";var campaign=SaveStore.Load();if(campaign==null)throw new InvalidOperationException("Campaign save unavailable; the scenario remains saved.");Saved=campaign;Begin(campaign);if(completed)Sim.Chronicle("scenario-"+prior,"Completed optional scenario: "+title,true);Save();if(SaveFailed)throw new InvalidOperationException(Message);}
   catch(Exception e){SaveStore.ScenarioKey=prior;Sim=priorSim;Saved=priorSaved;Message="Return failed: "+e.Message;}
  }
 }
}
