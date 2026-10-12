using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CelestialMaker;
using BreakInfinity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

public class TouchInputTests {
 Touchscreen touchscreen;
 readonly List<InputDevice> extraDevices=new List<InputDevice>();
 InputSettings.BackgroundBehavior background;
 InputSettings.EditorInputBehaviorInPlayMode editorInput;
 int frameRate;
 [SetUp] public void Setup() {
  SaveStore.OverridePath=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../Logs/AndroidTouch/Saves/"+System.Guid.NewGuid().ToString("N")+".json"));
  background=InputSystem.settings.backgroundBehavior;
  editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
  InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
  InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
  touchscreen=InputSystem.AddDevice<Touchscreen>();
  frameRate=Application.targetFrameRate;Application.targetFrameRate=60;
 }
 [TearDown] public void Cleanup() {
  if(touchscreen!=null)InputSystem.RemoveDevice(touchscreen);
  foreach(var device in extraDevices)InputSystem.RemoveDevice(device);extraDevices.Clear();
  InputSystem.settings.backgroundBehavior=background;
  InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
  if(GameSession.Instance)Object.DestroyImmediate(GameSession.Instance.gameObject);
  GameSession.Instance=null;SaveStore.OverridePath=null;
  Application.targetFrameRate=frameRate;
 }
 static Button Find(string name)=>Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name==name);
 static void BringIntoView(Button button) {
  Canvas.ForceUpdateCanvases();var scroll=button.GetComponentInParent<ScrollRect>();if(!scroll)return;
  var rect=(RectTransform)button.transform;var center=scroll.viewport.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
  scroll.StopMovement();scroll.content.anchoredPosition-=new Vector2(0,center.y-scroll.viewport.rect.center.y);
  Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=Mathf.Clamp01(scroll.verticalNormalizedPosition);Canvas.ForceUpdateCanvases();
 }
 static void PhoneLayout() {
  // The hidden host is unfocused. Keep simulated frame timing representative
  // without focusing or sending input to any desktop window.
  typeof(GameSession).GetField("applicationFocused",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(GameSession.Instance,true);
  GameSession.Instance.CurrentSettings.fps=60;Application.targetFrameRate=60;
  var ui=Object.FindFirstObjectByType<GameUI>();
  typeof(GameUI).GetField("phoneReview",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ui,true);
  typeof(GameUI).GetMethod("Build",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,null);
 }
 IEnumerator Menu() {SceneManager.LoadScene("00_Bootstrap");yield return new WaitForSecondsRealtime(1);PhoneLayout();yield return null;yield return null;}
 IEnumerator Tap(Button button,Vector2 movement=default,float hold=0,System.Action duringPress=null) {
  Canvas.ForceUpdateCanvases();
  var rect=(RectTransform)button.transform;
  var position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
  var module=EventSystem.current.GetComponent<InputSystemUIInputModule>();
  var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=position},hits);
  Debug.Log("TOUCH DIAGNOSTIC "+button.name+" position="+position+" screen="+Screen.width+"x"+Screen.height+" focus="+Application.isFocused+" module="+EventSystem.current.currentInputModule+" point="+module.point.action.enabled+":"+string.Join(",",module.point.action.controls.Select(c=>c.path))+" click="+module.leftClick.action.enabled+":"+string.Join(",",module.leftClick.action.controls.Select(c=>c.path))+" hits="+string.Join(",",hits.Select(h=>h.gameObject.name)));
  Assert.That(hits.Count,Is.GreaterThan(0),"Button must be raycastable");
  Assert.AreEqual(button.gameObject,ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject),"Top raycast must target the requested button");
  string tappedName=button.name;float started=Time.realtimeSinceStartup;
  button.onClick.AddListener(()=>Debug.Log("TOUCH CALLBACK "+tappedName+" offline="+GameSession.Instance?.Sim?.S.offlineRemaining));
  InputSystem.QueueStateEvent(touchscreen,new TouchState{touchId=1,phase=TouchPhase.Began,position=position});
  yield return null;yield return null;
  duringPress?.Invoke();
  if(hold>0)yield return new WaitForSecondsRealtime(hold);
  if(movement!=Vector2.zero)for(int i=0;i<2;i++){position+=movement/2;InputSystem.QueueStateEvent(touchscreen,new TouchState{touchId=1,phase=TouchPhase.Moved,position=position});yield return null;yield return null;}
  InputSystem.QueueStateEvent(touchscreen,new TouchState{touchId=1,phase=TouchPhase.Ended,position=position});
  yield return null;yield return null;
  Debug.Log("TOUCH END "+tappedName+" seconds="+(Time.realtimeSinceStartup-started)+" alive="+(bool)button+" longpress="+(button?button.GetComponent<GameTooltip>()?.LongPressed.ToString():"destroyed"));
 }
 [UnityTest] public IEnumerator UnlockDuringFingerPressDoesNotDestroyMenu() {
  yield return Menu();yield return Tap(Find("New Game"));yield return new WaitForSecondsRealtime(.4f);PhoneLayout();yield return null;yield return null;
  var pressed=Find("Menu");
  yield return Tap(pressed,hold:.30f,duringPress:()=>{GameSession.Instance.Sim.S.resets[0]=1;});
  Assert.IsTrue(pressed,"The pressed navigation control must survive an automatic unlock refresh");
  Assert.IsNotNull(Find("Automation"),"The same tap opens the drawer with the newly unlocked category");
 }
 [UnityTest] public IEnumerator SmallFingerMovementStillClicks() {
  SceneManager.LoadScene("00_Bootstrap");yield return new WaitForSecondsRealtime(1);
  var ui=Object.FindFirstObjectByType<GameUI>();
  typeof(GameUI).GetField("phoneReview",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ui,true);
  typeof(GameUI).GetMethod("Build",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,null);
  yield return null;yield return null;
  yield return Tap(Find("New Game"),new Vector2(0,12));
  yield return new WaitForSecondsRealtime(.2f);
  Assert.AreEqual("02_Game",SceneManager.GetActiveScene().name,"Normal finger jitter must not cancel a tap inside a scroll view");
 }
 [UnityTest] public IEnumerator TouchStartsPhoneGameAndOpensNavigation() {
  SceneManager.LoadScene("00_Bootstrap");yield return new WaitForSecondsRealtime(1);
  Assert.AreEqual("01_MainMenu",SceneManager.GetActiveScene().name);
  var ui=Object.FindFirstObjectByType<GameUI>();
  typeof(GameUI).GetField("phoneReview",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ui,true);
  typeof(GameUI).GetMethod("Build",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,null);
  yield return null;yield return null;
  yield return Tap(Find("New Game"));
  yield return new WaitForSecondsRealtime(.3f);
  Assert.AreEqual("02_Game",SceneManager.GetActiveScene().name,"A real touch must start a game without invoking onClick directly");
  ui=Object.FindFirstObjectByType<GameUI>();
  typeof(GameUI).GetField("phoneReview",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ui,true);
  typeof(GameUI).GetMethod("Build",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,null);
  yield return null;yield return null;
  yield return Tap(Find("Menu"));
  Assert.IsTrue(Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Any(b=>b.name=="Production"),"Touch opens navigation drawer");
  yield return Tap(Find("Production"));
  Assert.IsNull(GameObject.Find("Modal backdrop"),"Navigation closes the modal");
  yield return Tap(Find("Production / Processes · Views"));
  yield return Tap(Find("Research"));
  Assert.IsNotNull(Find("Production / Research · Views"),"Touch selects a research view");
 }
 [UnityTest] public IEnumerator ResourceHeaderOpensByTouch() {
  SceneManager.LoadScene("00_Bootstrap");yield return new WaitForSecondsRealtime(1);
  GameSession.Instance.StartGame(true);yield return new WaitForSecondsRealtime(.4f);
  var ui=Object.FindFirstObjectByType<GameUI>();
  typeof(GameUI).GetField("phoneReview",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(ui,true);
  typeof(GameUI).GetMethod("Build",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,null);
  yield return null;yield return null;
  yield return Tap(Find("Resource meter 0"));
  Assert.IsNotNull(GameObject.Find("Modal backdrop"),"Resource header opens its resource dialog by touch");
  yield return Tap(Find("Close"));
  Assert.IsNull(GameObject.Find("Modal backdrop"));
  yield return Tap(Find("Resource meter 0"));
  Assert.IsNotNull(GameObject.Find("Modal backdrop"),"Resource header responds to a second tap after closing");
 }
 [UnityTest] public IEnumerator SwipeScrollsWithoutStartingGameAndNextTapWorks() {
  yield return Menu();var button=Find("New Game");var scroll=button.GetComponentInParent<ScrollRect>();
  var before=scroll.content.anchoredPosition;
  yield return Tap(button,new Vector2(0,45));
  Assert.AreEqual("01_MainMenu",SceneManager.GetActiveScene().name,"Swiping a button must not activate it");
  Assert.Greater(Vector2.Distance(before,scroll.content.anchoredPosition),1,"A deliberate swipe must move the list");
  scroll.StopMovement();scroll.verticalNormalizedPosition=1;yield return null;yield return null;
  yield return Tap(Find("New Game"));yield return new WaitForSecondsRealtime(.2f);
  Assert.AreEqual("02_Game",SceneManager.GetActiveScene().name,"A swipe must not poison the next tap");
 }
 [UnityTest] public IEnumerator FingerWorksWithMouseAndPenPresent() {
  var pen=InputSystem.AddDevice<Pen>();var mouse=InputSystem.AddDevice<Mouse>();extraDevices.Add(pen);extraDevices.Add(mouse);
  yield return Menu();
  InputSystem.QueueStateEvent(pen,new PenState{position=new Vector2(50,50)});
  InputSystem.QueueStateEvent(mouse,new MouseState{position=new Vector2(75,75)});yield return null;
  yield return Tap(Find("New Game"),new Vector2(0,12));yield return new WaitForSecondsRealtime(.2f);
  Assert.AreEqual("02_Game",SceneManager.GetActiveScene().name,"A stylus or mouse must not steal finger input");
 }
 [UnityTest] public IEnumerator ContinueWorksAfterSceneReload() {
  yield return Menu();yield return Tap(Find("New Game"));yield return new WaitForSecondsRealtime(.3f);
  GameSession.Instance.Sim.S.owned[0]=7;GameSession.Instance.ReturnToMenu();yield return new WaitForSecondsRealtime(.4f);
  PhoneLayout();yield return null;yield return null;
  BringIntoView(Find("Continue"));yield return null;yield return null;
  yield return Tap(Find("Continue"));yield return new WaitForSecondsRealtime(.3f);
  Assert.AreEqual("02_Game",SceneManager.GetActiveScene().name);Assert.AreEqual(7,GameSession.Instance.Sim.S.owned[0]);
 }
 [UnityTest] public IEnumerator RepeatedPurchasesUseTouchAndDisabledPurchaseDoesNotSpend() {
  yield return Menu();yield return Tap(Find("New Game"));yield return new WaitForSecondsRealtime(.4f);PhoneLayout();
  var sim=GameSession.Instance.Sim;sim.S.settings.tutorial=false;sim.S.resources[0]=new BigDouble(10000);sim.S.lifetime[0]=new BigDouble(10000);
  yield return null;yield return null;yield return Tap(Find("Menu"));yield return Tap(Find("Production"));
  for(int i=0;i<3;i++){
   yield return new WaitForSecondsRealtime(.4f);var buy=Find("Buy process 0");BringIntoView(buy);
   long before=sim.S.owned[0];yield return Tap(buy,new Vector2(0,12));Assert.AreEqual(before+1,sim.S.owned[0],"Each finger tap buys exactly one unit");
  }
  sim.S.resources[0]=BigDouble.Zero;sim.S.owned[0]=0;sim.RefreshRates();yield return new WaitForSecondsRealtime(.4f);
  var disabled=Find("Buy process 0");BringIntoView(disabled);yield return null;Assert.IsFalse(disabled.interactable);
  yield return Tap(disabled);Assert.AreEqual(0,sim.S.owned[0],"Disabled purchases ignore touch");
 }
 [UnityTest] public IEnumerator SceneOwnsItsActionsAndResumeRestoresInput() {
  yield return Menu();var menuAsset=EventSystem.current.GetComponent<InputSystemUIInputModule>().actionsAsset;
  yield return Tap(Find("New Game"));yield return new WaitForSecondsRealtime(.4f);PhoneLayout();
  var events=EventSystem.current;var module=events.GetComponent<InputSystemUIInputModule>();
  yield return null;yield return null;
  Assert.AreNotSame(menuAsset,module.actionsAsset,"Scene transitions must not share action ownership");
  module.actionsAsset.Disable();events.GetComponent<ObservatoryInput>().SendMessage("OnApplicationFocus",true);
  Assert.IsTrue(module.leftClick.action.enabled);Assert.IsTrue(module.point.action.enabled);
  yield return Tap(Find("Menu"));yield return Tap(Find("Production"));
  module.actionsAsset.Disable();events.GetComponent<ObservatoryInput>().SendMessage("OnApplicationPause",false);
  Assert.IsTrue(module.leftClick.action.enabled);
  yield return Tap(Find("Menu"));Assert.IsNotNull(Find("Settings"));
 }
 [UnityTest] public IEnumerator OfflineCatchupReleasesButtons() {
  yield return Menu();yield return Tap(Find("New Game"));yield return new WaitForSecondsRealtime(.4f);PhoneLayout();
  var sim=GameSession.Instance.Sim;sim.BeginOffline(120);
  yield return new WaitUntil(()=>sim.S.offlineRemaining==0);yield return new WaitForSecondsRealtime(.4f);
  var resume=Find("Continue experiment");BringIntoView(resume);yield return null;yield return Tap(resume);
  yield return Tap(Find("Menu"));Assert.IsNotNull(Find("Production"),"Menu responds after offline completion");
  yield return Tap(Find("Production"));Assert.IsNotNull(Find("Buy process 0"));
 }
 [UnityTest] public IEnumerator LongPressShowsHintWithoutActionThenNormalTapWorks() {
  yield return Menu();yield return Tap(Find("New Game"),hold:.65f);
  Assert.AreEqual("01_MainMenu",SceneManager.GetActiveScene().name,"A tooltip hold must not start a new campaign");
  yield return Tap(Find("New Game"));yield return new WaitForSecondsRealtime(.2f);
  Assert.AreEqual("02_Game",SceneManager.GetActiveScene().name);
 }
 [TestCase(480,960,0,16)]
 [TestCase(1080,2340,420,36)]
 [TestCase(1440,3120,500,48)]
 [TestCase(3120,1440,500,48)]
 [TestCase(1440,3120,99999,48)]
 public void PhysicalPixelsScaleWithPhoneUi(int width,int height,float dpi,int expected) {
  Assert.AreEqual(expected,ObservatoryTouch.DragThreshold(width,height,dpi));
 }
}
