using UnityEngine;

namespace Prism
{
    public enum LightColor
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,    
        Magenta,   
        Cyan,      
        White      
    }

    //datamizdan kolay sekilde asset uretebilmek icin menu olustur
    [CreateAssetMenu(fileName = "NewLightColor", menuName = "Prism/Light Color Data")]
    public class LightColorData : ScriptableObject
    {
        public LightColor color = LightColor.Red;
        public Color displayColor = Color.red;

        
        //karisim rengini dondur
        public static LightColor Mix(LightColor a, LightColor b) => (a, b) switch
        {
            (LightColor.Red,   LightColor.Green) => LightColor.Yellow,
            (LightColor.Green, LightColor.Red)   => LightColor.Yellow,

            (LightColor.Red,   LightColor.Blue)  => LightColor.Magenta,
            (LightColor.Blue,  LightColor.Red)   => LightColor.Magenta,

            (LightColor.Green, LightColor.Blue)  => LightColor.Cyan,
            (LightColor.Blue,  LightColor.Green) => LightColor.Cyan,

            // renkler ayniysa ayni kalir
            _ when a == b => a,

            // TODO
            _ => LightColor.White
        };
    }
}
