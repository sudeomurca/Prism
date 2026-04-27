using UnityEngine;

namespace Prism
{
    // diyagonal ayna, 4 farkli yonde durabilir
    // tap olunca saat yonu TERSINE 90 derece doner (rotationIndex artar)
    //
    // sprite Aseprite'ta YATAY cizildi, PARLAK (yansitici) yuz UST KENARDA.
    // rotation pozitif yon = saat tersi. Sprite'in ust kenari rotation ile birlikte
    // su konumlara dogru bakar:
    //   Idx 0:  +45  ->  /  parlak yuz SOL-UST
    //   Idx 1: +135  ->  \  parlak yuz SOL-ALT
    //   Idx 2: +225  ->  /  parlak yuz SAG-ALT
    //   Idx 3: +315  ->  \  parlak yuz SAG-UST
    //
    // her durumda parlak yuze carpan 2 yon yansir (90 derece doner),
    // diger 2 yon arkadan vurur ve durur (None).
    [RequireComponent(typeof(BoxCollider2D))]
    public class Mirror : MonoBehaviour
    {
        [Header("Ayna Durumu")]
        [Tooltip("0=/sol-ust  1=\\sol-alt  2=/sag-alt  3=\\sag-ust (parlak yuzun konumu)")]
        [Range(0, 3)]
        [SerializeField] private int rotationIndex = 0;

        [Header("Visual")]
        [Tooltip("Sprite renderer referansi, gorsel rotation icin.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        public int RotationIndex => rotationIndex;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            UpdateVisualRotation();
        }

        // rotationIndex'e gore transform'u dondurur
        // 0 -> 45, 1 -> 135, 2 -> 225, 3 -> 315
        // animate=true ise DOTween ile yumusak gecis, false ise aninda (Awake/SetRotationIndex icin)
        private void UpdateVisualRotation(bool animate = false)
        {
            float angle = 45f + rotationIndex * 90f;

            if (animate)
            {
                AnimationHelper.RotateTo(transform, angle);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // tap olunca saat yonu TERSINE 90 derece doner (rotationIndex artar)
        // hamle sayilmiyor (prototip karari, sinirsiz tap)
        public void OnTap()
        {
            rotationIndex = (rotationIndex + 1) % 4;
            UpdateVisualRotation(animate: true); // tap olunca smooth animasyon

            // ayna degisti, BeamManager isinlari yeniden hesaplasin
            if (BeamManager.Instance != null) BeamManager.Instance.RecomputeBeams();
        }

        // LevelLoader runtime'da spawn ederken bu degeri ayarlar
        public void SetRotationIndex(int index)
        {
            rotationIndex = Mathf.Clamp(index, 0, 3);
            UpdateVisualRotation(animate: false); // spawn sirasinda aninda set, animasyon yok
        }

        // gelen yon verilince yansima yonunu dondurur.
        // arkadan gelen (yansitici OLMAYAN yuze carpan) icin None doner.
        public Direction Reflect(Direction incoming)
        {
            switch (rotationIndex)
            {
                // Idx 0: /  parlak SOL-UST
                // ust+sol taraflardan gelen parlak yuze carpar, yansir
                case 0:
                    return incoming switch
                    {
                        Direction.Down  => Direction.Left, // yukaridan iner  -> sola
                        Direction.Right => Direction.Up,   // soldan gelir    -> yukari
                        _ => Direction.None
                    };

                // Idx 1: \  parlak SOL-ALT
                // alt+sol taraflardan gelen parlak yuze carpar, yansir
                case 1:
                    return incoming switch
                    {
                        Direction.Up    => Direction.Left, // asagidan cikar -> sola
                        Direction.Right => Direction.Down, // soldan gelir   -> asagi
                        _ => Direction.None
                    };

                // Idx 2: /  parlak SAG-ALT
                // alt+sag taraflardan gelen parlak yuze carpar, yansir
                case 2:
                    return incoming switch
                    {
                        Direction.Up   => Direction.Right, // asagidan cikar -> saga
                        Direction.Left => Direction.Down,  // sagdan gelir   -> asagi
                        _ => Direction.None
                    };

                // Idx 3: \  parlak SAG-UST
                // ust+sag taraflardan gelen parlak yuze carpar, yansir
                case 3:
                    return incoming switch
                    {
                        Direction.Down => Direction.Right, // yukaridan iner -> saga
                        Direction.Left => Direction.Up,    // sagdan gelir   -> yukari
                        _ => Direction.None
                    };

                default:
                    return Direction.None;
            }
        }

        public bool CanReflect(Direction incoming) => Reflect(incoming) != Direction.None;

        // Inspector'da rotationIndex degisirse gorsel de guncellesin
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            rotationIndex = Mathf.Clamp(rotationIndex, 0, 3);
            UpdateVisualRotation();
        }
    }
}
