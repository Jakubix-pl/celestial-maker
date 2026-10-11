using UnityEngine;
using UnityEngine.UI;

namespace CelestialMaker {
 public static class CosmicStyle {
  public static bool Light;
  public static Color Background=>Hex(Light?"EDF3F7":"0B1118");
  public static Color Panel=>Hex(Light?"FFFFFF":"121C27");
  public static Color Raised=>Hex(Light?"E3ECF2":"1A2836");
  public static Color Focus=>Hex(Light?"CFE5E6":"263F50");
  public static Color PrimaryFocus=>Hex(Light?"0E514D":"A6F1E7");
  public static Color PrimaryPressed=>Hex(Light?"0A423F":"50B9AF");
  public static Color Edge=>Hex(Light?"B1C0CB":"354858");
  public static Color Text=>Hex(Light?"152C3C":"EDF4FA");
  public static Color Muted=>Hex(Light?"425C70":"A5B7C8");
  public static Color Gold=>Cyan;
  public static Color Cyan=>Hex(Light?"176A65":"71DFD4");
  public static Color Violet=>Hex(Light?"72539C":"C1A5EF");
  public static Color Danger=>Hex(Light?"9C3644":"F1ACB5");
  public static Color Warning=>Hex(Light?"805500":"F0C178");
  public static Color Control=>Hex(Light?"647C8D":"748B9F");
  public static readonly string[] Chapters={"Particles","Atoms","Planets","Stars","Galaxies","Universes"};
  public static Color Section(int index)=>Hex((Light?new[]{"176A65","245C91","72539C","805500","5259A3","526575"}:new[]{"71DFD4","89C8FF","CBB0F3","F0C178","B6BDF8","BED0DF"})[Mathf.Clamp(index,0,5)]);
  public static Color Tint(Color accent,float amount=.07f)=>Color.Lerp(Panel,accent,amount);
  public static Color[] Accents=>new[]{Cyan,Cyan,Text,Cyan,Violet,Violet};
  public static Color Hex(string s){ColorUtility.TryParseHtmlString("#"+s,out var c);return c;}
  public static string Html(Color c)=>"#"+ColorUtility.ToHtmlStringRGB(c);
 }
}
