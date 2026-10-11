using UnityEngine;
namespace CelestialMaker {
 // A short opacity transition changes neither layout nor input availability.
 [RequireComponent(typeof(CanvasGroup))]
 public sealed class ObservatoryReveal:MonoBehaviour {
  CanvasGroup group;float elapsed;
  public static void Begin(RectTransform target,bool reduced){var effect=target.GetComponent<ObservatoryReveal>()??target.gameObject.AddComponent<ObservatoryReveal>();effect.group=target.GetComponent<CanvasGroup>();effect.elapsed=0;effect.group.alpha=reduced?1:0;effect.enabled=!reduced;}
  public void Step(float dt,bool reduced){if(!group)group=GetComponent<CanvasGroup>();elapsed+=Mathf.Max(0,dt);float t=Mathf.Clamp01(elapsed/.14f);group.alpha=reduced?1:1-Mathf.Pow(1-t,3);if(group.alpha>=.999f){group.alpha=1;enabled=false;}}
  void Update()=>Step(Time.unscaledDeltaTime,GameSession.Instance?.CurrentSettings.reducedMotion??true);
  void OnDisable(){if(group)group.alpha=1;}
 }
}
