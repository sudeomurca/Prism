using UnityEngine;

namespace Prism
{
    
    public class LightSource : MonoBehaviour
    {
        [Header("Isik Ayarlari")]
        [Tooltip("Bu kaynagin hangi renkte isin uretecegi.")]
        [SerializeField] private LightColorData colorData;

        [Tooltip("Isinin hangi yone dogru cikacagi.")]
        [SerializeField] private Direction direction = Direction.Up;

        // get var set yok
        public LightColorData ColorData => colorData;
        public Direction Direction => direction;

        // world pozisyonunu getir (baska scriptlerin kullanimi icin)
        public Vector3 Position => transform.position;

        // debug
        private void OnDrawGizmos()
        {
            
            Gizmos.color = colorData != null ? colorData.displayColor : Color.white;
            Gizmos.DrawSphere(transform.position, 0.25f);

            
            Vector3 dirVector = direction.ToVector();
            Gizmos.DrawLine(transform.position, transform.position + dirVector * 0.6f);
        }
    }
}
