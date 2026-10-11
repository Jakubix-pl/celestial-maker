using System;
using System.IO;
using UnityEngine;
using CelestialMaker.Core;
namespace CelestialMaker {
 public static partial class SaveStore {
  public static string OverridePath;
  public static readonly string SelectedProfile=ReadProfile();
  static string ReadProfile(){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-cmProfile");if(i<0)return "";if(i+1>=args.Length)throw new ArgumentException("Save profile name missing");string value=args[i+1];if(value.Length<1||value.Length>48)throw new ArgumentException("Invalid save profile");foreach(char c in value)if(!char.IsLetterOrDigit(c)&&c!='_'&&c!='-')throw new ArgumentException("Invalid save profile");return value;}
  public static string ScenarioKey="";
  public static string Pathname=>BasePath+(ScenarioKey==""?"":".scenario-"+ScenarioKey+".json");
  public static string BasePath=>OverridePath??Path.Combine(Application.persistentDataPath,SelectedProfile!=""?"celestial-profile-"+SelectedProfile+".json":
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    DevelopmentSmoke.Enabled?"celestial-smoke.json":
#endif
    "celestial-v1.json");
  public static State Parse(string json){if(string.IsNullOrWhiteSpace(json)||json.Length>2*1024*1024)throw new ArgumentException("Save is empty or exceeds 2 MB");var state=SaveMigrations.Upgrade(JsonUtility.FromJson<State>(json));new Simulation(GameSession.Content,state);return state;}
  public static State Load(){if(!File.Exists(Pathname))return null;try{string raw=File.ReadAllText(Pathname);var legacy=UnityEngine.JsonUtility.FromJson<State>(raw);if(legacy.version<2&&!File.Exists(Pathname+".pre-expansion.bak"))File.Copy(Pathname,Pathname+".pre-expansion.bak");return Parse(raw);}catch(Exception e){Debug.LogWarning("Primary save rejected: "+e.Message);if(File.Exists(Pathname+".bak"))return Parse(File.ReadAllText(Pathname+".bak"));return null;}}
  public static void CheckRoute(State state){string slot=state.discovery.scenario==0?"":state.discovery.scenario+"-"+state.discovery.seed;if(slot!=ScenarioKey)throw new InvalidOperationException("This save belongs to a different campaign or scenario slot. Open its matching scenario first.");}
  public static void Write(State s){new Simulation(GameSession.Content,s).Validate();CheckRoute(s);string text=JsonUtility.ToJson(s);var tmp=Pathname+".tmp";Directory.CreateDirectory(Path.GetDirectoryName(Pathname));File.WriteAllText(tmp,text);if(File.Exists(Pathname))File.Replace(tmp,Pathname,Pathname+".bak");else File.Move(tmp,Pathname);}
 }
}
