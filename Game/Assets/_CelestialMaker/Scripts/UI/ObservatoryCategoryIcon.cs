using UnityEngine;
using UnityEngine.UI;
namespace CelestialMaker {
 // Distinct silhouettes accompany category names; color is never the only cue.
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ObservatoryCategoryIcon : MaskableGraphic {
  public int category;
  protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var rect=rectTransform.rect;int sides=category==0?16:category==1?4:category==2?3:category==3?6:category==4?5:4;float angle=category==5?45:0;vh.AddVert(rect.center,color,Vector2.zero);for(int i=0;i<sides;i++){float a=(i*360f/sides+angle)*Mathf.Deg2Rad;vh.AddVert(rect.center+new Vector2(Mathf.Sin(a)*rect.width*.45f,Mathf.Cos(a)*rect.height*.45f),color,Vector2.zero);}for(int i=0;i<sides;i++)vh.AddTriangle(0,i+1,(i+1)%sides+1);}
 }
}
