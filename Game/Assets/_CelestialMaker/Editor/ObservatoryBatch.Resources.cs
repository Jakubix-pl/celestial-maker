using System;
using System.Linq;
using UnityEngine;
using TMPro;
using BreakInfinity;

namespace CelestialMaker.EditorTools {
 public static partial class ObservatoryBatch {
  static Transform ResourceCell(int slot)=>ui.GetComponentsInChildren<Transform>(true).First(t=>t.name=="Resource meter "+slot);
  static void ResourceChapter(int stage){for(int i=0;i<12;i++)session.Sim.S.lifetime[i]=BigDouble.Zero;if(stage>0)session.Sim.S.lifetime[stage*2-1]=BigDouble.Parse("1e100");session.Sim.RefreshRates();Call("RefreshPc");}
  static void ResourceChecks(){
   Check("Header identity and value follow chapter changes, reset and selected universe",()=>{
    Fixture(false,1,false,1920,1080);
    foreach(int stage in new[]{0,1,2,3,4,5,0}){
     ResourceChapter(stage);int resource=Math.Max(2,stage*2);session.Sim.S.resources[resource]=new BigDouble(700+stage);Call("RefreshPc");
     var cell=ResourceCell(2);var labels=cell.GetComponentsInChildren<TMP_Text>();
     Require(cell.GetComponentInChildren<ObservatoryResourceIcon>().Resource==resource&&labels[0].text==GameSession.Content.resources[resource].ToUpperInvariant()&&labels[1].text==(700+stage).ToString(),"Resource identity/value diverged at stage "+stage);
    }
    Fixture(true,1,false,1920,1080);Set("pcWorld",1);Call("ShowPc");var parallel=session.Sim.Universe(1);for(int i=0;i<12;i++)parallel.S.lifetime[i]=BigDouble.Zero;parallel.S.resources[2]=new BigDouble(456);parallel.RefreshRates();session.Sim.S.prestige[0]=new BigDouble(789);Call("PresentPc");
    Require(ResourceCell(2).GetComponentInChildren<ObservatoryResourceIcon>().Resource==2&&ResourceCell(2).GetComponentsInChildren<TMP_Text>()[1].text=="456","Satellite header kept home chapter");
    Require(ResourceCell(3).GetComponentsInChildren<TMP_Text>()[1].text=="789","Home currency read cached satellite balance");
    Set("page","Overview");Call("ShowPc");Require(ResourceCell(2).GetComponentInChildren<ObservatoryResourceIcon>().Resource==10,"Global page kept satellite chapter");
   });
   Check("Resource header retains contrast, large numbers and contained geometry",()=>{
    foreach(bool light in new[]{false,true})foreach(var config in new[]{new Vector3(1500,900,1),new Vector3(1920,1080,1),new Vector3(960,720,2),new Vector3(1280,720,2),new Vector3(3840,2160,2)}){
     Fixture(false,config.z,light,(int)config.x,(int)config.y);
     foreach(int stage in new[]{0,2,3,4,5}){
      ResourceChapter(stage);session.Sim.S.resources[0]=BigDouble.Parse("1.234e300");session.Sim.S.prestige[0]=BigDouble.Parse("1.234e300");Call("RefreshPc");Settle();Geometry("resource header");
      foreach(var icon in ui.GetComponentsInChildren<ObservatoryResourceIcon>()){
       var cell=(RectTransform)icon.transform.parent.parent;var panel=cell.GetComponent<CosmicPanel>();float bg=Luminance(panel.color);
       foreach(var label in cell.GetComponentsInChildren<TMP_Text>()){
        float fg=Luminance(label.color);Require((Mathf.Max(fg,bg)+.05f)/(Mathf.Min(fg,bg)+.05f)>=4.5f,"Resource text contrast: "+label.text);
        var corners=new Vector3[4];label.rectTransform.GetWorldCorners(corners);Require(cell.InverseTransformPoint(corners[0]).y>=cell.rect.yMin-1&&cell.InverseTransformPoint(corners[2]).y<=cell.rect.yMax+1,"Resource text escaped header: "+label.text);
       }
       float ink=Luminance(icon.color);Require((Mathf.Max(ink,bg)+.05f)/(Mathf.Min(ink,bg)+.05f)>=3,"Resource icon contrast");
      }
     }
    }
   });
  }
  static void RenderResources(){
   foreach(bool light in new[]{false,true}){
    string theme=light?"dawn":"night";
    foreach(int stage in new[]{0,2,3,4,5}){Fixture(false,1,light,1920,1080);ResourceChapter(stage);Shot("resources-"+theme+"-1920-stage-"+stage);}
    foreach(var config in new[]{new Vector3(1500,900,1),new Vector3(960,720,2),new Vector3(1280,720,2),new Vector3(2560,1440,1.25f),new Vector3(3840,2160,2)}){
     Fixture(true,config.z,light,(int)config.x,(int)config.y);session.Sim.S.resources[0]=BigDouble.Parse("1.234e300");session.Sim.S.prestige[0]=BigDouble.Parse("1.234e300");Call("PresentPc");Shot("resources-"+theme+"-"+(int)config.x+"-"+(int)(config.z*100)+"-large-values");
    }
    Fixture(true,1,light,1920,1080);Set("pcWorld",1);Call("ShowPc");Shot("resources-"+theme+"-parallel");
    Fixture(false,1,light,1920,1080);session.Sim.S.owned[1]=20;session.Sim.S.resources[0]=BigDouble.Parse("1e12");Call("RefreshPc");Shot("resources-"+theme+"-negative-net");
   }
  }
 }
}
