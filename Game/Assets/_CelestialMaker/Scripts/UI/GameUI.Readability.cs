using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using BreakInfinity;
using CelestialMaker.Core;

namespace CelestialMaker {
 public sealed partial class GameUI {
  bool pcCompactNav,pcSearchFavorites;string pcExpandedResearch="",pcPaletteQuery="",pcGoalNotice="";
  TMP_InputField pcListSearch,pcPaletteInput;RectTransform pcPaletteResults;
  float pcSearchDue=-1,pcPaletteDue=-1;
  // The track lives inside the button and never participates in layout or hit testing.
  // Yellow measures funding; green also requires the real purchase prerequisites.
  void PurchaseBarPc(Button button,Func<double> funding,Func<bool> eligible,Func<bool> completed=null,Func<bool> show=null){
   RectTransform Make(string name,Transform parent,Color color){var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(LayoutElement));go.transform.SetParent(parent,false);go.GetComponent<LayoutElement>().ignoreLayout=true;var image=go.GetComponent<Image>();image.color=color;image.raycastTarget=false;return (RectTransform)go.transform;}
   var track=Make("Purchase progress",button.transform,new Color(.32f,.35f,.4f,.35f));track.anchorMin=new Vector2(0,0);track.anchorMax=new Vector2(1,0);track.offsetMin=new Vector2(10,2);track.offsetMax=new Vector2(-10,5);
   var fill=Make("Purchase funding",track,new Color(1f,.76f,.18f));Stretch(fill,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var ink=fill.GetComponent<Image>();
   Action update=()=>{bool done=completed!=null&&completed(),visible=!done&&(show==null||show());if(track.gameObject.activeSelf!=visible)track.gameObject.SetActive(visible);bool ready=!done&&S.offlineRemaining<=0&&eligible();button.interactable=ready;if(!visible)return;float fraction=(float)Math.Max(0,Math.Min(1,funding()));var anchor=new Vector2(fraction,1);if(fill.anchorMax!=anchor)fill.anchorMax=anchor;var color=ready?new Color(.18f,.87f,.44f):new Color(1f,.76f,.18f);if(ink.color!=color)ink.color=color;};
   var target=button.GetComponentInChildren<TMP_Text>();BindVisible(target,update);BindLive(target,update);
  }
  void ResearchBarPc(Button button,Upgrade u){var price=BigDouble.Parse(u.cost);PurchaseBarPc(button,()=>Simulation.PurchaseFunding(EconomyPc.S.resources[u.costResource].Value,price),()=>EconomyPc.CanResearch(u),()=>EconomyPc.S.purchases.Contains(u.id));}
  void ProcessBarPc(Button button,Func<Simulation> economy,int id){var g=GameSession.Content.generators[id];PurchaseBarPc(button,()=>{var e=economy();return Simulation.PurchaseFunding(e.ResearchAvailable(g.costResource),e.Cost(id));},()=>{var e=economy();return e.Open(g)&&e.MaxBuy(id)>0;});}
  void PurchaseGuidePc(){
   Label("Yellow: saving resources · Green: ready to buy. Expand a row for details.",list,16,0,CosmicStyle.Muted);
   if(page!="Research"||PrefsPc.researchFilter==2)return;
   var next=EconomyPc.Data.research.FirstOrDefault(u=>EconomyPc.StageOpen(u.stage)&&!EconomyPc.S.purchases.Contains(u.id)&&(string.IsNullOrEmpty(u.requires)||EconomyPc.S.purchases.Contains(u.requires)));
   if(next==null)return;var card=PCard(list,"Suggested research");VerticalPaddingPc(card,12,4);Caption("ONE STEP AT A TIME",card);var row=HRow(card,"Suggested research actions",0,8);
   var status=Label("",row,18);BindVisible(status,()=>status.text="Next discovery: "+next.name+" · "+UpgradeStatusPc(EconomyPc,next));
   PcButton("Show next discovery",row,()=>OpenUpgradePc(next.id));
  }
  ObservatoryPreferences PrefsPc=>GameSession.Instance.CurrentSettings.observatory??(GameSession.Instance.CurrentSettings.observatory=new ObservatoryPreferences());
  void LoadReadabilityPreferencesPc(){var p=PrefsPc;pcProductionStage=p.productionStage;pcAvailability=p.availability;}
  void VerticalPaddingPc(RectTransform rect,int padding,int gap){var l=rect.GetComponent<VerticalLayoutGroup>();l.padding=new RectOffset(padding,padding,padding,padding);l.spacing=gap;}
  void CategoryMarkPc(Button button,int category){
   var go=new GameObject("Category symbol",typeof(RectTransform),typeof(ObservatoryCategoryIcon));go.transform.SetParent(button.transform,false);var rect=(RectTransform)go.transform;rect.anchorMin=rect.anchorMax=new Vector2(0,.5f);rect.sizeDelta=new Vector2(14,14);rect.anchoredPosition=new Vector2(18,0);var icon=go.GetComponent<ObservatoryCategoryIcon>();icon.category=category;icon.color=CosmicStyle.Section(category);icon.raycastTarget=false;
   var label=button.GetComponentInChildren<TMP_Text>().rectTransform;label.offsetMin=new Vector2(34,7);button.GetComponent<ObservatoryButtonSize>().horizontalPadding=50;
  }
  void NavigationDrawerPc(){
   var dialog=ModalPc("Observatory navigation",820,850);for(int i=0;i<ObservatorySections.Length;i++){int group=i;if(!Sim.SectionUnlocked(i))continue;var b=PcButton(ObservatorySections[i],dialog,()=>NavigatePc(SectionEntryPc(group)));((ObservatoryButton)b).Accent(CosmicStyle.Section(i));CategoryMarkPc(b,i);EmphasizePc(b,SectionPc(page)==i);}
   var actions=HRow(dialog,"Navigation tools",0);PcButton("Find",actions,()=>SearchPalettePc(false));PcButton("Favorites",actions,()=>SearchPalettePc(true));PcButton("Research plan",actions,ResearchPlanPc);PcButton("Economy: "+WorldNamePc,actions,ChooseWorldPc);PcButton("Settings",actions,()=>NavigatePc("Settings"));PcButton("Save now",actions,()=>{GameSession.Instance.Save();ToastPc(GameSession.Instance.SaveFailed?"Save failed. Check Settings.":"Your cosmos has been saved.");});PcButton("Close",dialog,ClosePcModal);FocusModalPc();
  }
  void SectionViewsPc(){
   int group=SectionPc(page);var dialog=ModalPc(group<0?"Views":ObservatorySections[group]+" views",900,760);if(group>=0)for(int i=0;i<ObservatoryRoutes[group].Length;i++){string route=ObservatoryRoutes[group][i];if(!RouteUnlockedPc(route))continue;var b=PcButton(ObservatoryLabels[group][i],dialog,()=>NavigatePc(route));SectionTabPc(b,route,route==page);}
   PcButton("Economy: "+WorldNamePc,dialog,ChooseWorldPc);PcButton("All categories",dialog,NavigationDrawerPc);PcButton("Close",dialog,ClosePcModal);FocusModalPc();
  }
  TMP_InputField SearchFieldPc(Transform parent,string value,string hint,Action<string> changed){
   var field=PcBox("Search field",parent,CosmicStyle.Background,48*PcScale);field.gameObject.AddComponent<RectMask2D>();var input=field.gameObject.AddComponent<TMP_InputField>();var text=(TextMeshProUGUI)PcText("",field,18);text.richText=false;text.textWrappingMode=TextWrappingModes.NoWrap;Stretch(text.rectTransform,Vector2.zero,Vector2.one,new Vector2(14,8),new Vector2(-14,-8));var placeholder=PcText(hint,field,18);placeholder.color=CosmicStyle.Muted;placeholder.textWrappingMode=TextWrappingModes.NoWrap;placeholder.overflowMode=TextOverflowModes.Ellipsis;Stretch(placeholder.rectTransform,Vector2.zero,Vector2.one,new Vector2(14,8),new Vector2(-14,-8));input.textComponent=text;input.textViewport=field;input.placeholder=placeholder;input.lineType=TMP_InputField.LineType.SingleLine;input.characterLimit=80;input.customCaretColor=true;input.caretColor=CosmicStyle.Cyan;input.SetTextWithoutNotify(value);input.onValueChanged.AddListener(s=>changed(s));return input;
  }
  void ReadabilitySearchTickPc(){
   if(pcSearchDue>=0&&Time.unscaledTime>=pcSearchDue){pcSearchDue=-1;bool focused=pcListSearch&&pcListSearch.isFocused;int caret=pcListSearch?pcListSearch.caretPosition:0;ShowPc();if(focused&&pcListSearch){pcListSearch.ActivateInputField();pcListSearch.caretPosition=caret;}if(pcScroll)pcScroll.verticalNormalizedPosition=1;}
   if(pcPaletteDue>=0&&Time.unscaledTime>=pcPaletteDue){pcPaletteDue=-1;if(pcPaletteResults&&pcModal)PopulatePalettePc();}
  }
  bool FavoritePc(string id)=>PrefsPc.favorites.Contains(id);
  void ToggleFavoritePc(string id){if(!PrefsPc.favorites.Remove(id)&&PrefsPc.favorites.Count<256)PrefsPc.favorites.Add(id);}
  string UpgradeStatusPc(Simulation sim,Upgrade u){
   if(sim.S.purchases.Contains(u.id))return "Purchased for this run";
   if(!sim.StageOpen(u.stage))return "Unlock "+CosmicStyle.Chapters[u.stage]+" first";
   if(!string.IsNullOrEmpty(u.requires)&&!sim.S.purchases.Contains(u.requires))return "Research "+sim.FindUpgrade(u.requires).name+" first";
   var missing=BigDouble.Max(0,BigDouble.Parse(u.cost)-sim.S.resources[u.costResource].Value);
   if(missing<=0)return "Ready to acquire";
   return "Need "+new Number(missing)+" more "+GameSession.Content.resources[u.costResource]+" · "+EtaPc(sim.PurchaseEta(u));
  }
  static string EtaPc(double seconds){if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<0)return "No ETA at current net rate";if(seconds<1)return "< 1s";if(seconds>315360000)return "> 10 years";long t=(long)Math.Ceiling(seconds);return "~"+(t<60?t+"s":t<3600?t/60+"m "+t%60+"s":t<86400?t/3600+"h "+t%3600/60+"m":t/86400+"d "+t%86400/3600+"h");}
  string UpgradeEffectPc(Upgrade u)=>(u.target<0?"All processes":GameSession.Content.generators[u.target].name)+" · capacity +"+((u.effect-1)*100).ToString("0.#")+"%";
  void ReadableResearchPc(Upgrade[] defs){
   PurchaseGuidePc();bool research=page=="Research";var p=PrefsPc;int filter=research?p.researchFilter:p.upgradeFilter;pcResearchFilter=filter;
   string query=research?p.researchSearch:p.upgradeSearch;bool favorites=research?p.researchFavorites:p.upgradeFavorites;
   if(pcCompactNav){PcButton(new[]{"All","Available","Purchased"}[filter]+(favorites?" / Favorites":"")+(query!=""?" / Search active":"")+" · Search & filters",list,()=>ResearchFilterDialogPc(research));}
   else{var toolbar=HRow(list,"Research tools",0,8);pcListSearch=SearchFieldPc(toolbar,query,"Search research…",value=>{if(research)p.researchSearch=value;else p.upgradeSearch=value;pcSearchDue=Time.unscaledTime+.18f;});
   var favorite=PcButton(favorites?"Favorites: On":"Favorites: Off",toolbar,()=>{if(research)p.researchFavorites=!p.researchFavorites;else p.upgradeFavorites=!p.upgradeFavorites;ShowPc();});EmphasizePc(favorite,favorites);
   if(research)PcButton("Research plan ("+EconomyPc.S.researchPlan.queue.Count+")",toolbar,ResearchPlanPc);
   var modes=HRow(list,"Research filters",0,8);for(int i=0;i<3;i++){int f=i;var button=PcButton(new[]{"All","Available","Purchased"}[i],modes,()=>{if(research)p.researchFilter=f;else p.upgradeFilter=f;ShowPc();});EmphasizePc(button,filter==i);button.name="Research filter "+i;}}
   var sim=EconomyPc;var visible=defs.Where(u=>sim.StageOpen(u.stage)&&(filter!=1||sim.CanResearch(u))&&(filter!=2||sim.S.purchases.Contains(u.id))&&(!favorites||FavoritePc(u.id))&&MatchesUpgradePc(u,query)).ToArray();
   Caption(visible.Length+" RESULTS / "+WorldNamePc,list);if(filter==1)Label("Available results refresh when you change filters or reopen this view.",list,18,0,CosmicStyle.Muted);
   if(visible.Length==0){Label("No matches. Try a broader search or clear the filters.",list,18);PcButton("Clear research filters",list,()=>{if(research){p.researchFilter=0;p.researchSearch="";p.researchFavorites=false;}else{p.upgradeFilter=0;p.upgradeSearch="";p.upgradeFavorites=false;}ShowPc();});return;}
   foreach(var u in visible){var d=u;bool expanded=pcExpandedResearch==d.id;
    var card=PCard(list,"Research row "+u.id);VerticalPaddingPc(card,12,5);var panel=card.GetComponent<CosmicPanel>();panel.color=CosmicStyle.Tint(CosmicStyle.Section(1),.025f);panel.edgeColor=expanded?CosmicStyle.Section(1):CosmicStyle.Edge;
    var columns=HRow(card,"Research columns",0,12);columns.GetComponent<ObservatoryFlow>().cellWidth=280*PcScale;
    var identity=PcBox("Research identity",columns,Color.clear);Vertical(identity,0,3);var name=PcButton((expanded?"− ":"+ ")+d.name+(FavoritePc(d.id)?" *":""),identity,()=>{pcExpandedResearch=expanded?"":d.id;ShowPc();});name.name="Expand research "+d.id;Hint(name,"Expand details, compare actual production, favorite or pin this goal.");Label(UpgradeEffectPc(u),identity,18,0,CosmicStyle.Muted);
    var buying=PcBox("Research purchase",columns,Color.clear);Vertical(buying,0,3);var button=Primary("Acquire",buying,()=>AcquireResearchPc(d));button.name="Acquire "+d.id;var cost=Label("",buying,18,0,CosmicStyle.Muted);ResearchBarPc(button,d);
    var status=Label("",card,18);BindVisible(status,()=>{var e=EconomyPc;bool owned=e.S.purchases.Contains(d.id),ready=e.CanResearch(d);status.text=UpgradeStatusPc(e,d);status.color=ready?CosmicStyle.Section(1):CosmicStyle.Muted;cost.text="Cost "+new Number(BigDouble.Parse(d.cost))+" "+GameSession.Content.resources[d.costResource];button.interactable=ready;button.GetComponentInChildren<TMP_Text>().text=owned?"Purchased":ready?"Acquire · "+new Number(BigDouble.Parse(d.cost))+" "+GameSession.Content.resources[d.costResource]:"Acquire";});
    if(expanded)ResearchDetailsContentPc(card,d,research);
   }
  }
  void ResearchFilterDialogPc(bool research){
   var p=PrefsPc;int filter=research?p.researchFilter:p.upgradeFilter;bool favorite=research?p.researchFavorites:p.upgradeFavorites;string query=research?p.researchSearch:p.upgradeSearch;
   var dialog=ModalPc("Search & filters",920,780);var field=SearchFieldPc(dialog,query,"Search name or resource…",value=>query=value);var choices=HRow(dialog,"Filter choices",0);var buttons=new List<Button>();
   for(int i=0;i<3;i++){int index=i;var b=PcButton(new[]{"All","Available","Purchased"}[i],choices,()=>{filter=index;for(int k=0;k<buttons.Count;k++)EmphasizePc(buttons[k],k==filter);});EmphasizePc(b,filter==i);buttons.Add(b);}
   Button favorites=null;favorites=PcButton("Favorites: "+(favorite?"On":"Off"),dialog,()=>{favorite=!favorite;favorites.GetComponentInChildren<TMP_Text>().text="Favorites: "+(favorite?"On":"Off");EmphasizePc(favorites,favorite);});EmphasizePc(favorites,favorite);
   var actions=HRow(dialog,"Apply filters",0);Primary("Apply filters",actions,()=>{if(research){p.researchSearch=query;p.researchFavorites=favorite;p.researchFilter=filter;}else{p.upgradeSearch=query;p.upgradeFavorites=favorite;p.upgradeFilter=filter;}ClosePcModal();ShowPc();});PcButton("Clear filters",actions,()=>{query="";filter=0;favorite=false;field.SetTextWithoutNotify("");for(int k=0;k<buttons.Count;k++)EmphasizePc(buttons[k],k==0);favorites.GetComponentInChildren<TMP_Text>().text="Favorites: Off";EmphasizePc(favorites,false);});PcButton("Research plan",dialog,ResearchPlanPc);PcButton("Cancel",dialog,ClosePcModal);FocusModalPc();
  }
  bool MatchesUpgradePc(Upgrade u,string query)=>string.IsNullOrWhiteSpace(query)||(u.name+" "+u.description+" "+UpgradeEffectPc(u)+" "+GameSession.Content.resources[u.costResource]).IndexOf(query.Trim(),StringComparison.OrdinalIgnoreCase)>=0;
  void AcquireResearchPc(Upgrade u){bool acquired=false;EditEconomyPc(e=>acquired=e.Purchase(u));if(acquired)ToastPc("Acquired / "+u.name);ClosePcModal();ShowPc();}
  void ResearchDetailsContentPc(Transform parent,Upgrade u,bool research){
   Label(u.description,parent,18,0,CosmicStyle.Muted);Label("Capacity is the installed multiplier. Actual net output also depends on inputs, reserves and spending. ETA assumes unchanged rates and no purchases or resets.",parent,18,0,CosmicStyle.Muted);
   var actions=HRow(parent,"Research detail actions",0,8);PcButton(FavoritePc(u.id)?"Remove favorite":"Add favorite",actions,()=>{ToggleFavoritePc(u.id);ClosePcModal();ShowPc();});PcButton(PrefsPc.goal==u.id&&PrefsPc.goalWorld==pcWorld?"Unpin goal":"Pin goal",actions,()=>{if(PrefsPc.goal==u.id&&PrefsPc.goalWorld==pcWorld)PrefsPc.goal="";else{PrefsPc.goal=u.id;PrefsPc.goalWorld=pcWorld;}pcGoalNotice="";ClosePcModal();ShowPc();});
   if(research){bool queued=EconomyPc.S.researchPlan.queue.Contains(u.id);var q=PcButton(queued?"Remove from plan":"Queue with prerequisites",actions,()=>{EditEconomyPc(e=>{if(queued)e.RemoveQueuedResearch(u.id);else e.QueueResearch(u.id);});ClosePcModal();ShowPc();});q.interactable=!EconomyPc.S.purchases.Contains(u.id);}
   var preview=PcButton("Compare production",actions,()=>ResearchComparisonPc(u));preview.interactable=EconomyPc.CanResearch(u);Hint(preview,"Available when the research can be purchased. Computes a disposable snapshot without spending your resources.");
   if(!string.IsNullOrEmpty(u.requires)){string dependency=u.requires;PcButton("Show prerequisite: "+EconomyPc.FindUpgrade(dependency).name,parent,()=>OpenUpgradePc(dependency));}
  }
  void OpenUpgradePc(string id){
   var u=EconomyPc.FindUpgrade(id);if(u==null)return;ClosePcModal();var p=PrefsPc;bool research=GameSession.Content.research.Any(x=>x.id==id);if(research){p.researchFilter=0;p.researchSearch=u.name;p.researchFavorites=false;}else{p.upgradeFilter=0;p.upgradeSearch=u.name;p.upgradeFavorites=false;}pcExpandedResearch=id;NavigatePc(research?"Research":"Upgrades");if(pcScroll)pcScroll.verticalNormalizedPosition=1;
  }
  void ResearchComparisonPc(Upgrade u){
   var e=EconomyPc;e.RefreshRates();var after=e.PreviewRates(e.Meta.laws,e.Meta.ascensionUpgrades,purchase:u);var dialog=ModalPc("Production comparison / "+u.name,1040,800);
   Label("Snapshot for "+WorldNamePc+". Estimated net rates after paying the cost. Current inputs and reserves are included; future purchases and resets can change these results.",dialog,18,0,CosmicStyle.Muted);
   if(after==null)Label("Requirements changed. Gather the missing resources before comparing.",dialog,18);
   else{bool changed=false;for(int i=0;i<12;i++){if(!e.StageOpen(i/2))continue;var before=e.CachedRate(i);if(before==after[i].Value)continue;changed=true;var card=PCard(dialog,"Comparison "+i);Label(GameSession.Content.resources[i]+" / net per second",card,20);Label(new Number(before)+" → "+after[i]+"   ("+(after[i].Value>=before?"+":"")+new Number(after[i].Value-before)+")",card,20);}if(!changed)Label("No immediate net-rate change. Check input limits and protected resources; installed capacity still increases.",dialog,18);var acquire=Primary("Acquire research",dialog,()=>AcquireResearchPc(u));ResearchBarPc(acquire,u);}
   PcButton("Close",dialog,ClosePcModal);FocusModalPc();
  }
  void PinnedGoalPc(Transform parent){
   var p=PrefsPc;if(string.IsNullOrEmpty(p.goal))return;var e=Sim.Universe(p.goalWorld);var u=e?.FindUpgrade(p.goal);if(u==null)return;
   if(pcCompactNav){var b=PcButton("Goal / "+u.name,parent,()=>{pcWorld=p.goalWorld;OpenUpgradePc(u.id);});Hint(b,"Pinned goal. Expand its details to unpin.");return;}
   var card=PCard(parent,"Pinned goal");VerticalPaddingPc(card,10,4);Caption("PINNED / "+(p.goalWorld==0?"HOME":"UNIVERSE "+p.goalWorld),card);var row=HRow(card,"Pinned goal actions",0,8);Label(u.name,row,20);PcButton("Open goal",row,()=>{pcWorld=p.goalWorld;OpenUpgradePc(u.id);});PcButton("Unpin",row,()=>{p.goal="";ShowPc();});
   var text=Label("",card,18,48,CosmicStyle.Muted);BindVisible(text,()=>{if(e!=Sim&&(!LocalPagePc||e!=EconomyPc))e.RefreshRates();text.text=UpgradeStatusPc(e,u);});
  }
  void CheckPinnedGoalPc(){
   if(S==null||S.offlineRemaining>0||!PrefsPc.readyNotifications||string.IsNullOrEmpty(PrefsPc.goal))return;var e=Sim.Universe(PrefsPc.goalWorld);var u=e?.FindUpgrade(PrefsPc.goal);if(u==null)return;
   string stamp=S.profileId+"/"+PrefsPc.goalWorld+"/"+u.id;bool complete=e.S.purchases.Contains(u.id),ready=e.CanResearch(u);string notice=stamp+(complete?"/done":ready?"/ready":"/waiting");if(notice==pcGoalNotice)return;pcGoalNotice=notice;if(complete)ToastPc("Goal complete / "+u.name);else if(ready)ToastPc("Goal ready / "+u.name);
  }
  void ResearchPlanPc(){
   var e=EconomyPc;var plan=e.S.researchPlan;var dialog=ModalPc("Research plan / "+WorldNamePc,1120,860);
   Label("Ordered plan. Adding research includes missing prerequisites. Removing a prerequisite also removes its dependants. Plans are saved for each economy.",dialog,18,0,CosmicStyle.Muted);
   var mode=PcButton("Automatic queue: "+(plan.automatic?"On":"Off"),dialog,()=>{EditEconomyPc(s=>s.S.researchPlan.automatic=!s.S.researchPlan.automatic);ResearchPlanPc();});mode.interactable=e.HasA("a_time_2")||plan.automatic;
   var ruleState=Label("",dialog,18);Bind(()=>ruleState.text=EconomyPc.ExplainRule("research").ToString());PcButton("Save this economy operations",dialog,()=>{Sim.SaveOperations(pcWorld,"Economy "+pcWorld);ToastPc("Operations saved. Review and apply them in Automation.");});
   Label(!e.HasA("a_time_2")?"Manual plan available. Automatic execution requires Temporal workshop in the Time tree.":"Automatic execution runs at simulation decisions, including offline time. It respects reserves and the construction planner. The queue has priority over general autobuy and automatic research; a blocked first item waits. Resets pause execution.",dialog,18,0,CosmicStyle.Muted);
   if(plan.queue.Count==0)Label("Your plan is empty. Expand a research row and choose Queue with prerequisites.",dialog,18);
   for(int i=0;i<plan.queue.Count;i++){string id=plan.queue[i];var u=e.FindUpgrade(id);var c=PCard(dialog,"Queued "+id);VerticalPaddingPc(c,12,5);Label((i+1)+". "+u.name,c,22);var status=Label("",c,18,0,CosmicStyle.Muted);Bind(()=>{var current=EconomyPc;status.text=UpgradeStatusPc(current,u)+(current.ResearchAvailable(u.costResource)<BigDouble.Parse(u.cost)&&current.CanResearch(u)?" · Automatic queue is waiting for unreserved stock":"");});
    var actions=HRow(c,"Queue actions",0,8);PcButton("Open",actions,()=>OpenUpgradePc(id));var acquire=PcButton("Acquire now",actions,()=>{bool ok=false;EditEconomyPc(s=>ok=s.Purchase(u));if(ok)ToastPc("Acquired / "+u.name);ResearchPlanPc();});ResearchBarPc(acquire,u);
    var up=PcButton("Move earlier",actions,()=>{bool moved=false;EditEconomyPc(s=>moved=s.MoveQueuedResearch(id,-1));if(!moved)ToastPc("Prerequisites must stay before dependent research.");ResearchPlanPc();});up.interactable=i>0;
    var down=PcButton("Move later",actions,()=>{bool moved=false;EditEconomyPc(s=>moved=s.MoveQueuedResearch(id,1));if(!moved)ToastPc("Prerequisites must stay before dependent research.");ResearchPlanPc();});down.interactable=i+1<plan.queue.Count;
    PcButton("Remove",actions,()=>{EditEconomyPc(s=>s.RemoveQueuedResearch(id));ResearchPlanPc();});
   }
   var bottom=HRow(dialog,"Plan actions",0);PcButton("Browse research",bottom,()=>NavigatePc("Research"));PcButton("Close",bottom,()=>{ClosePcModal();ShowPc();});FocusModalPc();
  }
  void SearchPalettePc(bool favorites){
   pcSearchFavorites=favorites;pcPaletteQuery="";var dialog=ModalPc(favorites?"Favorites":"Find in Observatory",1040,840);Label("Find screens, processes, research and upgrades. Results use the selected economy: "+WorldNamePc+".",dialog,18,0,CosmicStyle.Muted);
   pcPaletteInput=SearchFieldPc(dialog,"","Type a name, category or resource…",query=>{pcPaletteQuery=query;pcPaletteDue=Time.unscaledTime+.12f;});var actions=HRow(dialog,"Search options",0);PcButton(favorites?"Search everything":"Favorites only",actions,()=>SearchPalettePc(!favorites));PcButton("Close",actions,ClosePcModal);
   pcPaletteResults=PcBox("Search results",dialog,Color.clear);Vertical(pcPaletteResults,0,6);PopulatePalettePc();FocusModalPc();if(Application.isPlaying)pcPaletteInput.ActivateInputField();
  }
  void PopulatePalettePc(){
   EconomyPc.DiscoverProcesses();Clear(pcPaletteResults);int count=0,total=0;bool Match(string key,string words)=> (!pcSearchFavorites||FavoritePc(key))&&(string.IsNullOrWhiteSpace(pcPaletteQuery)||words.IndexOf(pcPaletteQuery.Trim(),StringComparison.OrdinalIgnoreCase)>=0);
   void Result(string key,string title,Action open){total++;if(count++>=30)return;var row=HRow(pcPaletteResults,"Search result",0,8);row.GetComponent<ObservatoryFlow>().cellWidth=300*PcScale;PcButton(title,row,open);PcButton(FavoritePc(key)?"Remove favorite":"Add favorite",row,()=>{ToggleFavoritePc(key);PopulatePalettePc();});}
   for(int i=0;i<ObservatoryRoutes.Length;i++)for(int j=0;j<ObservatoryRoutes[i].Length;j++){string route=ObservatoryRoutes[i][j],label=ObservatorySections[i]+" / "+ObservatoryLabels[i][j];if(RouteUnlockedPc(route)&&Match("page:"+route,label))Result("page:"+route,label,()=>NavigatePc(route));}
   foreach(string route in new[]{"Settings","Credits"})if(Match("page:"+route,route))Result("page:"+route,route,()=>NavigatePc(route));
   for(int i=0;i<GameSession.Content.generators.Length;i++){int id=i;var g=GameSession.Content.generators[i];if(!EconomyPc.ProcessDiscovered(i)||!Match("process:"+i,g.name+" "+GameSession.Content.resources[g.output]))continue;Result("process:"+i,"Process / "+g.name,()=>{PrefsPc.productionStage=pcProductionStage=-1;PrefsPc.availability=pcAvailability=0;NavigatePc("Production");ProcessDetailsPc(id);});}
   foreach(var u in GameSession.Content.research.Concat(GameSession.Content.upgrades)){string id=u.id;if(!EconomyPc.StageOpen(u.stage)||!Match(id,u.name+" "+UpgradeEffectPc(u)+" "+GameSession.Content.resources[u.costResource]))continue;Result(id,(GameSession.Content.research.Contains(u)?"Research / ":"Upgrade / ")+u.name,()=>OpenUpgradePc(id));}
   if(total==0)Label(pcSearchFavorites?"No matching favorites. Add them from search results or expanded research.":"No matches. Try a resource or screen name.",pcPaletteResults,18);else if(total>30)Label("Showing 30 of "+total+" matches. Refine your search.",pcPaletteResults,18,0,CosmicStyle.Muted);
  }
  void ShortcutHelpPc(){var dialog=ModalPc("Keyboard shortcuts",780,650);Label("Ctrl+K  — Find screens, processes and research\nAlt+1…6  — Open the six main categories\nTab / Shift+Tab  — Move between controls\nEnter / Space  — Activate a focused button\nEscape  — Close dialog / return to Overview\nF1  — This guide\n\nNavigation shortcuts are disabled while typing and while a dialog is open.",dialog,18);PcButton("Close",dialog,ClosePcModal);FocusModalPc();}
 }
}
