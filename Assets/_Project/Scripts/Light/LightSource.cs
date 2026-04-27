using UnityEngine;

namespace Prism
{
    // grid uzerinde duran isik kaynagi
    // yon ve renk level designer tarafindan ayarlanir, oyuncu mudahale edemez
    public class LightSource : MonoBehaviour
    {
        [Header("Isik Ayarlari")]
        [Tooltip("Bu kaynagin hangi renkte isin uretecegi.")]
        [SerializeField] private LightColorData colorData;

        [Tooltip("Isinin hangi yone dogru cikacagi.")]
        [SerializeField] private Direction direction = Direction.Up;

        // get var, set yok , disaridan degistirilemez
        public LightColorData ColorData => colorData;
        public Direction Direction => direction;

        // baska scriptlerin kullanmasi icin world pozisyonu
        public Vector3 Position => transform.position;

        // event-driven sistem: BeamManager bizi tani diye Awake'te kendimizi register ederiz
        private void Awake()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.RegisterSource(this);
        }

        private void OnDestroy()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.UnregisterSource(this);
        }

        // LevelLoader runtime'da bu kaynagi spawn ederken cagirir
        public void SetConfig(LightColorData newColor, Direction newDir)
        {
            colorData = newColor;
            direction = newDir;
        }
    }
}
