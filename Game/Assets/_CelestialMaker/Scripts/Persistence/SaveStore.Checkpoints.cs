using System;
using System.IO;
using System.Linq;
using CelestialMaker.Core;
using UnityEngine;

namespace CelestialMaker {
 public sealed class SaveCheckpoint {
  public string file,reason,error; public DateTime utc; public State state;
 }
 public static partial class SaveStore {
  public static string CheckpointFolder=>Pathname+".checkpoints";
  public static void Checkpoint(State state,string reason){
   if(state==null)return;new Simulation(GameSession.Content,state).Validate();
   Directory.CreateDirectory(CheckpointFolder);
   string safe=new string(reason.Where(c=>char.IsLetterOrDigit(c)||c=='-').Take(40).ToArray());
   string file=Path.Combine(CheckpointFolder,"point-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff")+"-"+safe+"-"+Guid.NewGuid().ToString("N").Substring(0,6)+".json");
   string tmp=file+".tmp";File.WriteAllText(tmp,JsonUtility.ToJson(state));Parse(File.ReadAllText(tmp));File.Move(tmp,file);
   foreach(var old in new DirectoryInfo(CheckpointFolder).GetFiles("point-*.json").OrderByDescending(f=>f.LastWriteTimeUtc).Skip(12))File.Delete(old.FullName);
  }
  public static SaveCheckpoint[] Checkpoints(){
   if(!Directory.Exists(CheckpointFolder))return Array.Empty<SaveCheckpoint>();
   return new DirectoryInfo(CheckpointFolder).GetFiles("point-*.json").OrderByDescending(f=>f.LastWriteTimeUtc).Take(12).Select(f=>{
    var p=new SaveCheckpoint{file=f.Name,reason=f.Name.Length>=37?f.Name.Substring(25,f.Name.Length-37):"Unlabelled restore point",utc=f.LastWriteTimeUtc};try{p.state=Parse(File.ReadAllText(f.FullName));}catch(Exception e){p.error=e.Message;}return p;
   }).ToArray();
  }
  public static State ReadCheckpoint(string name){
   if(string.IsNullOrEmpty(name)||Path.GetFileName(name)!=name||!name.StartsWith("point-")||!name.EndsWith(".json"))throw new ArgumentException("Invalid restore point");
   return Parse(File.ReadAllText(Path.Combine(CheckpointFolder,name)));
  }
 }
}
