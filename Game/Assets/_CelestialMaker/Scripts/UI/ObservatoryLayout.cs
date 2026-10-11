using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CelestialMaker {
 // A wrapping row whose height follows its actual children, including scaled text.
 public sealed class ObservatoryFlow : LayoutGroup {
  public float cellWidth=190, gap=8;
  readonly List<float> heights=new List<float>();
  int Columns=>Mathf.Max(1,Mathf.Min(rectChildren.Count,Mathf.FloorToInt((rectTransform.rect.width-padding.horizontal+gap)/(cellWidth+gap))));
  float Width=>Mathf.Max(1,(rectTransform.rect.width-padding.horizontal-gap*(Columns-1))/Columns);
  public override void CalculateLayoutInputHorizontal(){base.CalculateLayoutInputHorizontal();SetLayoutInputForAxis(0,float.PositiveInfinity,cellWidth,1,0);}
  public override void CalculateLayoutInputVertical(){
   heights.Clear();int columns=Columns;
   for(int i=0;i<rectChildren.Count;i++){if(i%columns==0)heights.Add(0);int row=i/columns;heights[row]=Mathf.Max(heights[row],LayoutUtility.GetPreferredHeight(rectChildren[i]));}
   float total=padding.vertical+gap*Mathf.Max(0,heights.Count-1);foreach(float h in heights)total+=h;
   SetLayoutInputForAxis(total,float.PositiveInfinity,total,0,1);
  }
  public override void SetLayoutHorizontal(){int cols=Columns;for(int i=0;i<rectChildren.Count;i++)SetChildAlongAxis(rectChildren[i],0,padding.left+(i%cols)*(Width+gap),Width);}
  public override void SetLayoutVertical(){int cols=Columns;float y=padding.top;for(int i=0;i<rectChildren.Count;i++){int row=i/cols;float h=row<heights.Count?heights[row]:LayoutUtility.GetPreferredHeight(rectChildren[i]);SetChildAlongAxis(rectChildren[i],1,y,h);if(i%cols==cols-1)y+=h+gap;}}
 }
 public sealed class ObservatoryButtonSize : MonoBehaviour, ILayoutElement {
  public TMP_Text label;public float baseline=44,horizontalPadding=32;
  public void CalculateLayoutInputHorizontal(){} public void CalculateLayoutInputVertical(){}
  public float minWidth=>0;public float preferredWidth=>180;public float flexibleWidth=>1;
  public float maxWidth=>-1;public float maxHeight=>-1;
  public float minHeight=>baseline;
  public float preferredHeight=>Mathf.Max(baseline,label?label.GetPreferredValues(label.text,Mathf.Max(40,((RectTransform)transform).rect.width-horizontalPadding),0).y+18:baseline);
  public float flexibleHeight=>0;public int layoutPriority=>2;
 }
}
