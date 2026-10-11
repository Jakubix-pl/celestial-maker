using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CelestialMaker.Core;

namespace CelestialMaker.EditorTools {
 public static partial class ObservatoryBatch {
  static void PhoneChecks(){
   Set("phoneReview",true);
   try{
    string[] routes={"Overview","Production","Research","Upgrades","Prestige","Trees","Challenges","Automation","Analytics","Megastructures","Reality laws","Parallel universes","Constellations","Expeditions","Achievements","Codex","Settings","Offline summary","Roadmap"};
    int[,] sizes={{480,1040},{1040,480},{360,800},{800,360}};
    foreach(bool late in new[]{false,true})foreach(float scale in new[]{1f,1.25f})for(int size=0;size<4;size++){
     Fixture(late,scale,false,sizes[size,0],sizes[size,1]);
     string context="Phone/"+late+"/"+scale+"/"+sizes[size,0]+"x"+sizes[size,1];
     foreach(string route in routes)Check(context+"/"+route,()=>{Set("page",route);Call("ShowPc");Settle();Geometry(context+"/"+route);});
     Check(context+"/Navigation",()=>{Call("NavigationDrawerPc");Settle();Geometry(context);Require(Button("Settings"),"Settings unreachable");Call("ClosePcModal");});
     foreach(string dialog in new[]{"GoalSequencePc","PlanLaboratoryPc","LessonsPc","ChroniclePc","PhoneResourcesPc"})Check(context+"/"+dialog,()=>{Call(dialog);Settle();Geometry(dialog);Call("ClosePcModal");});
     Check(context+"/Touch targets",()=>{Set("page","Overview");Call("ShowPc");Settle();foreach(var button in ui.GetComponentsInChildren<Button>())Require(((RectTransform)button.transform).rect.height>=51,"Touch target too short: "+button.name);});
     Check(context+"/Menu",()=>{ui.menu=true;Call("BuildPc");Settle();Geometry(context);ui.menu=false;});
    }
   }finally{Set("phoneReview",false);}
  }
  static void RenderPhone(){
   Set("phoneReview",true);
   try{
    foreach(bool landscape in new[]{false,true}){
     Fixture(true,1,false,landscape?1040:480,landscape?480:1040);
     foreach(string page in new[]{"Overview","Production","Research","Automation","Megastructures","Settings"}){Set("page",page);Call("ShowPc");Shot("phone-"+(landscape?"landscape-":"portrait-")+page.ToLowerInvariant());}
     Call("GoalSequencePc");Shot("phone-"+(landscape?"landscape-":"portrait-")+"goals");Call("ClosePcModal");
     ui.menu=true;Call("BuildPc");Shot("phone-"+(landscape?"landscape-":"portrait-")+"menu");
    }
   }finally{Set("phoneReview",false);}
  }
 }
}
