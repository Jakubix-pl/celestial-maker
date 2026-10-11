using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using CelestialMaker.Core;
namespace CelestialMaker {
 public sealed partial class GameSession:MonoBehaviour {
  public static GameSession Instance; public static Content Content;
  public Simulation Sim; public State Saved; public string Message=""; public bool SaveReady,SaveBusy,WebWriter=true;
  public bool SaveFailed; public double LastSavedUtc;
  double autosave,suspendedUtc,monotonic;bool applicationPaused,applicationFocused=true;
  public string LastOfflineSummary=>Sim?.S.offlineReport!=null&&!Sim.S.offlineReport.active?Sim.S.offlineReport.Heading():"";
  bool DesktopBackground=>Application.platform==RuntimePlatform.WindowsPlayer||Application.platform==RuntimePlatform.LinuxPlayer||Application.platform==RuntimePlatform.OSXPlayer;
  bool Suspended=>applicationPaused||!applicationFocused&&!DesktopBackground;
  bool IsTest=>SaveStore.OverridePath!=null
#if DEVELOPMENT_BUILD || UNITY_EDITOR
   ||DevelopmentSmoke.Enabled
#endif
   ;
  public double LastTickMs;
  public Settings MenuSettings=new Settings();string SettingsKey=>SaveStore.OverridePath!=null?"celestial-test-settings":
#if DEVELOPMENT_BUILD || UNITY_EDITOR
 DevelopmentSmoke.Enabled?"celestial-smoke-settings":
#endif
 "celestial-settings"+(SaveStore.SelectedProfile!=""?"-"+SaveStore.SelectedProfile:"");public Settings CurrentSettings=>Sim?.S.settings??Saved?.settings??MenuSettings;
#if UNITY_WEBGL && !UNITY_EDITOR
  [DllImport("__Internal")]static extern void CM_Load();
  [DllImport("__Internal")]static extern void CM_Save(string json);
  [DllImport("__Internal")]static extern void CM_Download(string text);
  [DllImport("__Internal")]static extern void CM_RequestImport();
#endif
  void Awake(){System.Globalization.CultureInfo.CurrentCulture=System.Globalization.CultureInfo.GetCultureInfo("en-US");System.Globalization.CultureInfo.CurrentUICulture=System.Globalization.CultureInfo.GetCultureInfo("en-US");System.Globalization.CultureInfo.DefaultThreadCurrentCulture=System.Globalization.CultureInfo.CurrentCulture;System.Globalization.CultureInfo.DefaultThreadCurrentUICulture=System.Globalization.CultureInfo.CurrentUICulture;if(Instance&&Instance!=this){Destroy(gameObject);return;}Instance=this;DontDestroyOnLoad(gameObject);if(DesktopBackground)Application.runInBackground=true;Content=JsonUtility.FromJson<Content>(Resources.Load<TextAsset>("content").text);var account=new GameObject("AccountBridge");account.transform.SetParent(transform);account.AddComponent<AccountBridge>();try{var storedSettings=SaveStore.OverridePath==null?PlayerPrefs.GetString(SettingsKey,""):"";if(storedSettings!="")MenuSettings=JsonUtility.FromJson<Settings>(storedSettings);}catch{MenuSettings=new Settings();}
#if UNITY_WEBGL && !UNITY_EDITOR
   CM_Load();
#else
   try{Saved=SaveStore.Load();}catch(Exception e){Message="Save recovery failed: "+e.Message;}SaveReady=true;SceneManager.LoadScene("01_MainMenu");
#endif
  }
  public void WebLoadComplete(string text){try{if(text!="")Saved=SaveStore.Parse(text);}catch(Exception e){Message=e.Message;}SaveReady=true;SceneManager.LoadScene("01_MainMenu");}
  public void WebSaveComplete(string result){SaveBusy=false;SaveFailed=result!="ok";if(!SaveFailed)LastSavedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();if(result!="ok"){Message="Save failed: "+result;Debug.LogError(Message);}}
  public void WebReadOnly(string ignored){WebWriter=false;Message="This tab is read-only. Close the other game tab, then reload.";}
  public void StartGame(bool fresh){if(!SaveReady||!WebWriter)return;if(fresh&&Saved!=null){Message="Existing progress is preserved until you confirm New Game.";return;}Begin(fresh?new State{settings=JsonUtility.FromJson<Settings>(JsonUtility.ToJson(CurrentSettings))}:Saved??new State());}
  public void ConfirmNew(){if(!CreateCheckpoint("new-game"))return;Begin(FreshState());}
  void Begin(State state){lastPrestigeCheckpoint=-3600;if(state.utc==0&&(Application.platform==RuntimePlatform.Android||Screen.width<700)){state.settings.quality=0;state.settings.fps=Application.platform==RuntimePlatform.Android?60:30;}double now=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000d;double absence=state.utc>0?Math.Max(0,now-state.utc):0;state.utc=now;Sim=new Simulation(Content,state);Sim.BeforeReset=layer=>Sim.S.offlineRemaining>0||CreateCheckpoint(ResetRules.Names[layer]);if(absence>=60||state.offlineRemaining>0)CreateCheckpoint("before-offline");Sim.BeginOffline(absence);AccountBridge.Instance.ApplyCached();QualitySettings.SetQualityLevel(Math.Min(state.settings.quality,QualitySettings.names.Length-1));Number.DisplayNotation=state.settings.notation;SceneManager.LoadScene("02_Game");}
  public void Import(string text){if(!WebWriter)throw new InvalidOperationException("This tab is read-only. Close the other game tab, then reload.");var candidate=SaveStore.Parse(text);SaveStore.CheckRoute(candidate);if(!CreateCheckpoint("before-import"))throw new InvalidOperationException(Message);if(Sim!=null)Save();Saved=candidate;Begin(candidate);Save();Message="Save imported. Your cosmic history is preserved.";}
  double lastPrestigeCheckpoint=-3600;
  public bool CreateCheckpoint(string reason){
   if(reason=="Prestige"&&Sim!=null&&Sim.S.time-lastPrestigeCheckpoint<600)return true;
   try{SaveStore.Checkpoint(Sim?.S??Saved,reason);if(reason=="Prestige")lastPrestigeCheckpoint=Sim.S.time;return true;}
   catch(Exception e){Message="Restore point failed: "+e.Message;SaveFailed=true;return false;}
  }
  public void RestoreCheckpoint(string file){
   var candidate=SaveStore.ReadCheckpoint(file);SaveStore.CheckRoute(candidate);if(!CreateCheckpoint("before-restore"))throw new InvalidOperationException(Message);
   candidate.utc=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000d;Saved=candidate;Begin(candidate);Save();Message="Restore point loaded. A copy of the previous state was retained.";
  }
  public void RequestWebImport(){
#if UNITY_WEBGL && !UNITY_EDITOR
   if(!WebWriter){Message="This tab is read-only. Close the other game tab, then reload.";return;}WebGLInput.captureAllKeyboardInput=false;CM_RequestImport();
#endif
  }
  public void WebImportComplete(string text){WebImportCancelled("");try{Import(text);}catch(Exception e){Message="Import rejected: "+e.Message;}}
  public void WebImportCancelled(string ignored){
#if UNITY_WEBGL && !UNITY_EDITOR
   WebGLInput.captureAllKeyboardInput=true;
#endif
  }
  public void ReturnToMenu(){Save();Sim=null;SceneManager.LoadScene("01_MainMenu");}
  public void Save(){if(!WebWriter||SaveBusy)return;if(SaveStore.OverridePath==null)PlayerPrefs.SetString(SettingsKey,JsonUtility.ToJson(CurrentSettings));if(Sim!=null){Sim.SettleUniverses();Sim.Validate();Sim.S.utc=Suspended&&suspendedUtc>0&&!IsTest?suspendedUtc:DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000d;Saved=Sim.S;}if(Saved==null)return;
#if UNITY_WEBGL && !UNITY_EDITOR
   SaveBusy=true;CM_Save(JsonUtility.ToJson(Saved));
#else
   try{SaveBusy=true;SaveStore.Write(Saved);SaveFailed=false;LastSavedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();}catch(Exception e){SaveFailed=true;Message="Save failed: "+e.Message;Debug.LogError(Message);}finally{SaveBusy=false;}
#endif
  }
  public void Export(){Sim?.SettleUniverses();var s=Sim?.S??Saved;if(s==null)return;string json=JsonUtility.ToJson(s);
#if UNITY_WEBGL && !UNITY_EDITOR
   CM_Download(json);
#else
   string file=System.IO.Path.Combine(Application.persistentDataPath,"celestial-export-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")+".json");System.IO.File.WriteAllText(file,json);GUIUtility.systemCopyBuffer=json;Message="Export saved: "+file+" (also copied to clipboard)";
#endif
  }
  void Update(){double nowTick=Time.realtimeSinceStartupAsDouble;double elapsed=monotonic>0?Math.Max(0,nowTick-monotonic):0;monotonic=nowTick;if(Sim==null||!WebWriter||Suspended&&!IsTest)return;var s=Sim.S;double dt=elapsed;long tickStart=System.Diagnostics.Stopwatch.GetTimestamp();
   if(s.offlineRemaining>0||s.liveRemaining>2){Sim.BeginOffline(dt);Sim.ProcessOffline(60000,6);if(s.offlineRemaining==0)Save();}
   else {FixedStepClock.Advance(Sim,dt);}
   LastTickMs=(System.Diagnostics.Stopwatch.GetTimestamp()-tickStart)*1000d/System.Diagnostics.Stopwatch.Frequency;
   autosave+=dt;if(autosave>=20){autosave=0;Save();}Application.targetFrameRate=DesktopBackground?Math.Max(60,s.settings.fps):applicationFocused?s.settings.fps:5;
  }
  void SuspensionChanged(bool wasSuspended){if(IsTest||wasSuspended==Suspended)return;monotonic=Time.realtimeSinceStartupAsDouble;if(Suspended){suspendedUtc=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000d;Save();}else if(Sim!=null&&suspendedUtc>0){double now=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()/1000d;Sim.BeginOffline(Math.Max(0,now-suspendedUtc));Sim.S.utc=now;AccountBridge.Instance.ApplyCached();suspendedUtc=0;}}
  public void WebVisibility(string hidden){bool was=Suspended;applicationFocused=hidden!="hidden";SuspensionChanged(was);}
  void OnApplicationPause(bool paused){bool was=Suspended;applicationPaused=paused;SuspensionChanged(was);}
  void OnApplicationFocus(bool focus){if(!DesktopBackground)monotonic=Time.realtimeSinceStartupAsDouble;Application.targetFrameRate=DesktopBackground?Math.Max(60,CurrentSettings.fps):focus?CurrentSettings.fps:5;bool was=Suspended;applicationFocused=focus;SuspensionChanged(was);}
  void OnApplicationQuit(){Save();}
 }
}





