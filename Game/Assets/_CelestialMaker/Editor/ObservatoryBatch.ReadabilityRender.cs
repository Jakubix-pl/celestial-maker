using UnityEngine;
using CelestialMaker.Core;
using BreakInfinity;
namespace CelestialMaker.EditorTools {
 public static partial class ObservatoryBatch {
  static void RenderReadability(){
   foreach(bool light in new[]{false,true})foreach(float scale in new[]{1f,2f}){
    int width=scale==1?1920:1280,height=scale==1?1080:720;string key="readable-"+(light?"dawn":"night")+"-"+(scale==1?"100":"200");
    Fixture(false,scale,light,width,height);Set("page","Research");Call("ShowPc");Shot(key+"-research");
    Set("pcExpandedResearch",GameSession.Content.research[0].id);Call("ShowPc");Shot(key+"-details",.08f);
    session.Sim.S.resources[0]=new BigDouble(10000);session.CurrentSettings.observatory.goal=GameSession.Content.research[0].id;Call("ShowPc");Shot(key+"-pinned");
    Call("ResearchComparisonPc",GameSession.Content.research[0]);Shot(key+"-comparison");ModalBottom();Shot(key+"-comparison-bottom");Call("ClosePcModal");
    session.Sim.QueueResearch(GameSession.Content.research[1].id);Call("ResearchPlanPc");Shot(key+"-plan");ModalBottom();Shot(key+"-plan-bottom");Call("ClosePcModal");
    Call("SearchPalettePc",false);Set("pcPaletteQuery","Quantum memory");Call("PopulatePalettePc");Shot(key+"-search");Call("ClosePcModal");
    Call("ResearchFilterDialogPc",true);Shot(key+"-filters");Call("ClosePcModal");
    Call("NavigationDrawerPc");Shot(key+"-navigation");Call("ClosePcModal");
   }
   Fixture(false,1,false,960,720);Set("page","Research");Call("ShowPc");Shot("readable-960-research");Call("NavigationDrawerPc");Shot("readable-960-navigation");Call("ClosePcModal");
  }
 }
}
