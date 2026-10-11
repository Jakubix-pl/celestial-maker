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
  public const string ObservatoryVersion="2.8.3";
  static readonly string[] ObservatorySections={"Overview","Production","Development","Automation","Cosmos","Journal"};
  static readonly string[][] ObservatoryRoutes={new[]{"Overview"},new[]{"Production","Research","Upgrades","Analytics"},new[]{"Prestige","Trees","Roadmap","Challenges"},new[]{"Automation"},new[]{"Megastructures","Reality laws","Parallel universes","Constellations","Expeditions"},new[]{"Achievements","Codex","Offline summary"}};
  static readonly string[][] ObservatoryLabels={new[]{"Overview"},new[]{"Processes","Research","Upgrades","Flows"},new[]{"Resets","Trees & profiles","Milestones","Challenges"},new[]{"Automation"},new[]{"Megastructures","Reality laws","Universes","Connections","Expeditions"},new[]{"Achievements","Codex","Last absence"}};
  int pcSelectedProcess=-1,pcAvailability,pcWorld,pcModalBindingStart,pcModalLiveStart,pcDiscoveryMask=-1;
  readonly List<Action> pcInspectorBindings=new List<Action>();
  GameObject pcReturnFocus;
  float PcScale=>Mathf.Clamp(GameSession.Instance?.CurrentSettings.textScale??1,.8f,2);
  Simulation pcEconomy,pcEconomyHome;int pcEconomyWorld=-1;
  Simulation EconomyPc {get{if(pcEconomyHome!=Sim||pcEconomyWorld!=pcWorld){pcEconomyHome=Sim;pcEconomyWorld=pcWorld;pcEconomy=pcWorld>0?Sim.Universe(pcWorld):Sim;}return pcEconomy??Sim;}}
  bool LocalPagePc=>page=="Production"||page=="Research"||page=="Upgrades"||page=="Analytics";
  string WorldNamePc=>pcWorld==0?"Home universe":Sim.Meta.universes.FirstOrDefault(u=>u.id==pcWorld)?.name??"Home universe";
  int SectionPc(string route){for(int i=0;i<ObservatoryRoutes.Length;i++)if(ObservatoryRoutes[i].Contains(route))return i;return -1;}
  string PageTitlePc(){int section=SectionPc(page);return section<0?page:ObservatoryLabels[section][Array.IndexOf(ObservatoryRoutes[section],page)];}
  static float NextTextScalePc(float current)=>current<1?1:current<1.25f?1.25f:current<1.5f?1.5f:current<2?2:1;
  bool RouteUnlockedPc(string route){
   if(S==null)return route=="Settings"||route=="Credits";
   int group=SectionPc(route);if(group>=0&&!Sim.SectionUnlocked(group))return false;
   switch(route){case "Trees":case "Megastructures":case "Reality laws":return Sim.HasLayer(1);case "Parallel universes":return Sim.UniverseCapacity>1;case "Constellations":case "Challenges":return Sim.HasLayer(0);case "Expeditions":return Sim.StageOpen(1)||Sim.HasLayer(0);case "Offline summary":return S.offlineReport!=null;default:return true;}
  }
  string SectionEntryPc(int section)=>ObservatoryRoutes[section].FirstOrDefault(RouteUnlockedPc)??"Overview";
  int NavigationStampPc()=>Enumerable.Range(0,6).Where(i=>Sim.SectionUnlocked(i)).Aggregate(0,(mask,i)=>mask|(1<<i))+(Sim.HasLayer(1)?64:0)+(Sim.UniverseCapacity>1?128:0)+(S.offlineReport!=null?256:0);
  void NavigatePc(string route){if(!RouteUnlockedPc(route))return;pcSearchDue=pcPaletteDue=-1;ClosePcModal();page=route;ShowPc();}
  void EditEconomyPc(Action<Simulation> edit){if(pcWorld>0&&Sim.Meta.universes.Any(u=>u.id==pcWorld))Sim.EditUniverse(pcWorld,edit);else edit(Sim);}

  void BuildObservatoryPc(){
   LoadReadabilityPreferencesPc();
   CosmicStyle.Light=GameSession.Instance?.CurrentSettings.lightUI??false;
   pcBody=Resources.Load<TMP_FontAsset>("UI/PC/BodyFontHD");pcDisplay=Resources.Load<TMP_FontAsset>("UI/PC/DisplayFontHD");
   Clear(transform);pcBindings.Clear();pcShellBindings.Clear();pcLive.Clear();pcLiveShell.Clear();pcInspectorBindings.Clear();pcTabs.Clear();processText.Clear();ledgerText.Clear();pcModal=null;offlineDialog=null;pcToast=null;pcInspector=null;pcStage=-1;
   previousWidth=Screen.width;previousHeight=Screen.height;portrait=false;
   var scaler=GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;scaler.scaleFactor=PhoneObservatory&&Application.isPlaying?Mathf.Max(1,Mathf.Min(Screen.width,Screen.height)/480f):1;
   if(PhoneObservatory&&Application.isPlaying)GetComponent<Canvas>().scaleFactor=scaler.scaleFactor;Canvas.ForceUpdateCanvases();pcLastSafeArea=Screen.safeArea;
   var background=PcBox("Observatory background",transform,CosmicStyle.Background);Stretch(background,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
   float inset=Mathf.Max(0,(((RectTransform)transform).rect.width-2560*Mathf.Max(1,PcScale*.75f))/2);
   root=PcBox("PC Observatory",background,CosmicStyle.Background);root.GetComponent<CosmicPanel>().edge=0;Stretch(root,Vector2.zero,Vector2.one,new Vector2(inset,0),new Vector2(-inset,0));
   if(PhoneObservatory&&Application.isPlaying){var safe=Screen.safeArea;Stretch(root,new Vector2(safe.x/Screen.width,safe.y/Screen.height),new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height),Vector2.zero,Vector2.zero);}Canvas.ForceUpdateCanvases();
   if(menu){BuildMenuPc();return;}
   if(pcWorld>0&&!Sim.Meta.universes.Any(u=>u.id==pcWorld))pcWorld=0;
   pcCompactNav=PhoneObservatory||root.rect.width<1100*PcScale;
   float pad=PhoneObservatory?8:16,rail=pcCompactNav?0:Mathf.Max(236,164*PcScale+24),brandWidth=PhoneObservatory?104:pcCompactNav?160*PcScale:rail,top=88*PcScale,foot=36*PcScale,inspector=!PhoneObservatory&&root.rect.width>=1750*PcScale?336:0;
   var header=PcBox("Top resource bar",root,CosmicStyle.Panel);Stretch(header,new Vector2(0,1),Vector2.one,new Vector2(pad,-top-pad),new Vector2(-pad,-pad));
   var brand=PcBox("Brand",header,Color.clear);Stretch(brand,Vector2.zero,new Vector2(0,1),new Vector2(16,8),new Vector2(brandWidth-8,-8));Vertical(brand,0,0);
   if(pcCompactNav)PcButton(PhoneObservatory?"Menu":"Menu / Find",brand,NavigationDrawerPc);
   if(!PhoneObservatory){var wordmark=Label("CELESTIAL\nMAKER",brand,22);wordmark.font=pcDisplay?pcDisplay:pcBody;wordmark.characterSpacing=2;if(pcCompactNav){wordmark.text="CELESTIAL MAKER";wordmark.fontSize=14*PcScale;}}
   var meters=PcBox("Live resource meters",header,Color.clear);Stretch(meters,Vector2.zero,Vector2.one,new Vector2(brandWidth+16,8),new Vector2(-12,-8));var flow=meters.gameObject.AddComponent<HorizontalLayoutGroup>();flow.spacing=20;flow.childControlWidth=flow.childForceExpandWidth=flow.childControlHeight=flow.childForceExpandHeight=true;
   PresentationCanvas(meters,false);
   int meterCount=PhoneObservatory&&root.rect.width<600*PcScale?1:root.rect.width<1500*PcScale?2:4;
   for(int slot=0;slot<meterCount;slot++)ResourceMeterPc(meters,slot);
   if(!pcCompactNav){nav=PcBox("Navigation rail",root,CosmicStyle.Panel);Stretch(nav,Vector2.zero,new Vector2(0,1),new Vector2(pad,foot+pad*2),new Vector2(pad+rail,-top-pad*2));pcNav=ScrollContent(nav,out var ns,12);
   Caption("OBSERVATORY",pcNav);
   foreach(string section in ObservatorySections){int i=Array.IndexOf(ObservatorySections,section);if(!Sim.SectionUnlocked(i))continue;string route=SectionEntryPc(i);var b=PcButton(section,pcNav,()=>NavigatePc(route));pcTabs[section]=b;((ObservatoryButton)b).Accent(CosmicStyle.Section(i));CategoryMarkPc(b,i);}
   Hairline(pcNav,CosmicStyle.Edge);PcButton("Settings",pcNav,()=>NavigatePc("Settings"));PcButton("Save now",pcNav,()=>{GameSession.Instance.Save();ToastPc(GameSession.Instance.SaveFailed?"Save failed. Open Settings for details.":"Your cosmos has been saved.");});
   PcButton("Find / Ctrl+K",pcNav,()=>SearchPalettePc(false));PcButton("Favorites",pcNav,()=>SearchPalettePc(true));PcButton("Research plan",pcNav,ResearchPlanPc);}
   body=PcBox("Workspace",root,CosmicStyle.Background);body.GetComponent<CosmicPanel>().edge=0;Stretch(body,Vector2.zero,Vector2.one,new Vector2(pcCompactNav?pad:rail+pad*2,foot+pad*2),new Vector2(-pad-(inspector>0?inspector+pad:0),-top-pad*2));PresentationCanvas(body,true);list=ScrollContent(body,out pcScroll,16);
   if(inspector>0){var host=PcBox("Context rail",root,CosmicStyle.Panel);Stretch(host,new Vector2(1,0),Vector2.one,new Vector2(-pad-inspector,foot+pad*2),new Vector2(-pad,-top-pad*2));PresentationCanvas(host,true);pcInspector=ScrollContent(host,out var sc,18);}
   var footer=PcBox("Status strip",root,CosmicStyle.Panel);Stretch(footer,Vector2.zero,new Vector2(1,0),new Vector2(pad,pad),new Vector2(-pad,foot+pad));status=Label("Local save ready",footer,18);Stretch((RectTransform)status.transform,Vector2.zero,Vector2.one,new Vector2(12,2),new Vector2(-12,-2));
   pcToastPanel=PcBox("Notification",root,CosmicStyle.Raised);Stretch(pcToastPanel,new Vector2(.25f,0),new Vector2(.85f,0),new Vector2(0,foot+40),new Vector2(0,foot+40+100*PcScale));pcToast=Label("",pcToastPanel,18);Stretch((RectTransform)pcToast.transform,Vector2.zero,Vector2.one,new Vector2(16,12),new Vector2(-16,-12));pcToastPanel.gameObject.SetActive(false);
   pcLastPage="";ShowPc();RefreshPc();
  }

  void ObservatoryMenuPc(){
   if(PhoneObservatory){var phonePane=PcBox("Menu panel",root,CosmicStyle.Panel);Stretch(phonePane,Vector2.zero,Vector2.one,new Vector2(8,44*PcScale),new Vector2(-8,-8));list=ScrollContent(phonePane,out pcScroll,20);nav=null;body=phonePane;status=Label(ObservatoryVersion+" / ENGLISH",root,18);Stretch((RectTransform)status.transform,Vector2.zero,new Vector2(1,0),new Vector2(12,4),new Vector2(-12,40*PcScale));ShowMenuPc();return;}
   var pane=PcBox("Menu panel",root,CosmicStyle.Panel);Stretch(pane,new Vector2(0,0),new Vector2(1,1),new Vector2(48,40+36*PcScale),new Vector2(-Mathf.Max(48,root.rect.width*.4f),-48));list=ScrollContent(pane,out pcScroll,32);nav=null;body=pane;
   var artHost=PcBox("Observatory artwork",root,CosmicStyle.Background);artHost.GetComponent<CosmicPanel>().edge=0;Stretch(artHost,new Vector2(.62f,.06f),new Vector2(.98f,.96f),Vector2.zero,Vector2.zero);ObservatoryArt(artHost,"ObservatoryPortrait");
   var sigil=PcBox("Celestial emblem",root,Color.clear);Stretch(sigil,new Vector2(.88f,.06f),new Vector2(.96f,.18f),Vector2.zero,Vector2.zero);ObservatoryArt(sigil,"CelestialIcon");
   status=Label("CELESTIAL MAKER  /  "+ObservatoryVersion+"  /  ENGLISH",root,18);Stretch((RectTransform)status.transform,Vector2.zero,new Vector2(1,0),new Vector2(48,16),new Vector2(-48,16+32*PcScale));ShowMenuPc();
  }
  void ShowObservatoryPc(){
   if(!list)return;if(S==null&&page!="Settings"&&page!="Credits"){ShowMenuPc();return;}
   float scroll=pcScroll?pcScroll.verticalNormalizedPosition:1;bool same=page==pcLastPage;pcLastPage=page;
   pcBindings.Clear();pcLive.Clear();pcInspectorBindings.Clear();processText.Clear();ledgerText.Clear();Clear(list);pcGridIndex=0;pcGridRow=null;
   int group=SectionPc(page);foreach(var tab in pcTabs)EmphasizePc(tab.Value,group>=0&&ObservatorySections[group]==tab.Key);
   if(pcCompactNav&&!menu){var title=PcButton((group>=0?ObservatorySections[group]+" / ":"")+PageTitlePc()+" · Views",list,SectionViewsPc);SectionTabPc(title,page,true);}
   else{if(!menu){var context=PcButton(LocalPagePc?"ECONOMY / "+WorldNamePc:"GLOBAL SYSTEMS / Home universe",list,ChooseWorldPc);Hint(context,"Choose the economy shown in Production, Research and Flows. Development and cosmic systems are global.");}
   var sectionHeader=PCard(list,"Category header");sectionHeader.GetComponent<CosmicPanel>().color=CosmicStyle.Tint(CosmicStyle.Section(Math.Max(0,group)));sectionHeader.GetComponent<CosmicPanel>().edgeColor=CosmicStyle.Section(Math.Max(0,group));VerticalPaddingPc(sectionHeader,12,6);
   Caption(group>=0?"OBSERVATORY / "+ObservatorySections[group]:"CELESTIAL MAKER",sectionHeader).color=CosmicStyle.Section(Math.Max(0,group));Label(PageTitlePc(),sectionHeader,30);
   if(group>=0&&ObservatoryRoutes[group].Length>1){var tabs=HRow(list,"Section tabs",0,8);for(int i=0;i<ObservatoryRoutes[group].Length;i++){string route=ObservatoryRoutes[group][i];if(!RouteUnlockedPc(route))continue;var tab=PcButton(ObservatoryLabels[group][i],tabs,()=>NavigatePc(route));SectionTabPc(tab,route,route==page);}}
   }
   StarterGuidePc();PinnedGoalPc(list);
   Hairline(list,CosmicStyle.Edge);Canvas.ForceUpdateCanvases();pcColumns=Mathf.Max(1,Mathf.Min(3,Mathf.FloorToInt(list.rect.width/(360*PcScale))));
   if(page=="Settings")SettingsPc();else if(page=="Credits")Label("Celestial Maker\nOriginal universe, interface and audio.\nIBM Plex Sans — IBM (OFL). Rajdhani — Indian Type Foundry (OFL).\nBreakInfinity.cs — Andrei Andreev (MIT).\nUnity URP and TextMeshPro.",list,18);
   else switch(page){case "Overview":OverviewPc();break;case "Production":ProductionPc();break;case "Research":ObservatoryResearchPc(GameSession.Content.research);break;case "Upgrades":ObservatoryResearchPc(GameSession.Content.upgrades);break;case "Analytics":ObservatoryAnalyticsPc();break;case "Prestige":PrestigePc();break;case "Trees":if(!Sim.HasLayer(1))LockedPc("Ascension required","Ascend to unlock Energy, Matter and Time branches and strategy profiles.");else{MetaTreesPc();SpecializationsPc();}break;case "Roadmap":RoadmapPc();break;case "Challenges":ChallengesPc();break;case "Automation":ObservatoryAutomationPc();break;case "Megastructures":if(Sim.HasLayer(1))MegastructuresPc();else LockedPc("Ascension required","Your first Ascension unlocks construction projects.");break;case "Reality laws":if(Sim.HasLayer(1))LawsPc();else LockedPc("Ascension required","Ascend to choose laws of reality.");break;case "Parallel universes":UniversesPc();break;case "Constellations":ConstellationsPc();break;case "Expeditions":ExpeditionsPc();break;case "Achievements":AchievementsPc();break;case "Codex":CodexPc();break;case "Offline summary":OfflinePc(list,offlineDetails);PcButton(offlineDetails?"Show changed resources":"Expand accounting and events",list,()=>{offlineDetails=!offlineDetails;ShowPc();});break;}
   if(menu)PcButton("Back to main menu",list,ShowMenuPc);
   if(GameSession.Instance.SaveFailed){var error=PCard(list,"Save failure");Label(GameSession.Instance.Message,error,18,0,CosmicStyle.Danger);PcButton("Retry save",error,()=>{GameSession.Instance.Save();ShowPc();});}
   if(!menu&&S!=null)PcButton("Explain this view",list,ExplainViewPc);
   pcCompletedStamp=S==null?0:CompletedStampPc();RefreshInspectorPc();RefreshPc();Canvas.ForceUpdateCanvases();if(pcScroll)pcScroll.verticalNormalizedPosition=same?scroll:1;if(!same)RevealPc(list);
  }
  void ChooseWorldPc(){
   if(S==null)return;var modal=ModalPc("Choose an economy",780,620);Label("Production, research and flows show the selected economy. Resets, trees, campaigns and cosmic projects remain global.",modal,18);
   PcButton("Home universe",modal,()=>{pcWorld=0;pcSelectedProcess=-1;NavigatePc("Production");});foreach(var u in Sim.Meta.universes){int id=u.id;PcButton(u.name,modal,()=>{pcWorld=id;pcSelectedProcess=-1;NavigatePc("Production");});}
   if(Sim.Meta.universes.Count==0)Label("Parallel universes become available through cosmic progression.",modal,18,0,CosmicStyle.Muted);if(RouteUnlockedPc("Parallel universes"))PcButton("Manage universes",modal,()=>NavigatePc("Parallel universes"));PcButton("Close",modal,ClosePcModal);FocusModalPc();
  }

  long RequestedPc(Simulation sim,int id)=>quantity==0?sim.MaxBuy(id):quantity==-1?sim.NextMilestone(id):quantity;
  string ProcessStatePc(Simulation sim,int id){var g=GameSession.Content.generators[id];if(sim.ProcessMaxed(id))return "Maxed";if(sim.S.owned[id]==0)return "Not owned";var capacity=sim.CachedProductionMultiplier(id)*g.rate*sim.S.owned[id];return g.input>=0&&capacity>0&&sim.CachedFlow(id)<capacity*.999?"Input limited":"Running";}
  bool ProcessMatchesPc(Simulation sim,int id){var g=GameSession.Content.generators[id];return sim.ProcessDiscovered(id)&&(pcProductionStage<0||g.stage==pcProductionStage)&&(pcAvailability==0||pcAvailability==1&&sim.MaxBuy(id)>0||pcAvailability==2&&ProcessStatePc(sim,id)=="Input limited");}
  void ObservatoryProductionPc(){
   Label("Purchase bars show funding for one unit. Bulk modes buy as many as you can afford.",list,16,0,CosmicStyle.Muted);var sim=EconomyPc;sim.DiscoverProcesses();if(sim.StarterSupplyProtection)Label("Starter buffer: consumers leave at least 20% of incoming supply for purchases until your first Prestige.",list,16,0,CosmicStyle.Muted);var modes=HRow(list,"Purchase mode",0,8);foreach(long n in new long[]{1,10,0,-1}){long q=n;var b=PcButton(n==0?"Max":n==-1?"Next milestone":"Buy "+n,modes,()=>{quantity=q;ShowPc();});EmphasizePc(b,quantity==q);}
   var filters=HRow(list,"Production filters",0,8);PcButton("Chapter: "+(pcProductionStage<0?"All":CosmicStyle.Chapters[pcProductionStage]),filters,()=>{var stages=Enumerable.Range(0,6).Where(s=>sim.AllProcessesDiscovered||sim.StageOpen(s)).Prepend(-1).ToArray();pcProductionStage=stages[(Array.IndexOf(stages,pcProductionStage)+1)%stages.Length];PrefsPc.productionStage=pcProductionStage;ShowPc();});PcButton("Show: "+new[]{"All processes","Affordable","Input limited"}[pcAvailability],filters,()=>{pcAvailability=(pcAvailability+1)%3;PrefsPc.availability=pcAvailability;ShowPc();});PcButton("Process upgrades",filters,()=>NavigatePc("Upgrades"));
   if(pcAvailability>0)Label("Filtered when you open this view. Refresh to update membership; rows never reorder during production.",list,18,0,CosmicStyle.Muted);
   var ids=Enumerable.Range(0,24).Where(i=>ProcessMatchesPc(sim,i)).OrderBy(sim.ProcessMaxed).ToArray();if(ids.Length==0){Label("No processes match these filters.",list,18);PcButton("Clear filters",list,()=>{pcAvailability=0;pcProductionStage=-1;PrefsPc.availability=0;PrefsPc.productionStage=-1;ShowPc();});}
   foreach(int id in ids){
    var g=GameSession.Content.generators[id];var row=PCard(list,"Process row "+id);var layout=row.GetComponent<VerticalLayoutGroup>();layout.padding=new RectOffset(12,12,10,10);layout.spacing=4;
    var cells=HRow(row,"Process columns",0,12);cells.GetComponent<ObservatoryFlow>().cellWidth=210*PcScale;
    var identity=PcBox("Identity",cells,Color.clear);Vertical(identity,0,2);var select=PcButton(g.name,identity,()=>ProcessDetailsPc(id));select.name="Select process "+id;var state=Label("",identity,18);
    var production=PcBox("Output",cells,Color.clear);Vertical(production,0,2);var output=Label("",production,18);var stock=Label("",production,18,0,CosmicStyle.Muted);
    var action=PcBox("Purchase",cells,Color.clear);Vertical(action,0,2);var buy=Primary("Buy",action,()=>{long count=Math.Min(RequestedPc(EconomyPc,id),EconomyPc.MaxBuy(id));EditEconomyPc(e=>e.Buy(id,count));RefreshPc();});buy.name="Buy process "+id;var cost=Label("",action,18,0,CosmicStyle.Muted);
    BindVisible(state,()=>{var economy=EconomyPc;string condition=ProcessStatePc(economy,id);state.text="×"+economy.S.owned[id]+" · "+condition;state.color=condition=="Input limited"?CosmicStyle.Warning:CosmicStyle.Muted;output.text=new Number(economy.CachedFlow(id))+" "+GameSession.Content.resources[g.output]+" /s";long actual=Math.Min(RequestedPc(economy,id),economy.MaxBuy(id));long priced=Math.Max(1,actual);var price=economy.Cost(id,priced);var missing=BigDouble.Max(0,price-economy.ResearchAvailable(g.costResource));cost.text=actual>0?"Cost "+new Number(price)+" "+GameSession.Content.resources[g.costResource]:"Need "+new Number(missing)+" unreserved "+GameSession.Content.resources[g.costResource]+" · "+EtaPc(economy.CachedRate(g.costResource)>0?(missing/economy.CachedRate(g.costResource)).ToDouble():double.PositiveInfinity);buy.GetComponentInChildren<TMP_Text>().text=economy.ProcessMaxed(id)?"Maxed":"Buy "+priced;buy.interactable=actual>0&&economy.Open(g);if(!economy.Open(g))cost.text="Reach "+g.unlock+" lifetime "+GameSession.Content.resources[Math.Max(0,g.stage*2-1)]+" to build.";if(economy.ProcessMaxed(id))cost.text="Ownership cap reached / "+economy.ProcessCap+" units";Hint(buy,cost.text+". Purchases respect reserves and the construction planner.");row.GetComponent<CosmicPanel>().edgeColor=pcSelectedProcess==id?CosmicStyle.Cyan:CosmicStyle.Edge;});
    ProcessBarPc(buy,()=>EconomyPc,id);BindLive(stock,()=>{var economy=EconomyPc;stock.text="Stock "+economy.S.resources[g.output]+" · Net "+new Number(economy.CachedRate(g.output))+" /s";});
   }
   if(pcSelectedProcess<0&&ids.Length>0)pcSelectedProcess=ids[0];
  }
  void ProcessDetailsPc(int id){pcSelectedProcess=id;if(pcInspector){RefreshInspectorPc();RefreshPc();}else{var dialog=ModalPc(GameSession.Content.generators[id].name,840,780);ProcessDetailContentPc(dialog,id,false);PcButton("Close",dialog,ClosePcModal);FocusModalPc();}}
  void ProcessDetailContentPc(Transform parent,int id,bool live){
   var sim=EconomyPc;sim.RefreshRates();var g=GameSession.Content.generators[id];Caption("PROCESS DIAGNOSTICS",parent);Label(g.name,parent,24);var metrics=Label("",parent,18);var reason=Label("",parent,18,0,CosmicStyle.Muted);
   Action update=()=>{var e=EconomyPc;var capacity=e.CachedProductionMultiplier(id)*g.rate*e.S.owned[id];metrics.text=ProcessStatePc(e,id)+"\nActual output "+new Number(e.CachedFlow(id))+" /s\nInstalled capacity "+new Number(capacity)+" /s\nMultiplier ×"+new Number(e.CachedProductionMultiplier(id));reason.text=g.input<0?"No input consumption.":"Input: "+GameSession.Content.resources[g.input]+"\nConsumption "+new Number(e.CachedConsumption(id))+" /s\nStock "+e.S.resources[g.input]+"\nProtected "+e.S.reserves[g.input]+"\n\n"+(ProcessStatePc(e,id)=="Input limited"?"Increase upstream output or release a reserve. Other processes may be competing for this input.":"Reserves protect stock from spending and consumption.");};update();if(live)pcInspectorBindings.Add(update);
   if(g.input>=0){PcButton(sim.S.reserves[g.input].Value>0?"Release input reserve":"Protect 10% of lifetime input",parent,()=>{EditEconomyPc(e=>e.S.reserves[g.input]=e.S.reserves[g.input].Value>0?BigDouble.Zero:BigDouble.Max(1,e.S.lifetime[g.input].Value*.1));if(pcModal)ClosePcModal();ShowPc();});PcButton("Go to input producer",parent,()=>{int source=Array.FindIndex(GameSession.Content.generators,x=>x.output==g.input&&EconomyPc.Open(x));if(source>=0){pcSelectedProcess=source;pcProductionStage=g.input/2;pcAvailability=0;NavigatePc("Production");}});}
   PcButton(FavoritePc("process:"+id)?"Remove favorite":"Add favorite",parent,()=>{ToggleFavoritePc("process:"+id);if(pcModal)ClosePcModal();ShowPc();});
   Label("Next milestone: +"+sim.NextMilestone(id)+" units",parent,18,0,CosmicStyle.Muted);
  }
  void ObservatoryInspectorPc(){
   pcInspectorBindings.Clear();if(!pcInspector||S==null)return;Clear(pcInspector);
   if(page=="Production"&&pcSelectedProcess>=0){ProcessDetailContentPc(pcInspector,pcSelectedProcess,true);return;}
   Caption("NEXT DECISION",pcInspector);Label(NextDecisionPc(),pcInspector,20);if(RouteUnlockedPc("Roadmap"))PcButton("Open milestones",pcInspector,()=>NavigatePc("Roadmap"));Hairline(pcInspector,CosmicStyle.Edge);Caption("YOUR COSMOS",pcInspector);Label("Time in cosmos\n"+OfflineReport.Duration(S.time)+"\n\nDiscoveries "+S.achievements.Count+" / 48\nChallenges "+S.completedChallenges.Count+" / 12",pcInspector,18);Label(LocalPagePc?"Economy: "+WorldNamePc:"Global progress is shared across your universes.",pcInspector,18,0,CosmicStyle.Muted);
  }
  string NextDecisionPc(){if(S.settings.tutorial&&Sim.Discover.scenario==0&&S.activeChallenge==""&&Sim.FirstSteps().step<=6)return Sim.FirstSteps().title+".";if(Sim.CampaignComplete)return CampaignGoalPc();for(int i=2;i>=0;i--)if(Sim.PrestigeReward(i)>=1&&S.activeChallenge=="")return ResetRules.Names[i]+" is ready. Review the reward and what will reset.";if(Sim.PreparingGenesis)return CampaignGoalPc();int missing=Array.FindIndex(GameSession.Content.generators,g=>Sim.Open(g)&&S.owned[Array.IndexOf(GameSession.Content.generators,g)]==0);return missing>=0?"Build "+GameSession.Content.generators[missing].name+" to extend your production chain.":"Improve the next research discovery and balance your input supply.";}
  void ObservatoryOverviewPc(){

   if(!StarterGuideVisiblePc||Sim.FirstSteps().step>6){var next=PCard(list,"Next decision");Caption("YOUR NEXT DECISION",next);Label(NextDecisionPc(),next,24);var row=HRow(next,"Next actions",0);Primary("Open production",row,()=>NavigatePc("Production"));if(RouteUnlockedPc("Prestige"))PcButton("Review development",row,()=>NavigatePc("Prestige"));PcButton("What needs attention?",row,()=>{var dialog=ModalPc("Your next decisions",940,800);AdvicePc(dialog,Sim);PcButton("Close",dialog,ClosePcModal);FocusModalPc();});}
   if(Sim.HasLayer(0)&&!Sim.AnyAutobuyer&&!PrefsPc.autobuyerGuideSeen&&!S.settings.tutorial){var intro=PCard(list,"First autobuyer introduction");Caption("YOUR FIRST AUTOBUYER",intro);Label("One Prestige Point unlocks four automatic buyers.",intro,22);Label("Start with the Particles package in Automation. Spending a point reduces its unspent production bonus; bought packages survive Prestige and reset at Ascension or Genesis.",intro,18);var help=HRow(intro,"Autobuyer introduction actions",0);PcButton("Learn on a safe copy",help,AutobuyerGuidePc);PcButton("Open Automation",help,()=>NavigatePc("Automation"));PcButton("Dismiss this tip",help,()=>{PrefsPc.autobuyerGuideSeen=true;ShowPc();});}
   if(S.offlineReport!=null&&S.offlineReport.processedSeconds>=60)ReturnBriefPc(list);LivingCosmosPc();
   Sim.RefreshRates();int bottleneck=Enumerable.Range(0,24).Where(i=>ProcessStatePc(Sim,i)=="Input limited").DefaultIfEmpty(-1).First();
   if(bottleneck>=0){var c=PCard(list,"Production bottleneck");Caption("INPUT LIMITED",c).color=CosmicStyle.Warning;Label(GameSession.Content.generators[bottleneck].name+" is below installed capacity.",c,20);PcButton("Inspect this process",c,()=>{pcWorld=0;pcSelectedProcess=bottleneck;pcProductionStage=-1;pcAvailability=0;NavigatePc("Production");ProcessDetailsPc(bottleneck);});}
   HorizonPc(list);CampaignPc(list);DiscoveryOverviewPc();
   if(S.offlineReport!=null){var c=PCard(list,"Last absence");Caption("SINCE YOUR LAST VISIT",c);Label(S.offlineReport.Heading(),c,18);PcButton("Open last absence",c,()=>NavigatePc("Offline summary"));}
   if(!S.settings.tutorial)PcButton("Show tutorial",list,()=>{S.settings.tutorial=true;ShowPc();});
  }

  void ObservatoryResearchPc(Upgrade[] defs)=>ReadableResearchPc(defs);
  void ObservatoryAnalyticsPc(){FlowMapPc();var sim=EconomyPc;Label("Live engine rates. Time estimates assume unchanged production and spending.",list,18,0,CosmicStyle.Muted);for(int i=0;i<12;i++){if(!sim.StageOpen(i/2))continue;int r=i;var c=PCard(list,GameSession.Content.resources[i]);Caption(GameSession.Content.resources[i],c);var value=Label("",c,18);BindLive(value,()=>{var e=EconomyPc;value.text="Stock "+e.S.resources[r]+" · Protected "+e.S.reserves[r]+"\nGross "+new Number(e.CachedGross(r))+" /s · Consumption "+new Number(e.CachedSpend(r))+" /s\nNET "+new Number(e.CachedRate(r))+" /s";});}}

  void ObservatoryResetsPc(){
   for(int i=0;i<3;i++){
    int layer=i;var c=PCard(list,ResetRules.Names[i]);Caption("RESET LAYER "+(i+1),c);Label(ResetRules.Names[i],c,28);
    var reward=Label("",c,24);BindLive(reward,()=>{bool visible=ResetCurrencyVisible(layer);reward.text=visible?"Reward +"+new Number(Sim.PrestigeReward(layer))+" "+ResetRules.Currencies[layer]+"\nBalance "+S.prestige[layer]:"Locked";reward.color=visible?CosmicStyle.Violet:CosmicStyle.Muted;});
    var requirement=Label("",c,18,0,CosmicStyle.Muted);Bind(()=>requirement.text=VisibleResetRequirement(layer));
    var b=Primary("Review "+ResetRules.Names[i],c,()=>ResetPreviewPc(layer,false));Gate(b,()=>ResetCurrencyVisible(layer));Hint(b,"Meet the requirements to unlock this reset, then review its consequences before confirming.");
   }
   if(Sim.HasLayer(0)){Caption("PRESTIGE BRANCHES",list);foreach(int n in Enumerable.Range(0,4).OrderBy(i=>S.prestigePurchases.Contains("p0_"+i))){int node=n;var b=PcButton("Branch "+(n+1)+" · "+Math.Pow(4,n)+" Prestige Points",list,()=>{Sim.PrestigeUpgrade(0,node);ShowPc();});PurchaseBarPc(b,()=>Simulation.PurchaseFunding(S.prestige[0].Value,BigDouble.Pow(4,node)),()=>S.prestige[0].Value>=BigDouble.Pow(4,node),()=>S.prestigePurchases.Contains("p0_"+node));}}
  }
  void ResetPreviewPc(int layer,bool automatic){
   if(!ResetCurrencyVisible(layer)){var locked=ModalPc(ResetRules.Names[layer]+" locked",880,620);Label(VisibleResetRequirement(layer),locked,18);PcButton("Close",locked,ClosePcModal);FocusModalPc();return;}
   var d=ModalPc((automatic?"Enable automatic ":"Review ")+ResetRules.Names[layer],1040,820);Label("Reward now: +"+new Number(Sim.PrestigeReward(layer))+" "+ResetRules.Currencies[layer],d,24,0,CosmicStyle.Violet);
   var columns=HRow(d,"Reset consequences",0,20);columns.GetComponent<ObservatoryFlow>().cellWidth=300*PcScale;
   var kept=PCard(columns,"What stays");Caption("YOU KEEP",kept);Label(ResetRules.Kept+"\n\n"+(layer==0?"Prestige points and upgrades, links, laws, active automation, megastructures and parallel economies remain.":layer==1?"Ascension and Genesis points/trees, talents, laws, megastructures and work remain. Parallel identities stay; their economies restart.":"Genesis points and foundations remain. Parallel identities stay. Cosmic Memory preserves one completed level per megastructure; construction work resets."),kept,18);var reset=PCard(columns,"What resets");Caption("RESETS",reset).color=CosmicStyle.Warning;Label(ResetRules.Reset[layer]+"\n\nAn enabled Development Plan resumes its selected strategy at the next decision. First-time resets remain manual.\n\nAll manual resets clear run lifetime, protected reserves, expedition voyages and expedition levels."+(layer>0?" Advanced reset and expedition rules reset; automatic research is disabled. Transfer schedules are disabled until re-enabled.":""),reset,18);Label(Sim.ResetRequirement(layer),d,18,0,CosmicStyle.Muted);
   RecoveryPanelPc(d,layer);
   if(automatic)Label("The rule can perform this reset repeatedly once its reward threshold is met. Turn it off in Automation. Higher layers clear lower automation according to this contract.",d,18);
   var actions=HRow(d,"Reset confirmation",0);PcButton("Cancel",actions,ClosePcModal);var confirm=Primary(automatic?"Enable automatic "+ResetRules.Names[layer]:"Confirm "+ResetRules.Names[layer],actions,()=>{ClosePcModal();if(automatic)S.automation.repeatPrestige[layer]=true;else Sim.Rebirth(layer);BuildPc();});confirm.name="Confirm reset "+layer;confirm.interactable=automatic?Sim.AdvancedAutomation&&S.resets[layer]>0:Sim.PrestigeReward(layer)>=1&&S.activeChallenge=="";FocusModalPc();
  }
  void SpecializationsPc(){Caption("SPECIALIZATIONS",list);foreach(var u in GameSession.Content.talents.OrderBy(x=>S.talents.Contains(x.id))){var d=u;var c=PCard(list,u.name);Label(u.name,c,24);Label(u.description,c,18);var b=PcButton(S.talents.Contains(u.id)?"Assigned":u.cost+" Ascension Points",c,()=>{Sim.Talent(d);ShowPc();});PurchaseBarPc(b,()=>Simulation.PurchaseFunding(S.prestige[1].Value,BigDouble.Parse(d.cost)),()=>Sim.HasLayer(1)&&S.prestige[1].Value>=BigDouble.Parse(d.cost)&&(d.requires==""||S.talents.Contains(d.requires)),()=>S.talents.Contains(d.id));}PcButton("Refund specialization points",list,()=>{Sim.Respec();ShowPc();});}

  string MegaBlockPc(int project){
   var m=Sim.Meta.megas[project];if(!Sim.HasLayer(1))return "Requires Ascension";if(m.building)return "Construction in progress";if(m.level>=6)return "All stages complete";if(m.level>=3&&!Sim.HasG("g_horizon"))return "Requires Endless horizon";
   var missing=new List<string>();for(int slot=0;slot<3;slot++){int r=ExpansionContent.MegaResources[project][slot];if(!Simulation.Meets(Sim.MegaAvailable(r),Sim.MegaCost(project,slot)))missing.Add(GameSession.Content.resources[r]);}
   return missing.Count==0?"":"Need unreserved "+string.Join(", ",missing);
  }

  void ObservatoryAutomationPc(){
   if(!Sim.HasLayer(0)){LockedPc("Prestige required","Complete your first Prestige to discover automation.");return;}
   AutobuyersPc();DiscoveryAutomationPc();DevelopmentPlanPc();

   Label("Rules respect reserves. Decisions run every "+Sim.AutomationInterval+" seconds; the next decision is shown below.",list,18,0,CosmicStyle.Muted);
   void Rule(string name,Func<bool> enabled,Action toggle,Func<string> reason,bool unlocked=true){var c=PCard(list,name);Label(name,c,24);var state=Label("",c,18,0,CosmicStyle.Muted);var b=PcButton(enabled()?"Disable":"Enable",c,()=>{toggle();ShowPc();});b.interactable=unlocked||enabled();Bind(()=>state.text=reason());}
   Rule("Automatic purchases",()=>S.automation.buy,()=>S.automation.buy=!S.automation.buy,()=>Sim.ExplainRule("buy").ToString(),Sim.AnyAutobuyer);
   Rule("Guided routing",()=>Sim.Meta.guidedAutomation,()=>Sim.Meta.guidedAutomation=!Sim.Meta.guidedAutomation,()=>Sim.ExplainRule("routing").ToString(),Sim.AnyAutobuyer);
   Rule("Automatic research",()=>Sim.Meta.autoResearch,()=>Sim.Meta.autoResearch=!Sim.Meta.autoResearch,()=>Sim.ExplainRule("all-research").ToString(),Sim.HasA("a_time_2"));
   Rule("Construction planner",()=>Sim.Meta.autoBuild,()=>Sim.Meta.autoBuild=!Sim.Meta.autoBuild,()=>Sim.ExplainRule("build").ToString(),Sim.HasLayer(1));
   Rule("Expedition return and relaunch",()=>S.automation.expeditions,()=>S.automation.expeditions=!S.automation.expeditions,()=>Sim.ExplainRule("expeditions").ToString(),Sim.HasLayer(1));
   OperationsProfilesPc();
   for(int i=0;i<3;i++){int layer=i;var c=PCard(list,"Automatic "+ResetRules.Names[i]);Label("Automatic "+ResetRules.Names[i],c,24);var statusText=Label("",c,18);Bind(()=>statusText.text=S.resets[layer]==0?"LOCKED · Complete this reset manually first.":!S.automation.repeatPrestige[layer]?"OFF · Manual decision required.":layer==0&&Sim.Meta.prestigeEarned.Value>=ProgressionBalance.PrestigeForAscension?"WAITING · Prestige target reached; develop toward Ascension.":"Waiting for reward ≥ "+S.automation.prestigeThreshold+" · Current "+new Number(Sim.PrestigeReward(layer)));var b=PcButton(S.automation.repeatPrestige[i]?"Disable":"Review and enable",c,()=>{if(S.automation.repeatPrestige[layer]){S.automation.repeatPrestige[layer]=false;ShowPc();}else ResetPreviewPc(layer,true);});b.interactable=Sim.AdvancedAutomation&&S.resets[layer]>0;}
   PcButton("Reset threshold: "+S.automation.prestigeThreshold,list,()=>{S.automation.prestigeThreshold=S.automation.prestigeThreshold==2?10:2;ShowPc();});Label("Before Genesis, stop lower resets and finish one stage of each project. Once all trees are complete, stop resetting and complete construction.",list,18,0,CosmicStyle.Warning);
   Caption("RESOURCE RESERVES",list);Label("Presets capture an exact amount now; they do not track future stock. Reserves reduce spendable inputs and can slow production. Construction funding is protected separately.",list,18,0,CosmicStyle.Muted);PcButton("Review releasing all reserves",list,()=>ConfirmPc("Release all manual reserves?","All 12 manual reserve amounts will become zero. Production and automatic purchases may consume the released stock. The construction planner retains its own funding.",()=>{Array.Clear(S.reserves,0,S.reserves.Length);ShowPc();},"Release all reserves"));for(int i=0;i<12;i++){if(!Sim.StageOpen(i/2))continue;int r=i;var c=PCard(list,GameSession.Content.resources[r]+" reserve");var label=Label("",c,18);Bind(()=>label.text=GameSession.Content.resources[r]+" · Protected "+S.reserves[r]);var buttons=HRow(c,"Reserve actions",0);PcButton("Protect 10% of lifetime",buttons,()=>{S.reserves[r]=BigDouble.Max(1,S.lifetime[r].Value*.1);ShowPc();});foreach(double portion in new[]{.25,.5}){double fraction=portion;PcButton("Protect "+(int)(fraction*100)+"% of stock",buttons,()=>{S.reserves[r]=S.resources[r].Value*fraction;ShowPc();});}PcButton("Release reserve",buttons,()=>{S.reserves[r]=BigDouble.Zero;ShowPc();});}
   Caption("PURCHASE PRIORITY & SAVED RULES",list);PcButton("Priority: "+(S.automation.priority[0]==0?"Early first":"Late first"),list,()=>{Array.Reverse(S.automation.priority);ShowPc();});PcButton("Save automation profile",list,()=>{S.automation.reserves=(Number[])S.reserves.Clone();S.profiles.Add(JsonUtility.FromJson<AutomationProfile>(JsonUtility.ToJson(S.automation)));ShowPc();});for(int i=0;i<S.profiles.Count;i++){int j=i;PcButton("Review automation profile "+(j+1),list,()=>ConfirmPc("Apply automation profile?","This replaces purchase priority, reserves, expedition settings and automatic reset rules.\nEnabled resets: "+string.Join(", ",Enumerable.Range(0,3).Where(k=>S.profiles[j].repeatPrestige[k]).Select(k=>ResetRules.Names[k]))+"\n"+string.Join("\n",ResetRules.Reset),()=>{S.automation=JsonUtility.FromJson<AutomationProfile>(JsonUtility.ToJson(S.profiles[j]));S.reserves=(Number[])S.automation.reserves.Clone();ShowPc();},"Apply profile"));}
  }

  void AutobuyersPc(){
   var info=PCard(list,"Prestige autobuyers");Caption("PRESTIGE AUTOMATION",info);Label("Unlock your autobuyers",info,28);
   Label("Each chapter costs 1 Prestige Point and automates its four processes in every universe. Bought unlocks survive Prestige; Ascension and Genesis reset them. Saved profiles cannot unlock an unpurchased buyer.",info,18);
   Label("Spending points lowers their passive production bonus. Buy a package, then keep Automatic purchases enabled. An enabled Development Plan spends points for you and restores buyers after higher resets.",info,18,0,CosmicStyle.Muted);
   PcButton("Learn about autobuyers",info,AutobuyerGuidePc);
   var points=Label("",info,22,0,CosmicStyle.Violet);BindLive(points,()=>points.text=S.prestige[0]+" Prestige Points available");
   foreach(int chapter in Enumerable.Range(0,6).OrderBy(i=>S.autobuyers[i])){int stage=chapter;var card=PCard(list,"Autobuyer "+stage);card.GetComponent<CosmicPanel>().edgeColor=CosmicStyle.Section(stage);
    Label(CosmicStyle.Chapters[stage]+" autobuyer",card,23,0,CosmicStyle.Section(stage));
    Label(string.Join(" / ",GameSession.Content.generators.Where(g=>g.stage==stage).Select(g=>g.name)),card,17,0,CosmicStyle.Muted);
    var buy=Primary(S.autobuyers[stage]?"Unlocked":"Unlock / 1 Prestige Point",card,()=>{if(Sim.UnlockAutobuyer(stage)){S.automation.buy=true;ShowPc();}});buy.name="Unlock autobuyer "+stage;
    PurchaseBarPc(buy,()=>Simulation.PurchaseFunding(S.prestige[0].Value,Sim.AutobuyerCost(stage)),()=>Sim.HasLayer(0)&&S.prestige[0].Value>=Sim.AutobuyerCost(stage),()=>S.autobuyers[stage]);
   }
  }

  RectTransform ObservatoryModalPc(string title,float width,float height){
   var selected=EventSystem.current?EventSystem.current.currentSelectedGameObject:null;ClosePcModal();pcReturnFocus=selected;pcModalBindingStart=pcBindings.Count;pcModalLiveStart=pcLive.Count;
   pcModal=PcBox("Modal backdrop",root,new Color(.015f,.025f,.04f,.96f));Stretch(pcModal,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
   var panel=PcBox("Dialog",pcModal,CosmicStyle.Panel);panel.anchorMin=panel.anchorMax=panel.pivot=new Vector2(.5f,.5f);float margin=PhoneObservatory?8:48;panel.sizeDelta=new Vector2(Mathf.Min(width,root.rect.width-margin),Mathf.Min(height,root.rect.height-margin));panel.anchoredPosition=Vector2.zero;
   var content=ScrollContent(panel,out var scroll,PhoneObservatory?12:24);Caption("CELESTIAL MAKER",content);Label(title,content,28);Hairline(content,CosmicStyle.Edge);RevealPc(panel);return content;
  }
  void FocusModalPc(){Canvas.ForceUpdateCanvases();if(!pcModal||!EventSystem.current)return;var first=pcModal.GetComponentsInChildren<Selectable>().FirstOrDefault(s=>s.IsInteractable()&&!(s is Scrollbar));if(first)EventSystem.current.SetSelectedGameObject(first.gameObject);}
  void KeyboardObservatoryPc(){
   var keyboard=UnityEngine.InputSystem.Keyboard.current;if(keyboard==null||!EventSystem.current)return;
   bool typing=EventSystem.current.currentSelectedGameObject&&EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
   if(!typing&&!menu&&!pcModal){if(keyboard.ctrlKey.isPressed&&keyboard.kKey.wasPressedThisFrame){SearchPalettePc(false);return;}if(keyboard.f1Key.wasPressedThisFrame){ShortcutHelpPc();return;}if(keyboard.altKey.isPressed){var keys=new[]{keyboard.digit1Key,keyboard.digit2Key,keyboard.digit3Key,keyboard.digit4Key,keyboard.digit5Key,keyboard.digit6Key};for(int i=0;i<keys.Length;i++)if(keys[i].wasPressedThisFrame){NavigatePc(SectionEntryPc(i));return;}}}
   if(keyboard.tabKey.wasPressedThisFrame){var parent=pcModal?pcModal:root;var choices=parent.GetComponentsInChildren<Selectable>().Where(s=>s.IsActive()&&s.IsInteractable()&&!(s is Scrollbar)).ToArray();if(choices.Length==0)return;int current=Array.FindIndex(choices,s=>s.gameObject==EventSystem.current.currentSelectedGameObject);int direction=keyboard.shiftKey.isPressed?-1:1;int next=(current+direction+choices.Length)%choices.Length;var target=choices[next];EventSystem.current.SetSelectedGameObject(target.gameObject);var sc=target.GetComponentInParent<ScrollRect>();if(sc&&sc.content){Canvas.ForceUpdateCanvases();var bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(sc.content,target.transform);float overflow=sc.content.rect.height-sc.viewport.rect.height;if(overflow>0)sc.verticalNormalizedPosition=1-Mathf.Clamp01((-bounds.center.y-sc.viewport.rect.height*.5f)/overflow);}}
  }
 }
}
