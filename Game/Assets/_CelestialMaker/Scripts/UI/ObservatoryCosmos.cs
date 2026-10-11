using UnityEngine;
using UnityEngine.UI;
namespace CelestialMaker {
 // Geometry follows actual progression. Rebuilt only when a stage/style changes.
 [RequireComponent(typeof(CanvasRenderer))]
 public sealed class ObservatoryCosmos:MaskableGraphic {
  int chapter=-1,worlds,style;int[] levels=new int[3],progress=new int[3];bool light;
  public void Configure(int stage,int[] megas,int universes,int appearance,bool dawn,int[] work){bool changed=chapter!=stage||worlds!=universes||style!=appearance||light!=dawn;for(int i=0;i<3;i++){changed|=levels[i]!=megas[i];levels[i]=megas[i];changed|=progress[i]!=work[i];progress[i]=work[i];}chapter=stage;worlds=universes;style=appearance;light=dawn;if(changed)SetVerticesDirty();}
  protected override void OnPopulateMesh(VertexHelper mesh){
   mesh.Clear();var rect=rectTransform.rect;var center=rect.center;float scale=Mathf.Min(rect.width/2-16,rect.height/2-14);if(scale<1)return;
   Color accent=light?new Color(.05f,.38f,.5f):new Color(.35f,.87f,.95f);Color orbit=light?new Color(.22f,.4f,.49f,.45f):new Color(.27f,.47f,.65f,.45f);
   for(int ring=1;ring<=Mathf.Clamp(chapter+1,1,6);ring++){float r=scale*ring/7;Circle(mesh,center,r,orbit,1);float angle=ring*2.4f+style*.5f;var p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r;Disc(mesh,p,2+ring*.4f,accent);}
   Disc(mesh,center,chapter>=3?10:4,accent);
   for(int m=0;m<3;m++)for(int part=0;part<levels[m];part++){float a=part*Mathf.PI/3+m*.12f;float r=scale*(.78f+m*.07f);Arc(mesh,center,r,a,a+.68f,new Color(.8f,.63f,.98f),3);}
   for(int m=0;m<3;m++)if(progress[m]>0){float a=levels[m]*Mathf.PI/3+m*.12f;Arc(mesh,center,scale*(.78f+m*.07f),a,a+.68f*progress[m]/100,new Color(.95f,.74f,.3f),3);}
   for(int w=0;w<worlds;w++)Circle(mesh,center+new Vector2((w==0?-1:1)*scale*1.5f,0),scale*.2f,accent,2);
   if(style>0)for(int i=0;i<18;i++){float a=i*2.399963f,r=scale*Mathf.Sqrt((i+1)/18f);Disc(mesh,center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,style==2?2:1,orbit);}
  }
  static void Circle(VertexHelper v,Vector2 p,float r,Color c,float width)=>Arc(v,p,r,0,Mathf.PI*2,c,width);
  static void Arc(VertexHelper v,Vector2 p,float r,float start,float end,Color c,float width){int steps=Mathf.Max(8,Mathf.CeilToInt((end-start)*14));for(int i=0;i<steps;i++){float a=Mathf.Lerp(start,end,(float)i/steps),b=Mathf.Lerp(start,end,(float)(i+1)/steps);Vector2 x=p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,y=p+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r;Vector2 n=(y-x).normalized; n=new Vector2(-n.y,n.x)*width*.5f;int j=v.currentVertCount;v.AddVert(x-n,c,Vector2.zero);v.AddVert(x+n,c,Vector2.zero);v.AddVert(y+n,c,Vector2.zero);v.AddVert(y-n,c,Vector2.zero);v.AddTriangle(j,j+1,j+2);v.AddTriangle(j,j+2,j+3);}}
  static void Disc(VertexHelper v,Vector2 p,float r,Color c){int start=v.currentVertCount;v.AddVert(p,c,Vector2.zero);for(int i=0;i<=16;i++){float a=i*Mathf.PI/8;v.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,c,Vector2.zero);if(i>0)v.AddTriangle(start,start+i,start+i+1);}}
 }
}
