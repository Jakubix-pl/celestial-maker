using UnityEngine;
using UnityEngine.UI;

namespace CelestialMaker {
 // Small vector pictograms stay crisp at every text scale, without font glyph dependencies.
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ObservatoryTabIcon : MaskableGraphic {
  public string route;
  public static Color AccentFor(string route){
   switch(route){
    case "Production":case "Roadmap":case "Megastructures":case "Codex":return CosmicStyle.Hex(CosmicStyle.Light?"245C91":"89C8FF");
    case "Research":case "Prestige":case "Reality laws":return CosmicStyle.Hex(CosmicStyle.Light?"72539C":"CBB0F3");
    case "Upgrades":case "Automation":case "Constellations":case "Achievements":return CosmicStyle.Hex(CosmicStyle.Light?"805500":"F0C178");
    case "Challenges":case "Expeditions":return CosmicStyle.Hex(CosmicStyle.Light?"963F64":"F3ABC8");
    default:return CosmicStyle.Hex(CosmicStyle.Light?"176A65":"71DFD4");
   }
  }
  VertexHelper mesh;
  Vector2 Point(float x,float y){var r=rectTransform.rect;return new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);}
  void Line(float x1,float y1,float x2,float y2){
   var a=Point(x1,y1);var b=Point(x2,y2);var n=new Vector2(-(b-a).y,(b-a).x).normalized*Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.042f;
   int v=mesh.currentVertCount;mesh.AddVert(a-n,color,Vector2.zero);mesh.AddVert(a+n,color,Vector2.zero);mesh.AddVert(b+n,color,Vector2.zero);mesh.AddVert(b-n,color,Vector2.zero);mesh.AddTriangle(v,v+1,v+2);mesh.AddTriangle(v,v+2,v+3);
  }
  void Box(float x,float y,float w,float h){Line(x,y,x+w,y);Line(x+w,y,x+w,y+h);Line(x+w,y+h,x,y+h);Line(x,y+h,x,y);}
  void Arc(float x,float y,float rx,float ry,float start=0,float end=360){for(int i=0;i<20;i++){float a=Mathf.Lerp(start,end,i/20f)*Mathf.Deg2Rad,b=Mathf.Lerp(start,end,(i+1)/20f)*Mathf.Deg2Rad;Line(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry,x+Mathf.Cos(b)*rx,y+Mathf.Sin(b)*ry);}}
  protected override void OnPopulateMesh(VertexHelper vh){
   vh.Clear();mesh=vh;
   switch(route){
    case "Production":Box(.12f,.15f,.16f,.3f);Box(.42f,.15f,.16f,.5f);Box(.72f,.15f,.16f,.7f);break;
    case "Research":Line(.35f,.87f,.65f,.87f);Line(.42f,.87f,.42f,.6f);Line(.58f,.87f,.58f,.6f);Line(.42f,.6f,.17f,.2f);Line(.58f,.6f,.83f,.2f);Line(.17f,.2f,.83f,.2f);Line(.27f,.36f,.73f,.36f);break;
    case "Upgrades":for(int i=0;i<2;i++){float y=.25f+i*.35f;Line(.2f,y,.5f,y+.25f);Line(.5f,y+.25f,.8f,y);}break;
    case "Analytics":Line(.15f,.85f,.15f,.15f);Line(.15f,.15f,.87f,.15f);Line(.25f,.32f,.45f,.58f);Line(.45f,.58f,.63f,.43f);Line(.63f,.43f,.85f,.8f);break;
    case "Prestige":Arc(.5f,.5f,.34f,.34f,45,335);Line(.74f,.74f,.47f,.74f);Line(.74f,.74f,.74f,.99f);break;
    case "Trees":Box(.42f,.7f,.16f,.18f);Box(.1f,.13f,.18f,.18f);Box(.72f,.13f,.18f,.18f);Line(.5f,.7f,.5f,.5f);Line(.19f,.31f,.19f,.5f);Line(.81f,.31f,.81f,.5f);Line(.19f,.5f,.81f,.5f);break;
    case "Roadmap":Line(.2f,.12f,.2f,.87f);Line(.2f,.87f,.82f,.87f);Line(.82f,.87f,.62f,.65f);Line(.62f,.65f,.82f,.43f);Line(.82f,.43f,.2f,.43f);break;
    case "Challenges":Line(.2f,.87f,.8f,.87f);Line(.8f,.87f,.75f,.35f);Line(.75f,.35f,.5f,.12f);Line(.5f,.12f,.25f,.35f);Line(.25f,.35f,.2f,.87f);Line(.5f,.7f,.5f,.42f);break;
    case "Automation":Arc(.5f,.5f,.25f,.25f);for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Line(.5f+Mathf.Cos(a)*.29f,.5f+Mathf.Sin(a)*.29f,.5f+Mathf.Cos(a)*.43f,.5f+Mathf.Sin(a)*.43f);}break;
    case "Megastructures":Box(.12f,.12f,.22f,.48f);Box(.39f,.12f,.22f,.75f);Box(.66f,.12f,.22f,.48f);break;
    case "Reality laws":Line(.5f,.12f,.5f,.9f);Line(.32f,.12f,.68f,.12f);Line(.13f,.7f,.87f,.7f);Line(.22f,.7f,.1f,.38f);Line(.22f,.7f,.34f,.38f);Line(.1f,.38f,.34f,.38f);Line(.78f,.7f,.66f,.38f);Line(.78f,.7f,.9f,.38f);Line(.66f,.38f,.9f,.38f);break;
    case "Parallel universes":Arc(.37f,.61f,.25f,.25f);Arc(.64f,.36f,.25f,.25f);break;
    case "Constellations":Line(.2f,.75f,.5f,.3f);Line(.5f,.3f,.8f,.67f);Arc(.2f,.75f,.1f,.1f);Arc(.5f,.3f,.1f,.1f);Arc(.8f,.67f,.1f,.1f);break;
    case "Expeditions":Line(.35f,.28f,.4f,.67f);Line(.4f,.67f,.7f,.9f);Line(.7f,.9f,.85f,.52f);Line(.85f,.52f,.61f,.2f);Line(.61f,.2f,.35f,.28f);Line(.25f,.2f,.14f,.1f);Line(.46f,.12f,.36f,.02f);break;
    case "Achievements":Line(.3f,.85f,.7f,.85f);Line(.3f,.85f,.35f,.5f);Line(.7f,.85f,.65f,.5f);Arc(.5f,.52f,.15f,.15f,180,360);Line(.5f,.37f,.5f,.15f);Line(.28f,.15f,.72f,.15f);Arc(.28f,.68f,.16f,.15f,90,290);Arc(.72f,.68f,.16f,.15f,-110,90);break;
    case "Codex":Box(.1f,.19f,.8f,.63f);Line(.5f,.19f,.5f,.82f);Line(.23f,.63f,.38f,.63f);Line(.62f,.63f,.77f,.63f);Line(.23f,.43f,.38f,.43f);Line(.62f,.43f,.77f,.43f);break;
    case "Offline summary":Arc(.5f,.5f,.37f,.37f);Line(.5f,.5f,.5f,.76f);Line(.5f,.5f,.73f,.4f);break;
    default:Arc(.5f,.5f,.35f,.35f);Line(.1f,.5f,.9f,.5f);Line(.5f,.1f,.5f,.9f);break;
   }
   mesh=null;
  }
 }
}
