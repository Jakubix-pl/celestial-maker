using UnityEngine;
using UnityEngine.UI;
namespace CelestialMaker {
 // Flat observatory surfaces with small, crisp corner chamfers.
 public sealed class CosmicPanel:Image {
  public Color edgeColor=CosmicStyle.Edge;
  public float cut=6,edge=1;
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();var r=GetPixelAdjustedRect();if(r.width<=0||r.height<=0)return;
   float bevel=Mathf.Min(cut,Mathf.Min(r.width,r.height)/4),inset=color.a<.01f?0:edge;
   var outer=Points(r,bevel);var inner=Points(new Rect(r.x+inset,r.y+inset,Mathf.Max(0,r.width-inset*2),Mathf.Max(0,r.height-inset*2)),Mathf.Max(0,bevel-inset));
   var center=r.center;vh.AddVert(center,color,Vector2.zero);
   for(int i=0;i<8;i++)vh.AddVert(inner[i],color,Vector2.zero);
   for(int i=0;i<8;i++)vh.AddTriangle(0,1+i,1+(i+1)%8);
   if(inset>0){var border=edgeColor;border.a*=color.a;for(int i=0;i<8;i++)vh.AddVert(outer[i],border,Vector2.zero);for(int i=0;i<8;i++){int j=(i+1)%8;vh.AddTriangle(1+i,9+i,9+j);vh.AddTriangle(1+i,9+j,1+j);}}
  }
  static Vector2[] Points(Rect r,float c)=>new[]{new Vector2(r.xMin+c,r.yMin),new Vector2(r.xMax-c,r.yMin),new Vector2(r.xMax,r.yMin+c),new Vector2(r.xMax,r.yMax-c),new Vector2(r.xMax-c,r.yMax),new Vector2(r.xMin+c,r.yMax),new Vector2(r.xMin,r.yMax-c),new Vector2(r.xMin,r.yMin+c)};
 }
}
