using UnityEngine;
using UnityEngine.EventSystems;

namespace CelestialMaker {
 public static class ObservatoryTouch {
  // EventSystem measures dragging in physical pixels, while the phone UI uses
  // a 480-unit short edge. A desktop-sized threshold cancels ordinary finger
  // taps inside ScrollRects on dense displays. Keep deliberate swipes intact.
  public static int DragThreshold(int width,int height,float dpi) {
   float scale=Mathf.Max(1,Mathf.Min(width,height)/480f);
   float density=float.IsNaN(dpi)||float.IsInfinity(dpi)||dpi<100||dpi>1000?0:dpi*.08f;
   return Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(16*scale,density)),16,80);
  }
  public static void Configure(EventSystem events,int width,int height,float dpi) {
   if(events)events.pixelDragThreshold=DragThreshold(width,height,dpi);
  }
 }
}
