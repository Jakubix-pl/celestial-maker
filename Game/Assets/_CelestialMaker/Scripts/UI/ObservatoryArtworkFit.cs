using UnityEngine;
using UnityEngine.UI;
namespace CelestialMaker {
 // Native pixels are the ceiling. High resolution sources are only scaled down;
 // large monitors get breathing room, never a stretched low-resolution bitmap.
 [ExecuteAlways,RequireComponent(typeof(RawImage))]
 public sealed class ObservatoryArtworkFit:MonoBehaviour {
  RawImage image;Vector2 previous;float previousScale;
  public void Fit(){
   if(!image)image=GetComponent<RawImage>();if(!image.texture||!(transform.parent is RectTransform parent))return;
   var canvas=GetComponentInParent<Canvas>();float scale=canvas?Mathf.Max(.01f,canvas.scaleFactor):1;var size=parent.rect.size;
   if(size==previous&&scale==previousScale)return;previous=size;previousScale=scale;
   float ratio=Mathf.Min(1/scale,Mathf.Min(size.x/image.texture.width,size.y/image.texture.height));
   var r=(RectTransform)transform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=Vector2.zero;r.sizeDelta=new Vector2(image.texture.width,image.texture.height)*Mathf.Max(0,ratio);
  }
  void LateUpdate()=>Fit();
 }
}
