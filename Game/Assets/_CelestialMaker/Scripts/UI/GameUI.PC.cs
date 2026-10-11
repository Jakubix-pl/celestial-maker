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
  bool PcActive=>!Application.isMobilePlatform&&Application.platform!=RuntimePlatform.WebGLPlayer&&Screen.width>=Screen.height;
  TMP_FontAsset pcDisplay,pcBody;
  RectTransform pcInspector,pcModal,pcToastPanel,pcNav;
  ScrollRect pcScroll;
  readonly List<Action> pcBindings=new List<Action>(),pcShellBindings=new List<Action>();
  readonly Dictionary<string,Button> pcTabs=new Dictionary<string,Button>();
  RectTransform pcGridRow;int pcGridIndex,pcColumns=2;
  int pcStage=-1,pcAchievements=-1;float pcTick;bool pcRefreshing;
  string pcToastText="";float pcToastUntil;TMP_Text pcToast;
  int pcProductionStage=-1;string pcLastPage="";
  static readonly string[] PcPages={"Overview","Production","Research","Prestige","Constellations","Challenges","Expeditions","Achievements","Codex","Settings","Offline summary","Analytics","Automation","Roadmap","Megastructures","Reality laws","Parallel universes"};
  static readonly string[] PcDescriptions={"Your unfolding cosmos","Build the cosmic engine","Discover new possibilities","Trade a cosmos for lasting power","Connect the branches of creation","Rewrite the rules of an experiment","Send a signal into the unknown","A record of extraordinary beginnings","Stories written in starlight","Make yourself at home","Your universe kept moving","Understand every resource chain","Let your cosmos work for you","The next attainable horizon","Construct cosmic capabilities","Choose the rules of creation","Specialize connected economies"};

  RectTransform PcBox(string name,Transform parent,Color color,float height=0){
   var prefab=Resources.Load<GameObject>("UI/PC/Panel");var go=prefab?Instantiate(prefab):new GameObject(name,typeof(RectTransform),typeof(CosmicPanel));go.name=name;go.transform.SetParent(parent,false);
   var img=go.GetComponent<CosmicPanel>();img.color=color;img.raycastTarget=color.a>.01f;img.edge=color.a<.01f?0:1;img.edgeColor=CosmicStyle.Edge;img.cut=6;
   var r=(RectTransform)go.transform;if(height>0)Size(r,height);return r;
  }
  static LayoutElement Size(RectTransform r,float height,float flex=0){var e=r.GetComponent<LayoutElement>()??r.gameObject.AddComponent<LayoutElement>();e.minHeight=height;e.flexibleHeight=flex;return e;}
  TMP_Text PcText(string text,Transform parent,int size=18,float height=0){
   var go=new GameObject("Text",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);var t=go.GetComponent<TextMeshProUGUI>();t.font=size>=24&&pcDisplay?pcDisplay:pcBody?pcBody:font;t.text=text;t.fontSize=Mathf.Max(18,size)*PcScale;t.color=CosmicStyle.Text;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;t.richText=true;t.overflowMode=TextOverflowModes.Overflow;
   if(height>0)Size((RectTransform)t.transform,height*PcScale);return t;
  }
  TMP_Text Label(string text,Transform parent,int size=18,float height=0,Color? color=null){var t=PcText(text,parent,size,height);if(color.HasValue)t.color=color.Value;return t;}
  TMP_Text Caption(string text,Transform parent,float height=22){var t=Label(text.ToUpperInvariant(),parent,14,height,CosmicStyle.Muted);t.fontSize=14*PcScale;t.characterSpacing=1;return t;}
  RectTransform HRow(Transform parent,string name,float h,float gap=12){var r=PcBox(name,parent,Color.clear,h*PcScale);var l=r.gameObject.AddComponent<ObservatoryFlow>();l.cellWidth=190*PcScale;l.gap=gap;return r;}
  RectTransform PCard(Transform parent,string name,float h=0){var c=PcBox(name,parent,CosmicStyle.Panel,h);Vertical(c,20,8);return c;}
  void Hairline(Transform parent,Color color){var r=PcBox("Accent rule",parent,color,2);r.GetComponent<CosmicPanel>().edge=0;}
  Button PcButton(string label,Transform parent,Action action,float height=42){
   var r=PcBox(label,parent,CosmicStyle.Raised,Mathf.Max(42,height));var b=r.gameObject.AddComponent<ObservatoryButton>();b.targetGraphic=r.GetComponent<Image>();
   var t=PcText(label,r,18);t.name="Label";t.alignment=TextAlignmentOptions.MidlineLeft;t.enableAutoSizing=false;
   var fit=r.gameObject.AddComponent<ObservatoryButtonSize>();fit.label=t;fit.baseline=Mathf.Max(44,height)*PcScale;
   Stretch((RectTransform)t.transform,Vector2.zero,Vector2.one,new Vector2(16,7),new Vector2(-16,-7));
   b.Configure(t);if(SectionPc(page)>=0)b.Accent(CosmicStyle.Section(SectionPc(page)));
   var hint=r.gameObject.AddComponent<GameTooltip>();hint.font=pcBody?pcBody:font;hint.message=label;hint.cosmic=true;
   b.onClick.AddListener(()=>{if(pcModal&&!b.transform.IsChildOf(pcModal))return;if(S!=null&&S.offlineRemaining>0){GameSession.Instance.Message="Offline progress is being calculated. Controls resume when it completes.";return;}if(hint.LongPressed){hint.LongPressed=false;return;}clickAudio?.PlayOneShot(clickClip,GameSession.Instance.CurrentSettings.sound);action();if(Application.isPlaying)GameSession.Instance?.Save();});return b;
  }
  Button Primary(string label,Transform parent,Action action,float height=48){var b=PcButton(label,parent,action,height);((ObservatoryButton)b).Configure(b.GetComponentInChildren<TMP_Text>(),true);return b;}
  void EmphasizePc(Button button,bool selected=true)=>((ObservatoryButton)button).Emphasize(selected);
  void Hint(Button b,string text){b.GetComponent<GameTooltip>().message=text;}
  void Bind(Action action){pcBindings.Add(action);action();}
  void Gate(Button b,Func<bool> predicate){Bind(()=>{if(b)b.interactable=predicate();});}
  void Art(Transform parent,float alpha){var tex=Resources.Load<Texture2D>("UI/PC/CosmicBackdrop");if(!tex)return;var go=new GameObject("Cosmic illustration",typeof(RectTransform),typeof(RawImage));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;Stretch(r,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var raw=go.GetComponent<RawImage>();raw.texture=tex;raw.color=new Color(1,1,1,alpha);raw.raycastTarget=false;go.AddComponent<CosmicArtwork>();var e=go.AddComponent<LayoutElement>();e.ignoreLayout=true;}
  RectTransform ScrollContent(RectTransform host,out ScrollRect scroll,int padding=20){
   scroll=host.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.scrollSensitivity=46;scroll.movementType=ScrollRect.MovementType.Clamped;
   var viewport=PcBox("Viewport",host,Color.clear);Stretch(viewport,Vector2.zero,Vector2.one,new Vector2(0,0),new Vector2(-14,0));viewport.gameObject.AddComponent<RectMask2D>();viewport.GetComponent<Image>().raycastTarget=true;
   var content=PcBox("Content",viewport,Color.clear);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.offsetMin=content.offsetMax=Vector2.zero;Vertical(content,padding,14);content.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.viewport=viewport;scroll.content=content;
   var track=PcBox("Scroll track",host,CosmicStyle.Background);Stretch(track,new Vector2(1,0),Vector2.one,new Vector2(-10,8),new Vector2(-3,-8));var handle=PcBox("Scroll thumb",track,CosmicStyle.Edge);Stretch(handle,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);var bar=track.gameObject.AddComponent<Scrollbar>();bar.handleRect=handle;bar.targetGraphic=handle.GetComponent<Image>();bar.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=bar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;return content;
  }
  void BuildPc()=>BuildObservatoryPc();
  void BuildMenuPc()=>ObservatoryMenuPc();
  void ShowMenuPc(){
   ClosePcModal();Clear(list);pcBindings.Clear();pcLive.Clear();page="Start";pcLastPage="Start";Caption("FROM A SINGLE PARTICLE",list);var title=Label("CELESTIAL\nMAKER",list,48);title.font=pcDisplay;title.characterSpacing=3;Hairline(list,CosmicStyle.Gold);Label("Make room for a universe.",list,29,45);Label("A patient cosmos. An unfolding experiment.\nBuild the connections that turn a spark into infinity.",list,18,65,CosmicStyle.Muted);
   var saved=GameSession.Instance.Saved;
   if(saved!=null){var c=PCard(list,"Continue presentation");Caption("YOUR EXPERIMENT IS WAITING",c);Label("Time in cosmos  "+OfflineReport.Duration(saved.time),c,17,26);Primary("Continue",list,()=>GameSession.Instance.StartGame(false),55);}
   var fresh=saved==null?Primary("New Game",list,()=>GameSession.Instance.StartGame(true),55):PcButton("New Game",list,()=>ConfirmPc("Start a new cosmos?","A local restore point of your current experiment will be created before a fresh universe begins.\n\nYou can recover that progress from Settings / Restore points.",()=>GameSession.Instance.ConfirmNew(),"Begin new cosmos"),48);
   var row=HRow(list,"Menu options",44);PcButton("Settings",row,()=>{page="Settings";ShowPc();});PcButton("Credits",row,()=>{page="Credits";ShowPc();});Label("No account is required to play.",list,18,25,CosmicStyle.Muted);
  }
  void ShowPc()=>ShowObservatoryPc();
  RectTransform GridCard(string name,float height){
   if(pcGridIndex%pcColumns==0){pcGridRow=HRow(list,name+" row",0,14);pcGridRow.GetComponent<ObservatoryFlow>().cellWidth=Mathf.Max(250*PcScale,(list.rect.width-50)/pcColumns-14);}pcGridIndex++;
   var c=PCard(pcGridRow,name,height);var e=c.GetComponent<LayoutElement>();e.minWidth=e.preferredWidth=0;e.flexibleWidth=1;return c;
  }
  void FinishGrid(){if(pcGridRow&&pcGridIndex%pcColumns!=0)for(int i=pcGridIndex%pcColumns;i<pcColumns;i++){var spacer=PcBox("Grid breathing room",pcGridRow,Color.clear);var e=spacer.gameObject.AddComponent<LayoutElement>();e.flexibleWidth=1;e.minWidth=0;}pcGridIndex=0;pcGridRow=null;}
  void OverviewPc()=>ObservatoryOverviewPc();
  long PcBuyCount(int id)=>quantity==0?Sim.MaxBuy(id):quantity==-1?Sim.NextMilestone(id):quantity;
  void ProductionPc()=>ObservatoryProductionPc();
  void UpgradesPc(Upgrade[] defs){
   bool research=page=="Research";if(research)ResearchFiltersPc(defs);pcGridIndex=0;foreach(var u in defs){if(!Sim.StageOpen(u.stage)||research&&!ResearchMatches(u))continue;var d=u;var c=GridCard(u.name,310);Caption(CosmicStyle.Chapters[u.stage]+" / "+(u.target<0?"GLOBAL":"PROCESS"),c).color=CosmicStyle.Accents[u.stage];Label(u.name,c,25,57);Label(u.description+"\n"+(u.target>=0?GameSession.Content.generators[u.target].name:"All processes")+" ×"+u.effect,c,17,70,CosmicStyle.Muted);var note=Label("",c,15,27,CosmicStyle.Cyan);var button=PcButton("Research",c,()=>{if(Sim.Purchase(d)){ToastPc("Discovery complete / "+d.name);ShowPc();}},44);Bind(()=>{bool owned=S.purchases.Contains(d.id),locked=d.requires!=""&&!S.purchases.Contains(d.requires),affordable=S.resources[d.costResource].Value>=BigDouble.Parse(d.cost);note.text=owned?"ACQUIRED / permanent this run":locked?"Requires "+Sim.FindUpgrade(d.requires).name:"Cost "+new Number(BigDouble.Parse(d.cost))+" "+GameSession.Content.resources[d.costResource];button.GetComponentInChildren<TMP_Text>().text=owned?"Purchased":locked?"Prerequisite required":affordable?"Unlock discovery":"Gather resources";button.interactable=!owned&&!locked&&affordable;Hint(button,note.text+". "+d.description);});}FinishGrid();
  }
  void PrestigePc()=>ObservatoryResetsPc();
  void ConstellationsPc(){
   if(!Sim.HasLayer(0)){LockedPc("Prestige","Complete your first Prestige to connect cosmic branches.");return;}
   Label("Redirect 15% of one branch's output for a 40% gain in another. Links never multiply themselves recursively.",list,18,65,CosmicStyle.Muted);var row=HRow(list,"Branch connections",70);for(int i=0;i<6;i++){int stage=i;PcButton(CosmicStyle.Chapters[i]+"\n"+(S.links[i]<0?"Unlinked":"To "+CosmicStyle.Chapters[S.links[i]]),row,()=>{int target=S.links[stage]+1;if(target==stage)target++;if(target>=6)target=-1;if(!Sim.Link(stage,target))ToastPc("No free link slots. Reset progress unlocks more slots.");ShowPc();},70);}
   if(ResetCurrencyVisible(1)){Caption("SPECIALIZATIONS / ASCENSION",list);foreach(var u in GameSession.Content.talents){var d=u;var c=GridCard(u.name,216);Label(u.name,c,25,52);Label(u.description,c,16,50,CosmicStyle.Muted);var b=PcButton(S.talents.Contains(u.id)?"Assigned":u.cost+" Ascension Points",c,()=>{Sim.Talent(d);ShowPc();});Gate(b,()=>Sim.HasLayer(1)&&!S.talents.Contains(d.id)&&S.prestige[1].Value>=BigDouble.Parse(d.cost)&&(d.requires==""||S.talents.Contains(d.requires)));}FinishGrid();PcButton("Free respec — refund assigned Ascension Points",list,()=>{Sim.Respec();ShowPc();});}else Label("Complete Ascension to unlock specializations.",list,18);Caption("REALITY LAWS",list);if(Sim.HasLayer(1))PcButton("Choose reality laws",list,()=>{page="Reality laws";ShowPc();});else Label("Ascension unlocks nine laws with tradeoffs and a switch cooldown.",list,17,60,CosmicStyle.Muted);
  }
  void LockedPc(string title,string description){var c=PCard(list,"Locked system",220);Caption("A HORIZON STILL TO COME",c);Label(title,c,34,48);Label(description,c,19,65,CosmicStyle.Muted);PcButton("Explore Prestige",c,()=>{page="Prestige";ShowPc();});}
  void ChallengesPc(){
   if(Sim.Discover.challengeReview!="")Label(Sim.Discover.challengeReview,list,18);
   if(!Sim.HasLayer(0)){LockedPc("Prestige required","Your first rebirth unlocks alternate experiments and their lasting rewards.");return;}
   if(S.activeChallenge!=""){var active=PCard(list,"Active challenge");Caption("EXPERIMENT IN PROGRESS",active);Label(S.activeChallenge+" / Independent experiment",active,19,32);var row=HRow(active,"Challenge controls",44);PcButton("Exit and restore economy",row,()=>ConfirmPc("Leave this challenge?","Your previous economy will be restored.",()=>{Sim.ExitChallenge();ShowPc();},"Restore economy"));PcButton("Restart challenge",row,()=>ConfirmPc("Restart this challenge?","Your current challenge progress will reset. Your previous economy remains stored.",()=>{int i=Array.FindIndex(GameSession.Content.challenges,x=>x.id==S.activeChallenge);Sim.ExitChallenge();Sim.EnterChallenge(i);ShowPc();},"Restart experiment"));}
   for(int i=0;i<12;i++){int id=i;var d=GameSession.Content.challenges[i];var c=GridCard(d.name,304);Caption(S.completedChallenges.Contains(d.id)?"COMPLETED":"EXPERIMENT "+(i+1).ToString("00"),c);Label(d.name,c,26,51);Label(d.description+"\n"+DiscoveryContent.Rules[1+i%3],c,17,0,CosmicStyle.Muted);var trial=PcButton("Run readiness trial",c,()=>ChallengeTrialPc(id));trial.interactable=Sim.HasLayer(0)&&Sim.StageOpen(d.stage)&&S.activeChallenge=="";Label("Goal "+d.target+" "+GameSession.Content.resources[d.resource],c,16,28,CosmicStyle.Cyan);var b=PcButton("Enter challenge",c,()=>ConfirmPc(d.name,"Your current economy is stored and restored when you leave.\n\n"+d.description+"\n\nGoal: "+d.target+" "+GameSession.Content.resources[d.resource]+". Your original economy is restored on exit.",()=>{Sim.EnterChallenge(id);ShowPc();},"Begin challenge"));b.interactable=Sim.StageOpen(d.stage)&&S.activeChallenge=="";Hint(b,Sim.StageOpen(d.stage)?"Start a separate challenge experiment.":"Unlock "+CosmicStyle.Chapters[d.stage]+" first.");}FinishGrid();
  }
  void ExpeditionsPc(){
   ExpeditionDiscoveriesPc();
   if(!Sim.HasLayer(1)){LockedPc("Ascension required","Ascend to send expeditions into the unexplored cosmos.");return;}
   PcButton("Automatic return: "+S.automation.expeditions,list,()=>{S.automation.expeditions=!S.automation.expeditions;ShowPc();});for(int i=0;i<8;i++){int id=i;var d=GameSession.Content.expeditions[i];var c=GridCard(d.name,270);Caption("DEEP SPACE / EXPEDITION "+(i+1).ToString("00"),c);Label(d.name,c,27,48);var info=Label("",c,17,76,CosmicStyle.Muted);var b=PcButton("Launch expedition",c,()=>{var voyage=S.voyages.Find(x=>x.definition==id&&!x.claimed);if(voyage==null)Sim.StartVoyage(id);else if(Sim.Claim(voyage))ToastPc("Expedition reward collected / "+d.name);ShowPc();});Bind(()=>{var v=S.voyages.Find(x=>x.definition==id&&!x.claimed);var cost=BigDouble.Parse(d.cost)*BigDouble.Pow(2,S.expeditionLevels[id]);info.text="Level "+S.expeditionLevels[id]+" / Guaranteed "+new Number(BigDouble.Parse(d.reward)*BigDouble.Pow(2,S.expeditionLevels[id]))+" "+GameSession.Content.resources[d.resource]+"\n"+(v==null?"Cost "+new Number(cost)+" Energy / "+d.duration+"s":v.finish>S.time?"Returns in "+Math.Ceiling(v.finish-S.time)+"s":"Signal received. Reward ready.");b.GetComponentInChildren<TMP_Text>().text=v==null?"Launch expedition":v.finish>S.time?"In flight":"Claim reward";b.interactable=Sim.StageOpen(d.stage)&&(v==null?S.resources[0].Value>=cost:v.finish<=S.time);});}FinishGrid();
  }
  void AchievementsPc(){
   Label(S.achievements.Count+" / 48 discoveries  /  +1% production each",list,22,40,CosmicStyle.Gold);foreach(var a in GameSession.Content.achievements){var c=GridCard(a.name,166);bool owned=S.achievements.Contains(a.id);Caption(owned?"DISCOVERED":"UNDISCOVERED",c).color=owned?CosmicStyle.Cyan:CosmicStyle.Muted;Label(a.name,c,25,38);Label("Produce "+a.target+" "+GameSession.Content.resources[a.resource],c,16,35,CosmicStyle.Muted);}FinishGrid();
  }
  void CodexPc(){
   PcButton("Cosmic chronicle",list,ChroniclePc);
   int count=0;foreach(var s in GameSession.Content.stories)if(S.discoveredStories.Contains(s.id)){count++;var c=PCard(list,s.name);Caption("ARCHIVE / CHAPTER "+count.ToString("00"),c);Label(s.name,c,30,45);Label(s.text,c,19);}
   if(count==0){var c=PCard(list,"Unwritten stories",200);Label("The first page is still unwritten.",c,30,55);Label("Keep producing. New chapters appear as your cosmos grows, and never interrupt production.",c,19,70,CosmicStyle.Muted);}
  }
  void SettingsPc(){
   var s=GameSession.Instance.CurrentSettings;Label("Language: English",list,18,30,CosmicStyle.Muted);Label("Native-resolution UI · high-resolution text and artwork\nSmooth counters · display limit: 60 or 120 FPS",list,18,0,CosmicStyle.Muted);PcButton("Interface: "+(s.lightUI?"Dawn":"Night"),list,()=>{s.lightUI=!s.lightUI;BuildPc();});Caption("DISPLAY & COMFORT",list);var display=PCard(list,"Display preferences");var row=HRow(display,"Display choices",52);PcButton("Frame limit: "+Math.Max(60,s.fps),row,()=>{s.fps=s.fps==60?120:60;ShowPc();});PcButton("Quality: "+(s.quality==0?"Low":"Desktop"),row,()=>{s.quality=1-s.quality;QualitySettings.SetQualityLevel(s.quality);ShowPc();});row=HRow(display,"Comfort choices",52);PcButton("Reduced motion: "+(s.reducedMotion?"On":"Off"),row,()=>{s.reducedMotion=!s.reducedMotion;ShowPc();});PcButton("Text scale: "+Math.Round(s.textScale*100)+"%",row,()=>{s.textScale=NextTextScalePc(s.textScale);BuildPc();});row=HRow(display,"Notation choices",52);PcButton("Notation: "+s.notation,row,()=>{s.notation=s.notation=="Scientific"?"Engineering":"Scientific";Number.DisplayNotation=s.notation;ShowPc();});PcButton("Confirm prestige: "+s.confirmReset,row,()=>{s.confirmReset=!s.confirmReset;ShowPc();});Label("Manual resets always show their consequences before you proceed.",display,15,25,CosmicStyle.Muted);
   PcButton("Ready notifications: "+(s.observatory.readyNotifications?"On":"Off"),list,()=>{s.observatory.readyNotifications=!s.observatory.readyNotifications;ShowPc();});PcButton("Keyboard shortcuts",list,ShortcutHelpPc);
   Caption("SOUND & ATMOSPHERE",list);var audio=PCard(list,"Sound preferences");row=HRow(audio,"Audio choices",52);PcButton("Music: "+(s.music>0?"On":"Off"),row,()=>{s.music=s.music>0?0:.25f;ShowPc();});PcButton("UI sound: "+(s.sound>0?"On":"Off"),row,()=>{s.sound=s.sound>0?0:.5f;ShowPc();});PcButton("Cosmic theme: "+new[]{"Origin","Nebula","Cinders"}[Math.Min(2,Math.Max(0,s.theme))],audio,()=>{if(Sim!=null&&Sim.CosmeticsUnlocked){for(int offset=1;offset<=3;offset++){int theme=(s.theme+offset)%3;if(Sim.ThemeUnlocked(theme)){s.theme=theme;break;}}ShowPc();}else ToastPc("Cosmic themes unlock at 8 and 24 discoveries.");});
   Caption("SAVE & PROGRESS",list);var save=PCard(list,"Save management");Label("Your experiment is stored locally. Keep an exported copy for safekeeping.",save,18,50,CosmicStyle.Muted);row=HRow(save,"Save actions",48);PcButton("Export save",row,()=>{GameSession.Instance.Export();ToastPc(GameSession.Instance.Message);});PcButton("Import save",row,ImportPc);PcButton("Restore points",save,CheckpointsPc);if(!menu)PcButton("Save and return to menu",list,()=>GameSession.Instance.ReturnToMenu(),48);
  }
  void ImportPc(){
   var modal=ModalPc("Bring your cosmos home",760,580);Label("Paste an exported JSON save. It will be validated before it replaces the active experiment.",modal,18,70,CosmicStyle.Muted);
   var field=PcBox("Import field",modal,CosmicStyle.Background,240);field.gameObject.AddComponent<RectMask2D>();var input=field.gameObject.AddComponent<TMP_InputField>();var text=(TextMeshProUGUI)PcText("",field,16);Stretch((RectTransform)text.transform,Vector2.zero,Vector2.one,new Vector2(16,14),new Vector2(-16,-14));var placeholder=PcText("Paste an exported JSON save…",field,16);placeholder.color=CosmicStyle.Muted;Stretch((RectTransform)placeholder.transform,Vector2.zero,Vector2.one,new Vector2(16,14),new Vector2(-16,-14));input.textComponent=text;input.textViewport=field;input.placeholder=placeholder;input.lineType=TMP_InputField.LineType.MultiLineNewline;input.characterLimit=2*1024*1024;input.customCaretColor=true;input.caretColor=CosmicStyle.Cyan;var message=Label("",modal,16,44,CosmicStyle.Danger);
   var row=HRow(modal,"Import actions",48);Primary("Import validated save",row,()=>{try{GameSession.Instance.Import(input.text);ClosePcModal();}catch(Exception e){message.text="Import rejected: "+e.Message;}});PcButton("Cancel",row,ClosePcModal);
  }
  RectTransform ModalPc(string title,float width=780,float height=540)=>ObservatoryModalPc(title,width,height);
  void ClosePcModal(){pcLabJob?.Cancel();pcForecastGeneration++;pcLabRunning=false;if(pcModal){if(pcBindings.Count>pcModalBindingStart)pcBindings.RemoveRange(pcModalBindingStart,pcBindings.Count-pcModalBindingStart);pcModal.gameObject.SetActive(false);if(Application.isPlaying)Destroy(pcModal.gameObject);else DestroyImmediate(pcModal.gameObject);}pcModal=null;offlineDialog=null;if(pcReturnFocus&&EventSystem.current)EventSystem.current.SetSelectedGameObject(pcReturnFocus);pcReturnFocus=null;}
  void ConfirmPc(string title,string text,Action action,string confirm="Confirm"){
   var dialog=ModalPc(title,880,720);Label(text,dialog,18);var row=HRow(dialog,"Confirmation actions",48);PcButton("Cancel",row,ClosePcModal);Primary(confirm,row,()=>{ClosePcModal();action();});FocusModalPc();
  }
  void OfflinePc(Transform parent,bool details){
   var report=S.offlineReport;if(report==null){Label("No absence has been processed yet.",parent,20,45,CosmicStyle.Muted);return;}
   var summary=PCard(parent,"Absence summary");Caption(report.active?"CATCHING UP":"WELCOME BACK",summary);Label(OfflineReport.Duration(report.processedSeconds)+" simulated",summary,32,45);if(report.fixedTicks>0)Label(report.fixedTicks.ToString("N0",System.Globalization.CultureInfo.InvariantCulture)+" full ticks replayed at 60 Hz",summary,17,0,CosmicStyle.Muted);Label("Net change includes production, spending and resets.",summary,17,34,CosmicStyle.Muted);
   for(int i=0;i<15;i++){if(!report.knownBefore[i]&&!report.knownAfter[i])continue;int id=i;var delta=report.after[i].Value-report.before[i].Value;if(!details&&delta==0&&report.produced[i].Value==0&&report.spent[i].Value==0&&report.resetLoss[i].Value==0&&report.granted[i].Value==0)continue;var c=PCard(parent,"Offline resource "+i);Caption((i<12?GameSession.Content.resources[i]:ResetRules.Currencies[i-12])+(!report.knownBefore[i]?" / DISCOVERED":""),c);Label((delta>0?"+":"")+new Number(delta)+" net",c,29,38,delta<0?CosmicStyle.Danger:CosmicStyle.Cyan);Label("Before "+report.before[i]+"    /    After "+report.after[i],c,17,30,CosmicStyle.Muted);if(details)Label("Produced "+report.produced[i]+"  •  Spent "+report.spent[i]+"\nReset loss "+report.resetLoss[i]+"  •  Rewards "+report.granted[i],c,16,55,CosmicStyle.Muted);else if(delta<0)Label("Decrease reflects spending or a reset.",c,15,24,CosmicStyle.Muted);}
   var events=PCard(parent,"Offline events");Caption("DURING YOUR ABSENCE",events);Label("Automatic purchases: "+report.automaticUnits.Sum()+" units  /  Resets: "+report.resets.Sum()+"\nDiscoveries: "+report.discoveries.Count+"  /  Stories: "+report.stories.Count+"  /  Challenges: "+report.challenges.Count+"\nExpeditions completed: "+report.finishedExpeditions.Sum()+"  /  Claimed: "+report.claimedExpeditions.Sum(),events,17,84);
   for(int i=0;i<6;i++)if(report.stagesAfter[i]&&!report.stagesBefore[i])Label("UNLOCKED / "+CosmicStyle.Chapters[i],events,20,32,CosmicStyle.Gold);
   if(report.boostSeconds>0)Label("Legacy recorded interval: "+OfflineReport.Duration(report.boostSeconds)+"; historical only, with no current production effect.",events,16,50);
   if(report.legacyRemainder)Label("An older pending calculation resumed; its earlier resource history was unavailable.",events,16,50);
   if(details){for(int i=0;i<24;i++)if(report.automaticUnits[i]>0)Label("Autobuy / "+GameSession.Content.generators[i].name+" ×"+report.automaticUnits[i],events,16,28);for(int i=0;i<3;i++)if(report.resets[i]>0)Label(ResetRules.Names[i]+" ×"+report.resets[i],events,16,28);}
  }
  void ShowOfflinePc(){
   var dialog=ModalPc("Your cosmos kept moving",920,740);offlineDialog=pcModal;var actions=HRow(dialog,"Offline actions",48);PcButton("Expand details",actions,()=>{S.offlineReport.dismissed=true;ClosePcModal();page="Offline summary";offlineDetails=true;ShowPc();});Primary("Continue experiment",actions,()=>{S.offlineReport.dismissed=true;ClosePcModal();});ReturnBriefPc(dialog);OfflinePc(dialog,false);FocusModalPc();
  }
  void RefreshInspectorPc()=>ObservatoryInspectorPc();
  void ToastPc(string text){pcToastText=text;pcToastUntil=Time.unscaledTime+4;if(pcToast){pcToast.text=text;pcToastPanel.gameObject.SetActive(true);RevealPc(pcToastPanel);}}
  void RefreshPc(){if(S!=null){Sim.RefreshRates();if(pcWorld>0){pcEconomy=Sim.Universe(pcWorld);pcEconomyHome=Sim;pcEconomyWorld=pcWorld;if(LocalPagePc)EconomyPc.RefreshRates();}}RefreshBindings(pcShellBindings);RefreshBindings(pcBindings);RefreshBindings(pcInspectorBindings);PresentPc();}
  void UpdatePc(){
   KeyboardObservatoryPc();
   ReadabilitySearchTickPc();
   if(back!=null&&back.WasPressedThisFrame()){if(pcModal){if(offlineDialog&&S?.offlineReport!=null){S.offlineReport.dismissed=true;GameSession.Instance.Save();}ClosePcModal();}else if(menu)ShowMenuPc();else{page="Overview";ShowPc();}}
   if(Screen.width!=previousWidth||Screen.height!=previousHeight){Build();return;}
   if(pcToastPanel&&pcToastPanel.gameObject.activeSelf&&Time.unscaledTime>pcToastUntil)pcToastPanel.gameObject.SetActive(false);
   PresentPc();
   if(Time.unscaledTime<pcTick)return;pcTick=Time.unscaledTime+.25f;var gs=GameSession.Instance;if(gs==null)return;
   if(status){status.text=gs.SaveBusy?"Saving...":gs.SaveFailed?"Save failed / open Settings to retry":gs.LastSavedUtc>0?"Saved locally / "+DateTimeOffset.FromUnixTimeSeconds((long)gs.LastSavedUtc).ToLocalTime().ToString("HH:mm:ss",System.Globalization.CultureInfo.InvariantCulture):"Local save ready";if(S!=null&&S.offlineRemaining>0){var r=S.offlineReport;status.text="Catching up "+(r==null?"":(100*r.processedSeconds/Math.Max(1,r.requestedSeconds)).ToString("0.0")+"% / "+r.fixedTicks.ToString("N0")+" ticks / ")+OfflineReport.Duration(S.offlineRemaining)+" remaining";}}
   if(menu)return;if(S==null)return;int unlockStamp=(Sim.HasLayer(1)?1:0)+(Sim.UniverseCapacity>1?2:0);if(pcExpansionStamp!=unlockStamp&&!pcModal){pcExpansionStamp=unlockStamp;BuildPc();return;}
   if(S.offlineReport!=null&&!S.offlineReport.active&&!S.offlineReport.dismissed&&S.offlineReport.processedSeconds>=60&&S.offlineReport.id!=lastOfflineShown&&!pcModal){lastOfflineShown=S.offlineReport.id;ShowOfflinePc();}
   int stage=HighestStage();if(pcStage!=stage&&!pcModal){pcStage=stage;if(page!="Settings")ShowPc();}
   if(pcAchievements>=0&&S.achievements.Count>pcAchievements){ToastPc("Discovery unlocked / "+S.achievements.Count+" of 48");if(page=="Achievements")ShowPc();}pcAchievements=S.achievements.Count;
   CheckPinnedGoalPc();RefreshPc();
  }
 }
}
