using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CelestialMaker {
 // Explicit surface and text pairs retain contrast in both themes and all states.
 public sealed class ObservatoryButton : Button {
  TMP_Text label;bool primary,emphasized,sectionTab;Coroutine fade;Color? accent;
  public void Accent(Color color){accent=color;Refresh();}
  public void TabAccent(Color color){accent=color;sectionTab=true;Refresh();}
  public void Configure(TMP_Text text,bool isPrimary=false){label=text;primary=isPrimary;transition=Transition.None;Refresh();}
  public void Emphasize(bool value){emphasized=value;Refresh();}
  void Refresh()=>DoStateTransition(currentSelectionState,true);
  protected override void DoStateTransition(SelectionState state,bool instant){
   var panel=targetGraphic as CosmicPanel;if(!panel||!label)return;
   bool disabled=state==SelectionState.Disabled,focused=state==SelectionState.Selected||state==SelectionState.Highlighted,pressed=state==SelectionState.Pressed;
   var highlight=accent??CosmicStyle.Cyan;
   var next=disabled?CosmicStyle.Raised:primary?(pressed?CosmicStyle.PrimaryPressed:focused?CosmicStyle.PrimaryFocus:CosmicStyle.Cyan):focused||pressed?CosmicStyle.Focus:emphasized?Color.Lerp(CosmicStyle.Raised,highlight,.14f):CosmicStyle.Raised;
   if(sectionTab&&!disabled)next=Color.Lerp(CosmicStyle.Raised,highlight,CosmicStyle.Light?(focused||pressed?.14f:emphasized?.10f:.045f):(focused||pressed?.24f:emphasized?.19f:.075f));
   if(fade!=null){StopCoroutine(fade);fade=null;}
   if(instant||disabled||!Application.isPlaying||!isActiveAndEnabled||(GameSession.Instance?.CurrentSettings.reducedMotion??true))panel.color=next;
   else fade=StartCoroutine(Fade(panel,next));
   label.color=disabled?CosmicStyle.Muted:primary?(CosmicStyle.Light?Color.white:CosmicStyle.Background):CosmicStyle.Text;
   panel.edgeColor=disabled?CosmicStyle.Edge:focused||pressed||emphasized?highlight:primary?CosmicStyle.Cyan:accent.HasValue?Color.Lerp(CosmicStyle.Control,highlight,.45f):CosmicStyle.Control;
   if(primary&&!disabled&&(focused||pressed))panel.edgeColor=CosmicStyle.Light?Color.white:CosmicStyle.Background;
   panel.edge=focused||pressed||emphasized?2:1;panel.SetVerticesDirty();
  }
  System.Collections.IEnumerator Fade(CosmicPanel panel,Color target){var from=panel.color;float elapsed=0;while(elapsed<.10f){elapsed+=Time.unscaledDeltaTime;panel.color=Color.Lerp(from,target,Mathf.Clamp01(elapsed/.10f));if(GameSession.Instance?.CurrentSettings.reducedMotion??true)break;yield return null;}panel.color=target;fade=null;}
  protected override void OnDisable(){if(fade!=null){StopCoroutine(fade);fade=null;}base.OnDisable();}
 }
}
