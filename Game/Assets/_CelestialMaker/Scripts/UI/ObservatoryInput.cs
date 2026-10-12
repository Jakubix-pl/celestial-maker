using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace CelestialMaker {
 // Own the UI actions for this scene. The module's AssignDefaultActions helper
 // uses a static shared asset; scene teardown must not own the next scene's input.
 [DisallowMultipleComponent]
 [RequireComponent(typeof(EventSystem),typeof(InputSystemUIInputModule))]
 public sealed class ObservatoryInput:MonoBehaviour {
  DefaultInputActions actions;
  InputSystemUIInputModule module;
  readonly List<InputActionReference> references=new List<InputActionReference>();
  public static bool PointerGestureActive {
   get {
    bool Busy(UnityEngine.InputSystem.Controls.ButtonControl press)=>press!=null&&(press.isPressed||press.wasPressedThisFrame||press.wasReleasedThisFrame);
    if(Busy(Mouse.current?.leftButton)||Busy(Pen.current?.tip))return true;
    if(Touchscreen.current!=null)foreach(var touch in Touchscreen.current.touches)if(Busy(touch.press))return true;
    return false;
   }
  }
  public static void Ensure(EventSystem events) {
   if(events&&!events.GetComponent<ObservatoryInput>())events.gameObject.AddComponent<ObservatoryInput>();
  }
  InputActionReference Reference(InputAction action) {
   var reference=InputActionReference.Create(action);references.Add(reference);return reference;
  }
  void Awake() {
   module=GetComponent<InputSystemUIInputModule>();module.enabled=false;
   actions=new DefaultInputActions();module.actionsAsset=actions.asset;
   module.point=Reference(actions.UI.Point);module.leftClick=Reference(actions.UI.Click);
   module.rightClick=Reference(actions.UI.RightClick);module.middleClick=Reference(actions.UI.MiddleClick);
   module.scrollWheel=Reference(actions.UI.ScrollWheel);module.move=Reference(actions.UI.Navigate);
   module.submit=Reference(actions.UI.Submit);module.cancel=Reference(actions.UI.Cancel);
   module.trackedDevicePosition=Reference(actions.UI.TrackedDevicePosition);
   module.trackedDeviceOrientation=Reference(actions.UI.TrackedDeviceOrientation);
   module.enabled=true;
  }
  void RestoreInput() {
   if(isActiveAndEnabled&&module&&module.isActiveAndEnabled&&actions!=null)actions.UI.Enable();
  }
  void OnApplicationFocus(bool focused){if(focused)RestoreInput();}
  void OnApplicationPause(bool paused){if(!paused)RestoreInput();}
  void OnDestroy() {
   if(module)module.enabled=false;
   foreach(var reference in references)if(reference)Destroy(reference);
   actions?.Dispose();actions=null;
  }
 }
}
