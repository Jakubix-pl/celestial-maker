using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BreakInfinity;
namespace CelestialMaker.EditorTools {
 public static partial class ObservatoryBatch {
  static void PresentationChecks(){
   Check("Reset currency and chapter progress refresh without waiting for a slow tick",()=>{
    Fixture(false,1,false,1920,1080);Set("page","Prestige");Call("ShowPc");Settle();session.Sim.S.prestige[0]=new BigDouble(123);Call("PresentPc");Require(ui.GetComponentsInChildren<TMP_Text>().Any(t=>t.text.Contains("Balance 123")),"Reset currency stale");Set("page","Overview");Call("ShowPc");Settle();var fill=(RectTransform)ui.GetComponentsInChildren<Transform>().First(t=>t.name=="Chapter progress");session.Sim.S.lifetime[1]=new BigDouble(1);Call("PresentPc");float first=fill.anchorMax.x;session.Sim.S.lifetime[1]=new BigDouble(2);Call("PresentPc");Require(fill.anchorMax.x>first&&fill.anchorMax.x<1,"Progress bar waited for slow refresh");
   });
   Check("60 consecutive presentation frames follow authoritative stock without slow refresh",()=>{
    Fixture(false,1,false,1920,1080);session.Sim.S.offlineReport=new Core.OfflineReport{id="presentation-check",active=false,dismissed=true};var meter=ui.GetComponentsInChildren<Transform>().First(t=>t.name=="Resource meter 0").GetComponentsInChildren<TMP_Text>()[1];
    for(int frame=0;frame<60;frame++){session.Sim.S.resources[0]=new BigDouble(1000+frame);Call("PresentPc");Require(meter.text==(1000+frame).ToString(),"Counter skipped frame "+frame);}
    string before=JsonUtility.ToJson(session.Sim.S);for(int frame=0;frame<60;frame++)Call("PresentPc");string after=JsonUtility.ToJson(session.Sim.S);if(before!=after){File.WriteAllText(Path.Combine(ProjectBootstrap.Root,"Logs/ObservatoryPerformance/presentation-before.json"),before);File.WriteAllText(Path.Combine(ProjectBootstrap.Root,"Logs/ObservatoryPerformance/presentation-after.json"),after);}Require(before==after,"Presentation changed simulation");
    session.Sim.S.resources[0]=BigDouble.Parse("1e300");Call("PresentPc");Require(meter.text.Contains("e300"),"Large-number precision lost");session.Sim.S.resources[0]=new BigDouble(2);Call("PresentPc");Require(meter.text=="2","Reset/spending left stale numbers");
   });
   Check("Hidden live rows defer work and refresh immediately when scrolled into view",()=>{
    Fixture(true,1,false,1920,1080);Set("page","Analytics");Call("ShowPc");Settle();var label=ui.GetComponentsInChildren<TMP_Text>().Where(t=>t.text.StartsWith("Stock ")).Last();string old=label.text;session.Sim.S.resources[11]=new BigDouble(12345);Call("PresentPc");Require(label.text==old,"Offscreen row updated");Get<ScrollRect>("pcScroll").verticalNormalizedPosition=0;Settle();Call("PresentPc");Require(label.text.Contains("1.23e4"),"Newly visible row stale");
   });
   Check("Parallel resource header stays on selected economy at 60 Hz",()=>{
    Fixture(true,1,false,1920,1080);Set("pcWorld",1);Call("ShowPc");var parallel=session.Sim.Universe(1);parallel.S.resources[0]=new BigDouble(321);Call("PresentPc");var meter=ui.GetComponentsInChildren<Transform>().First(t=>t.name=="Resource meter 0").GetComponentsInChildren<TMP_Text>()[1];Require(meter.text=="321","Header read home economy");
   });
   Check("HD typography is generated at 96pt with 2048 atlases and native canvas",()=>{
    Fixture(false,2,false,1280,720);Require(ui.GetComponent<CanvasScaler>().scaleFactor<=1,"UI canvas upscaled");foreach(string name in new[]{"BodyFontHD","DisplayFontHD"}){var font=Resources.Load<TMP_FontAsset>("UI/PC/"+name);Require(font&&font.faceInfo.pointSize>=96&&font.atlasWidth>=2048,"Missing high-resolution font "+name);}
   });
   Check("Raster art preserves aspect and never exceeds source pixel size",()=>{
    Fixture(false,1,false,3840,2160);ui.menu=true;Call("BuildPc");Settle();foreach(var fit in ui.GetComponentsInChildren<ObservatoryArtworkFit>()){fit.Fit();var image=fit.GetComponent<RawImage>();var rect=((RectTransform)fit.transform).rect;Require(rect.width<=image.texture.width+.01&&rect.height<=image.texture.height+.01,"Upsampled art");Require(Math.Abs(rect.width/rect.height-(float)image.texture.width/image.texture.height)<.001,"Stretched art");}
   });
   Check("Reveal animation completes and reduced motion is immediate",()=>{
    var go=new GameObject("Animation test",typeof(RectTransform));try{var rect=(RectTransform)go.transform;ObservatoryReveal.Begin(rect,false);var effect=go.GetComponent<ObservatoryReveal>();var group=go.GetComponent<CanvasGroup>();Require(group.alpha==0,"No starting transition");effect.Step(.07f,false);Require(group.alpha>0&&group.alpha<1,"No intermediate animation");effect.Step(.07f,false);Require(group.alpha==1&&!effect.enabled,"Animation never stopped");ObservatoryReveal.Begin(rect,true);Require(group.alpha==1&&!effect.enabled,"Reduced motion animated");}finally{UnityEngine.Object.DestroyImmediate(go);}
   });
   Check("Default and Windows build icons are assigned",()=>{Require(UnityEditor.PlayerSettings.GetIconsForTargetGroup(UnityEditor.BuildTargetGroup.Unknown).Any(i=>i&&i.name=="CelestialIcon"),"Default icon missing");Require(UnityEditor.PlayerSettings.GetIconsForTargetGroup(UnityEditor.BuildTargetGroup.Standalone).Any(i=>i&&i.name=="CelestialIcon"),"Windows icon missing");});
   Check("Late-game autosave uses isolated file and preserves a valid reload",()=>{
    Fixture(true,1,false,1920,1080);var samples=new double[20];for(int i=0;i<20;i++){var clock=System.Diagnostics.Stopwatch.StartNew();session.Save();samples[i]=clock.Elapsed.TotalMilliseconds;Require(!session.SaveFailed,"Isolated save failed");}Array.Sort(samples);var restored=SaveStore.Load();Require(restored!=null&&restored.expansion.universes.Count==2,"Late save reload failed");File.WriteAllText(Path.Combine(ProjectBootstrap.Root,"Logs/ObservatoryPerformance/autosave.json"),"{\"samples\":20,\"meanMs\":"+samples.Average().ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"p95Ms\":"+samples[19].ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"path\":\"isolated fixture only\"}");
   });
  }
 }
}
