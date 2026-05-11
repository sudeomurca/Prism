using UnityEngine;

namespace Prism
{
    //  isik kaynagi renkleri
    // none isik yok demek (yanlis prizma kombinasyonu icin)
    public enum LightColor
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,
        Magenta,
        Cyan
    }

    
    [CreateAssetMenu(fileName = "NewLightColor", menuName = "Prism/Light Color")]
    public class LightColorData : ScriptableObject
    {
        public LightColor color = LightColor.Red;
        public Color displayColor = Color.red;

        [Header("Sprites")]
        [Tooltip("Bu renge ait kristal sprite")]
        public Sprite crystalSprite;

        [Tooltip("Bu renge ait isik kaynagi sprite")]
        public Sprite lightSourceSprite;

        // iki rengi karistir, sonuc rengi dondur
        public static LightColor Mix(LightColor a, LightColor b) => (a, b) switch
        {
            // ayni renkse degisim yok
            _ when a == b => a,

            // renk karisim sonuclari (mix sirasi onemsiz)
            (LightColor.Red,   LightColor.Green) => LightColor.Yellow,
            (LightColor.Green, LightColor.Red)   => LightColor.Yellow,

            (LightColor.Red,   LightColor.Blue)  => LightColor.Magenta,
            (LightColor.Blue,  LightColor.Red)   => LightColor.Magenta,

            (LightColor.Green, LightColor.Blue)  => LightColor.Cyan,
            (LightColor.Blue,  LightColor.Green) => LightColor.Cyan,

            // tanimsiz kombinasyon 
            // -> None doner ve prizma cikis vermez
            _ => LightColor.None
        };
    }
}
