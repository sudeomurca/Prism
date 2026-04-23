using UnityEngine;

namespace Prism
{
    // Grid uzerinde duran, isin uretien bir kaynak.
    
    public class LightSource : MonoBehaviour
    {
        [Header("Isik Ayarlari")]
        [Tooltip("Bu kaynagin hangi renkte isin uretecegi.")]
        [SerializeField] private LightColorData colorData;

        [Tooltip("Isinin hangi yone dogru cikacagi.")]
        [SerializeField] private Direction direction = Direction.Up;

        // Disariya okuma izni, degistirmek yok.
        public LightColorData ColorData => colorData;
        public Direction Direction => direction;

        // Dunya pozisyonunu getirir (baska scriptler bunu kullanacak).
        public Vector3 Position => transform.position;

        // Sahne editorunde bu kaynagi kolay gorebilelim diye kucuk bir gizmo.
        private void OnDrawGizmos()
        {
            // Kaynak pozisyonunda bir kucuk daire
            Gizmos.color = colorData != null ? colorData.displayColor : Color.white;
            Gizmos.DrawSphere(transform.position, 0.25f);

            // Yon gostergesi: isinin nereye gidecegini ok gibi ciz
            Vector3 dirVector = direction.ToVector();
            Gizmos.DrawLine(transform.position, transform.position + dirVector * 0.6f);
        }
    }
}
