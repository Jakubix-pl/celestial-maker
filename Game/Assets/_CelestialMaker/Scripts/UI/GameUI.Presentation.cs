using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialMaker {
 public sealed partial class GameUI {
  sealed class LiveBinding {public TMP_Text target;public ScrollRect scroll;public Action update;public int epoch=-1;public bool visible;public Vector2 position,size;public Vector3 world;public Rect viewport;}
  readonly List<LiveBinding> pcLive=new List<LiveBinding>(),pcLiveShell=new List<LiveBinding>();
  readonly Vector3[] pcCorners=new Vector3[4];
  int pcVisibilityEpoch,pcVisibilityCalculations;
  void BindVisible(TMP_Text target,Action action){var b=new LiveBinding{target=target,scroll=target.GetComponentInParent<ScrollRect>(),update=action};pcBindings.Add(()=>{if(LiveVisible(b))action();});action();}
  void BindLive(TMP_Text target,Action action,bool shell=false){
   var b=new LiveBinding{target=target,scroll=target.GetComponentInParent<ScrollRect>(),update=action};
   (shell?pcLiveShell:pcLive).Add(b);action();
  }
  bool LiveVisible(LiveBinding b){
   if(!b.target||!b.target.gameObject.activeInHierarchy)return false;
   if(!b.scroll||!b.scroll.viewport)return true;
   var position=b.scroll.content?b.scroll.content.anchoredPosition:Vector2.zero;var area=b.scroll.viewport.rect;var world=b.target.rectTransform.position;var size=b.target.rectTransform.rect.size;
   if(b.epoch==pcVisibilityEpoch&&b.position==position&&b.viewport==area&&b.world==world&&b.size==size)return b.visible;
   b.epoch=pcVisibilityEpoch;b.position=position;b.viewport=area;b.world=world;b.size=size;pcVisibilityCalculations++;
   b.target.rectTransform.GetWorldCorners(pcCorners);var viewport=b.scroll.viewport;var rect=viewport.rect;
   var bottom=viewport.InverseTransformPoint(pcCorners[0]);var top=viewport.InverseTransformPoint(pcCorners[2]);
   return b.visible=top.y>=rect.yMin&&bottom.y<=rect.yMax&&top.x>=rect.xMin&&bottom.x<=rect.xMax;
  }
  // Presentation reads authoritative state. No predicted balance is spent or saved.
  void PresentPc(){
   if(menu||S==null)return;
   for(int i=0;i<pcLiveShell.Count;i++)if(LiveVisible(pcLiveShell[i]))pcLiveShell[i].update();
   if(pcModal){for(int i=0;i<pcLive.Count;i++)if(pcLive[i].target&&pcLive[i].target.transform.IsChildOf(pcModal)&&LiveVisible(pcLive[i]))pcLive[i].update();return;}
   for(int i=0;i<pcLive.Count;i++)if(LiveVisible(pcLive[i]))pcLive[i].update();
  }
  static void RefreshBindings(List<Action> bindings){for(int i=0;i<bindings.Count;i++)bindings[i]();}
  static void PresentationCanvas(RectTransform rect,bool controls){
   rect.gameObject.AddComponent<Canvas>();if(controls)rect.gameObject.AddComponent<GraphicRaycaster>();
  }
  void RevealPc(RectTransform target){if(Application.isPlaying)ObservatoryReveal.Begin(target,GameSession.Instance?.CurrentSettings.reducedMotion??true);}
  RawImage ObservatoryArt(Transform parent,string resource){
   var texture=Resources.Load<Texture2D>("UI/PC/"+resource);if(!texture)return null;
   var go=new GameObject(resource,typeof(RectTransform),typeof(RawImage));go.transform.SetParent(parent,false);var image=go.GetComponent<RawImage>();image.texture=texture;image.raycastTarget=false;go.AddComponent<ObservatoryArtworkFit>();return image;
  }
  void CosmicBannerPc(){
   var host=PcBox("Observatory panorama",list,CosmicStyle.Background,Mathf.Min(208,160*PcScale));host.GetComponent<CosmicPanel>().edge=0;
   ObservatoryArt(host,"ObservatoryNebula");
  }
 }
}
