using UnityEngine;

namespace Prism
{
    public enum LightColor
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,    // Red + Green
        Magenta,   // Red + Blue
        Cyan,      // Green + Blue
        White      // All
    }

    
    [CreateAssetMenu(fileName = "NewLightColor", menuName = "Prism/Light Color Data")]
    public class LightColorData : ScriptableObject
    {
        public LightColor color = LightColor.Red;
        public Color displayColor = Color.red;

        // İki rengin karışım sonucunu döndürür.
        // Switch expression (C# 8+) kullanıldı; klasik switch'ten daha okunaklı.
        public static LightColor Mix(LightColor a, LightColor b) => (a, b) switch
        {
            (LightColor.Red,   LightColor.Green) => LightColor.Yellow,
            (LightColor.Green, LightColor.Red)   => LightColor.Yellow,

            (LightColor.Red,   LightColor.Blue)  => LightColor.Magenta,
            (LightColor.Blue,  LightColor.Red)   => LightColor.Magenta,

            (LightColor.Green, LightColor.Blue)  => LightColor.Cyan,
            (LightColor.Blue,  LightColor.Green) => LightColor.Cyan,

            // Aynı renk + aynı renk = yine aynı renk
            _ when a == b => a,

            // Tanımsız karışım (ileride geliştirilecek)
            _ => LightColor.White
        };
    }
}
