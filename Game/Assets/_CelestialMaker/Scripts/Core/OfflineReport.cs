using System;
using System.Collections.Generic;
using System.Text;
using BreakInfinity;

namespace CelestialMaker.Core {
 // One aggregate per absence, saved in the same State as the economy and cursor.
 [Serializable] public sealed class OfflineReport {
  public string id=Guid.NewGuid().ToString("N");
  public bool active=true,dismissed,legacyRemainder;
  public long fixedTicks;
  public double requestedSeconds,processedSeconds,boostSeconds;
  public Number[] before=new Number[15],after=new Number[15],produced=new Number[15],spent=new Number[15],resetLoss=new Number[15],granted=new Number[15];
  public bool[] knownBefore=new bool[15],knownAfter=new bool[15],stagesBefore=new bool[6],stagesAfter=new bool[6];
  public long[] automaticUnits=new long[24];
  public int[] resets=new int[3],finishedExpeditions=new int[8],claimedExpeditions=new int[8];
  public List<string> discoveries=new List<string>(),stories=new List<string>(),challenges=new List<string>();
  public void Add(Number[] totals,int resource,BigDouble amount){if(amount>0)totals[resource]=totals[resource].Value+amount;}
  public void Capture(Simulation sim,bool initial=false){
   for(int i=0;i<15;i++){
    var value=i<12?sim.S.resources[i]:sim.S.prestige[i-12];after[i]=value;
    bool known=i<12?sim.StageOpen(i/2)||sim.S.lifetime[i].Value>0||value.Value>0:sim.HasLayer(i-12)||value.Value>0;
    knownAfter[i]|=known;
    if(initial){before[i]=value;knownBefore[i]=known;}
   }
   for(int i=0;i<6;i++){stagesAfter[i]|=sim.StageOpen(i);if(initial)stagesBefore[i]=stagesAfter[i];}
  }
  public string Heading()=>"Welcome back • "+Duration(processedSeconds)+" simulated"+(active?" • Still catching up":"");
  public static string Duration(double seconds){double days=Math.Floor(seconds/86400);return days+"d "+Math.Floor(seconds%86400/3600)+"h "+Math.Floor(seconds%3600/60)+"m "+Math.Floor(seconds%60)+"s";}
  public string Describe(Content data,bool details){
   var text=new StringBuilder(Heading());
   text.Append("\nNet change includes production, spending and resets.");
   if(fixedTicks>0)text.Append("\nReplayed ticks (60 Hz): ").Append(fixedTicks.ToString("N0",System.Globalization.CultureInfo.InvariantCulture));
   for(int i=0;i<15;i++){
    if(!knownBefore[i]&&!knownAfter[i])continue;
    var delta=after[i].Value-before[i].Value;
    if(!details&&delta==0&&produced[i].Value==0&&spent[i].Value==0&&resetLoss[i].Value==0&&granted[i].Value==0)continue;
    string name=i<12?data.resources[i]:ResetRules.Currencies[i-12];
    text.Append("\n\n").Append(name).Append(knownBefore[i]?"":" (discovered)")
     .Append("\nBefore ").Append(before[i]).Append(" → After ").Append(after[i])
     .Append(" • Net ").Append(delta>0?"+":"").Append(new Number(delta));
    if(details){text.Append("\nProduced ").Append(produced[i]).Append(" • Spent ").Append(spent[i])
     .Append(" • Reset loss ").Append(resetLoss[i]).Append(" • Rewards ").Append(granted[i]);}
    else if(delta<0)text.Append("\nDecrease reflects spending or a reset, not negative production.");
   }
   long units=0;foreach(long count in automaticUnits)units+=count;
   int completed=0,claimed=0,resetCount=0;foreach(int count in finishedExpeditions)completed+=count;foreach(int count in claimedExpeditions)claimed+=count;foreach(int count in resets)resetCount+=count;
   text.Append("\n\nAutomatic purchases: ").Append(units).Append(" process units • Resets: ").Append(resetCount)
    .Append("\nDiscoveries: ").Append(discoveries.Count).Append(" • Story chapters: ").Append(stories.Count).Append(" • Challenges: ").Append(challenges.Count)
    .Append("\nExpeditions completed: ").Append(completed).Append(" • Claimed: ").Append(claimed);
   for(int i=0;i<6;i++)if(stagesAfter[i]&&!stagesBefore[i])text.Append("\nUnlocked: ").Append(new[]{"Particles","Atoms","Planets","Stars","Galaxies","Universes"}[i]);
   if(boostSeconds>0)text.Append("\nVerified 2× boost applied for ").Append(Duration(boostSeconds)).Append("; never included in challenges.");
   if(legacyRemainder)text.Append("\nAn older pending calculation resumes here; its earlier resource history was not available.");
   if(details){for(int i=0;i<24;i++)if(automaticUnits[i]>0)text.Append("\nAutobuy: ").Append(data.generators[i].name).Append(" × ").Append(automaticUnits[i]);for(int i=0;i<3;i++)if(resets[i]>0)text.Append("\n").Append(ResetRules.Names[i]).Append(" × ").Append(resets[i]);}
   return text.ToString();
  }
 }
}
