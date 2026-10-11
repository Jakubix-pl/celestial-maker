using UnityEngine;
using UnityEngine.UI;
namespace CelestialMaker {
 // Cover-crops the illustration without stretching planets on wide panels.
 [RequireComponent(typeof(RawImage))]
 public sealed class CosmicArtwork:MonoBehaviour {
  RawImage image;Vector2 previous;
  void LateUpdate(){if(!image)image=GetComponent<RawImage>();var size=((RectTransform)transform).rect.size;if(size==previous||!image.texture||size.x<=0||size.y<=0)return;previous=size;float aspect=size.x/size.y,source=(float)image.texture.width/image.texture.height;if(aspect>source){float h=source/aspect;image.uvRect=new Rect(0,(1-h)*.5f,1,h);}else{float w=aspect/source;image.uvRect=new Rect((1-w)*.5f,0,w,1);}}
 }
}
