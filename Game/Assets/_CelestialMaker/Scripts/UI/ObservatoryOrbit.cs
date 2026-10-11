using UnityEngine;
using UnityEngine.UI;

namespace CelestialMaker {
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ObservatoryOrbit : Graphic {
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();var rect=GetPixelAdjustedRect();float radius=Mathf.Min(rect.width,rect.height)*.44f;
   for(int ring=0;ring<3;ring++){
    float tilt=ring*Mathf.PI/3,cos=Mathf.Cos(tilt),sin=Mathf.Sin(tilt);var tint=color;tint.a*=ring==0?.8f:.38f;
    for(int segment=0;segment<192;segment++){
     int first=mesh.currentVertCount;
     for(int corner=0;corner<4;corner++){
      float angle=(segment+(corner>=2?1:0))*2*Mathf.PI/192;
      float r=radius+(corner==0||corner==3?-2:2);float x=Mathf.Cos(angle)*r,y=Mathf.Sin(angle)*r*.44f;
      mesh.AddVert(rect.center+new Vector2(x*cos-y*sin,x*sin+y*cos),tint,Vector2.zero);
     }
     mesh.AddTriangle(first,first+1,first+2);mesh.AddTriangle(first,first+2,first+3);
    }
   }
  }
 }
}
