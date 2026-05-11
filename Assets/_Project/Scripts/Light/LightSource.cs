using DG.Tweening;
using UnityEngine;

namespace Prism
{
    // isik kaynagi verisi
    // yon,renk leveldata dan ayarlanir
    public class LightSource : MonoBehaviour
    {
        [Header("Isik Ayarlari")]
        [Tooltip("Bu kaynagin hangi renkte isin uretecegi")]
        [SerializeField] private LightColorData colorData;

        [Tooltip("Isinin hangi yone dogru cikacagi")]
        [SerializeField] private Direction direction = Direction.Up;

        // sadece get,set yok (read-only)
        public LightColorData ColorData => colorData;
        public Direction Direction => direction;
        public Vector3 Position => transform.position;

        // self-registration pattern (isik kaynagi kendini beammanagera ekletir)
        private void Awake()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.RegisterSource(this);
        }

        //level degisiminde yok et 
        private void OnDestroy()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.UnregisterSource(this);

            transform.DOKill();
        }

        // LevelLoader runtimeda bu kaynagi spawn ederken cagirir
        public void SetConfig(LightColorData newColor, Direction newDir)
        {
            colorData = newColor;
            direction = newDir;

            // renk assetinde sprite varsa kullan
            if (newColor != null && newColor.lightSourceSprite != null)
            {
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = newColor.lightSourceSprite;
            }
        }
    }
}
