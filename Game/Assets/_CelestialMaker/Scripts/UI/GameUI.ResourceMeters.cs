using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CelestialMaker.Core;

namespace CelestialMaker {
 public sealed partial class GameUI {
  static int HeaderResourcePc(Simulation sim,int slot){
   if(slot<2)return slot;if(slot==3)return -1;
   for(int stage=5;stage>=1;stage--)if(sim.StageOpen(stage))return stage*2;
   return 2;
  }
  void ResourceMeterPc(Transform meters,int slot){
   var cell=PcBox("Resource meter "+slot,meters,Color.clear);Vertical(cell,0,0);
   cell.GetComponent<VerticalLayoutGroup>().padding=new RectOffset(10,8,0,0);
   var width=cell.gameObject.AddComponent<LayoutElement>();width.minWidth=0;width.preferredWidth=0;width.flexibleWidth=1;
   var title=Caption("",cell,16);title.margin=new Vector4(24*PcScale,0,0,0);
   var amount=Label("",cell,26);var rate=Label("",cell,18);
   var mark=new GameObject("Resource symbol",typeof(RectTransform),typeof(ObservatoryResourceIcon));mark.transform.SetParent(title.transform,false);
   var markRect=(RectTransform)mark.transform;markRect.anchorMin=markRect.anchorMax=new Vector2(0,.5f);markRect.sizeDelta=new Vector2(16,16)*PcScale;markRect.anchoredPosition=new Vector2(8*PcScale,0);
   var icon=mark.GetComponent<ObservatoryResourceIcon>();icon.raycastTarget=false;
   var rule=new GameObject("Resource accent",typeof(RectTransform),typeof(Image),typeof(LayoutElement));rule.transform.SetParent(cell,false);rule.GetComponent<LayoutElement>().ignoreLayout=true;
   var ruleRect=(RectTransform)rule.transform;Stretch(ruleRect,Vector2.zero,new Vector2(0,1),new Vector2(0,4),new Vector2(2,-4));var stripe=rule.GetComponent<Image>();stripe.raycastTarget=false;
   var panel=cell.GetComponent<CosmicPanel>();panel.edge=0;panel.raycastTarget=PhoneObservatory;
   if(PhoneObservatory){var touch=cell.gameObject.AddComponent<Button>();touch.targetGraphic=panel;touch.transition=Selectable.Transition.None;touch.onClick.AddListener(PhoneResourcesPc);}
   void Rate(Simulation sim,int resource){
    rate.text=resource<0?"Home currency":"NET "+new Number(sim.CachedRate(resource))+" /s";
    rate.color=resource>=0&&sim.CachedRate(resource)<0?CosmicStyle.Warning:CosmicStyle.Muted;
   }
   void Identity(Simulation sim,int resource){
    if(icon.Resource==resource)return;
    icon.SetResource(resource);title.text=resource<0?"PRESTIGE POINTS":GameSession.Content.resources[resource].ToUpperInvariant();title.color=icon.color;stripe.color=icon.color;panel.color=CosmicStyle.Tint(icon.color,.055f);
    Rate(sim,resource);
   }
   pcShellBindings.Add(()=>{
    var sim=LocalPagePc?EconomyPc:Sim;int resource=HeaderResourcePc(sim,slot);Identity(sim,resource);Rate(sim,resource);
    bool visible=resource<0?ResetCurrencyVisible(0):ResourceVisible(sim,resource);
    if(cell.gameObject.activeSelf!=visible)cell.gameObject.SetActive(visible);
   });
   BindLive(amount,()=>{
    var sim=LocalPagePc?EconomyPc:Sim;int resource=HeaderResourcePc(sim,slot);Identity(sim,resource);
    amount.text=resource<0?Sim.S.prestige[0].ToString():sim.S.resources[resource].ToString();
   },true);
  }
  void PhoneResourcesPc(){
   var sim=LocalPagePc?EconomyPc:Sim;var d=ModalPc("Resources / "+WorldNamePc,780,850);PcButton("Close",d,ClosePcModal);
   for(int i=0;i<12;i++){if(!ResourceVisible(sim,i))continue;int resource=i;var card=PCard(d,GameSession.Content.resources[i]);Label(GameSession.Content.resources[i],card,22,0,ObservatoryResourceIcon.AccentFor(i));var value=Label("",card,22);var rate=Label("",card,18,0,CosmicStyle.Muted);Bind(()=>{value.text=sim.S.resources[resource].ToString();rate.text="NET "+new Number(sim.CachedRate(resource))+" /s";});}
   for(int i=0;i<3;i++){if(!ResetCurrencyVisible(i))continue;int layer=i;var value=Label("",d,22);Bind(()=>value.text=ResetRules.Currencies[layer]+" / "+Sim.S.prestige[layer]);}
   FocusModalPc();
  }
 }
}
