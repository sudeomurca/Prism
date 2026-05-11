using DG.Tweening;
using UnityEngine;

namespace Prism
{
    // tap olunca saat yonu TERSINE 90 derece doner (4 yon)
    // yansiyan isin 90 derece doner
    
    [RequireComponent(typeof(BoxCollider2D))]
    public class Mirror : MonoBehaviour
    {
        [Header("Ayna Durumu")]
        [Tooltip("0=/sol-ust  1=\\sol-alt  2=/sag-alt  3=\\sag-ust (yansitan yuzeylerin durumlari) ")]
        [Range(0, 3)]
        [SerializeField] private int rotationIndex = 0;

        [Header("Visual")]
        [Tooltip("Rotation yapinca sprite gorsel olarak guncellenir")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        public int RotationIndex => rotationIndex;
        public Vector3 Position => transform.position;

        // [rotationIndex, gelen yon] -> cikan yon tablosu
        private static readonly Direction[,] ReflectionTable = BuildReflectionTable();

        //2 boyutlu dizi
        private static Direction[,] BuildReflectionTable()
        {
            // 4 rotation (satir) x 5 Direction (sutun)
            int dirCount = System.Enum.GetValues(typeof(Direction)).Length;
            var t = new Direction[4, dirCount];
            //default direction Direction.None olur
            // sol ust yansitan ayna (/)
            t[0, (int)Direction.Down]  = Direction.Left;  
            t[0, (int)Direction.Right] = Direction.Up;    
            // sol alt yansitan ayna (\)
            t[1, (int)Direction.Up]    = Direction.Left;  
            t[1, (int)Direction.Right] = Direction.Down;  
            // sag alt yansitan ayna (/)
            t[2, (int)Direction.Up]   = Direction.Right;  
            t[2, (int)Direction.Left] = Direction.Down;   
            // sag ust yansitan ayna (\)
            t[3, (int)Direction.Down] = Direction.Right;  
            t[3, (int)Direction.Left] = Direction.Up;     

            return t;
        }

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            UpdateVisualRotation();
        }

        // destroy edilmeden once aktif tweenleri durdur
        private void OnDestroy()
        {
            transform.DOKill();
        }

        // rotationIndex'e gore transformu 90 derece dondurur
        private void UpdateVisualRotation(bool animate = false)
        {
            //0 indexte 45 derece
            float angle = 45f + rotationIndex * 90f;

            if (animate)
                AnimationHelper.RotateTo(transform, angle);
            else
                transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // tap olunca saat yonu tersine 90 derece doner (rotationIndex++)
        public void OnTap()
        {
            rotationIndex = (rotationIndex + 1) % 4;
            UpdateVisualRotation(animate: true);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.MirrorTap);

            // ayna degisti, BeamManager isinlari yeniden hesaplasin
            if (BeamManager.Instance != null) BeamManager.Instance.RecomputeBeams();
        }

        // levelLoader runtimeda spawn ederken bu degeri ayarlar
        public void SetRotationIndex(int index)
        {
            rotationIndex = Mathf.Clamp(index, 0, 3);
            UpdateVisualRotation(animate: false);
        }

        // gelen yon verilince yansima yonunu dondurur
        public Direction Reflect(Direction incoming) => ReflectionTable[rotationIndex, (int)incoming];

        // isinin carptigi yuzey yansitmayan tarafsa none
        public bool CanReflect(Direction incoming) => Reflect(incoming) != Direction.None;

        // Inspector'da rotationIndex degisirse sprite guncellesin
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            rotationIndex = Mathf.Clamp(rotationIndex, 0, 3);
            UpdateVisualRotation();
        }
    }
}
