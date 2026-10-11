using UnityEngine;
using UnityEngine.UI;

namespace CelestialMaker {
 // Resource identity is independent of the meter slot and has a non-color cue.
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ObservatoryResourceIcon : MaskableGraphic {
  public int Resource {get;private set;}=int.MinValue;
  public void SetResource(int resource){if(Resource==resource)return;Resource=resource;color=AccentFor(resource);SetVerticesDirty();}
  public static Color AccentFor(int resource){
   switch(resource){
    case -1:return CosmicStyle.Hex(CosmicStyle.Light?"72539C":"CBB0F3");
    case 0:return CosmicStyle.Hex(CosmicStyle.Light?"805500":"F0C178");
    case 1:return CosmicStyle.Hex(CosmicStyle.Light?"245C91":"89C8FF");
    case 2:return CosmicStyle.Hex(CosmicStyle.Light?"176A65":"71DFD4");
    case 4:return CosmicStyle.Hex(CosmicStyle.Light?"326B42":"A0D9AB");
    case 6:return CosmicStyle.Hex(CosmicStyle.Light?"8B4826":"FFB889");
    case 8:return CosmicStyle.Hex(CosmicStyle.Light?"963F64":"F3ABC8");
    default:return CosmicStyle.Hex(CosmicStyle.Light?"246B7C":"85DBF0");
   }
  }
  VertexHelper mesh;
  Vector2 Point(float x,float y){var r=rectTransform.rect;return new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);}
  void Line(float x1,float y1,float x2,float y2){
   var a=Point(x1,y1);var b=Point(x2,y2);var n=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.045f;
   int v=mesh.currentVertCount;mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);mesh.AddTriangle(v,v+1,v+2);mesh.AddTriangle(v,v+2,v+3);
  }
  void Ring(float x,float y,float rx,float ry){for(int i=0;i<24;i++){float a=i*Mathf.PI/12,b=(i+1)*Mathf.PI/12;Line(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry,x+Mathf.Cos(b)*rx,y+Mathf.Sin(b)*ry);}}
  void Polygon(params Vector2[] points){int first=mesh.currentVertCount;foreach(var p in points)mesh.AddVert(Point(p.x,p.y),color,Vector2.zero);for(int i=1;i<points.Length-1;i++)mesh.AddTriangle(first,first+i,first+i+1);}
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();mesh=vh;
   switch(Resource){
    case 0:Polygon(new Vector2(.58f,.96f),new Vector2(.18f,.45f),new Vector2(.46f,.45f),new Vector2(.37f,.04f),new Vector2(.84f,.6f),new Vector2(.55f,.6f));break;
    case 1:Ring(.3f,.73f,.13f,.13f);Ring(.74f,.55f,.13f,.13f);Ring(.35f,.22f,.13f,.13f);Line(.4f,.69f,.61f,.6f);Line(.64f,.44f,.47f,.29f);break;
    case 2:Ring(.5f,.5f,.4f,.18f);Ring(.5f,.5f,.18f,.4f);Ring(.5f,.5f,.055f,.055f);break;
    case 4:Ring(.5f,.5f,.3f,.3f);Line(.1f,.27f,.9f,.73f);Line(.12f,.19f,.88f,.65f);break;
    case 6:Polygon(new Vector2(.5f,.96f),new Vector2(.62f,.62f),new Vector2(.96f,.5f),new Vector2(.62f,.38f),new Vector2(.5f,.04f),new Vector2(.38f,.38f),new Vector2(.04f,.5f),new Vector2(.38f,.62f));break;
    case 8:Ring(.5f,.5f,.4f,.4f);Ring(.5f,.5f,.23f,.23f);Ring(.5f,.5f,.05f,.05f);break;
    case -1:Line(.5f,.94f,.86f,.5f);Line(.86f,.5f,.5f,.06f);Line(.5f,.06f,.14f,.5f);Line(.14f,.5f,.5f,.94f);Line(.5f,.72f,.5f,.28f);Line(.33f,.5f,.67f,.5f);break;
    default:Line(.22f,.88f,.78f,.88f);Line(.22f,.12f,.78f,.12f);Line(.25f,.85f,.25f,.72f);Line(.75f,.85f,.75f,.72f);Line(.25f,.72f,.75f,.28f);Line(.75f,.72f,.25f,.28f);Line(.25f,.28f,.25f,.15f);Line(.75f,.28f,.75f,.15f);break;
   }
   mesh=null;
  }
 }
}
