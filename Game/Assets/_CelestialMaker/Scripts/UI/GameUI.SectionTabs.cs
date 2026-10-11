using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CelestialMaker {
 public sealed partial class GameUI {
  void SectionTabPc(Button button,string route,bool selected){
   var accent=ObservatoryTabIcon.AccentFor(route);
   ((ObservatoryButton)button).TabAccent(accent);EmphasizePc(button,selected);
   var go=new GameObject("View symbol",typeof(RectTransform),typeof(ObservatoryTabIcon));
   go.transform.SetParent(button.transform,false);
   var rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);
   rect.sizeDelta=new Vector2(18,18)*PcScale;rect.anchoredPosition=new Vector2(16+9*PcScale,0);
   var icon=go.GetComponent<ObservatoryTabIcon>();icon.route=route;icon.color=accent;icon.raycastTarget=false;
   var label=button.GetComponentInChildren<TMP_Text>();label.rectTransform.offsetMin=new Vector2(16+28*PcScale,7);
   button.GetComponent<ObservatoryButtonSize>().horizontalPadding=32+28*PcScale;
   if(!selected)return;
   label.fontStyle|=FontStyles.Bold;
   var line=new GameObject("Active view underline",typeof(RectTransform),typeof(Image));line.transform.SetParent(button.transform,false);
   var bar=(RectTransform)line.transform;bar.anchorMin=Vector2.zero;bar.anchorMax=new Vector2(1,0);bar.pivot=new Vector2(.5f,0);
   bar.offsetMin=new Vector2(10,1);bar.offsetMax=new Vector2(-10,1+3*PcScale);
   var image=line.GetComponent<Image>();image.color=accent;image.raycastTarget=false;
  }
 }
}
