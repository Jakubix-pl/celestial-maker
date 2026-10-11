using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using CelestialMaker.Core;
namespace CelestialMaker.EditorTools {
 public static class ExpansionBatch {
  [Serializable]class Result {public int pass,fail;public string[] checks;public string visual="UNVERIFIED - no graphical player or Play Mode",utc;}
  public static void Validate(){
   var rows=new List<string>();int pass=0,fail=0;
   void Check(string name,Action action){try{action();pass++;rows.Add("PASS "+name);}catch(Exception e){fail++;rows.Add("FAIL "+name+": "+(e.InnerException??e).Message);}}
   var asm=Assembly.Load("CelestialMaker.EditTests");var type=asm.GetType("CoreTests",true);var instance=Activator.CreateInstance(type);
   foreach(var method in type.GetMethods().Where(m=>m.GetCustomAttributes(false).Any(a=>a.GetType().Name=="TestAttribute")))Check(method.Name,()=>method.Invoke(instance,null));
   var content=JsonUtility.FromJson<Content>(Resources.Load<TextAsset>("content").text);
   Check("Unity JSON expansion and parallel save roundtrip",()=>{var s=new Simulation(content);s.S.resets[1]=s.S.resets[2]=1;s.CreateUniverse(1);s.Meta.megas[0].level=2;s.Meta.laws.Add("flux_density");s.SaveStrategy(0);var clone=SaveMigrations.Upgrade(JsonUtility.FromJson<State>(JsonUtility.ToJson(s.S)));new Simulation(content,clone);if(clone.expansion.universes.Count!=1||clone.expansion.megas[0].level!=2||clone.expansion.strategies[0].laws[0]!="flux_density")throw new Exception("Roundtrip differs");});
   Check("PC expansion routes and fonts present",()=>{var script=File.ReadAllText(Path.Combine(Application.dataPath,"_CelestialMaker/Scripts/UI/GameUI.Observatory.cs"));foreach(var p in new[]{"Analytics","Automation","Roadmap","Megastructures","Reality laws","Parallel universes"})if(!script.Contains("case \""+p+"\""))throw new Exception("Missing route "+p);if(!Resources.Load<TMPro.TMP_FontAsset>("UI/PC/BodyFont")||!Resources.Load<GameObject>("UI/PC/Panel"))throw new Exception("PC assets missing");if(script.Contains("ShopPc"))throw new Exception("Store route retained");});
   Check("No billing dependency",()=>{if(File.ReadAllText(Path.Combine(Application.dataPath,"../Packages/manifest.json")).Contains("com.unity.purchasing"))throw new Exception("Billing package present");});
   string path=Path.Combine(ProjectBootstrap.Root,"Logs/Expansion/unity-headless-tests.json");File.WriteAllText(path,JsonUtility.ToJson(new Result{pass=pass,fail=fail,checks=rows.ToArray(),utc=DateTime.UtcNow.ToString("o")},true));foreach(var row in rows)Debug.Log(row);if(fail>0)throw new Exception("Headless Unity validation failed: "+fail);
  }
 }
}
