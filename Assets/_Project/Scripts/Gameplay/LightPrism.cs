using UnityEngine;

namespace Prism
{
    // 2 isigi birlestiren prizma
    // SADECE belirli renk kombinasyonunu kabul eder, baska renk girerse cikis vermez
    // ornek: Magenta prizma sadece Red+Blue ister, Green girerse tikanir
    [RequireComponent(typeof(CircleCollider2D))]
    public class LightPrism : MonoBehaviour
    {
        [Header("Prizma Ayarlari")]
        [Tooltip("Bu prizma hangi rengi uretir, ScriptableObject olarak ver. Magenta/Cyan/Yellow olabilir.")]
        [SerializeField] private LightColorData outputColorData;

        [Tooltip("Karisim isigi hangi yone gonderilecek.")]
        [SerializeField] private Direction outputDirection = Direction.Up;

        [Tooltip("Collider yaricapi.")]
        [SerializeField] private float colliderRadius = 0.3f;

        public Direction OutputDirection => outputDirection;
        public LightColor ExpectedOutput => outputColorData != null ? outputColorData.color : LightColor.None;
        public Color DisplayColor => outputColorData != null ? outputColorData.displayColor : Color.gray;
        public Vector3 Position => transform.position;

        // bu frame'de gelen isiklar
        private LightColor accumulatedColor = LightColor.None;
        private int lightsReceivedThisFrame = 0;

        private CircleCollider2D circleCollider;

        private void Awake()
        {
            circleCollider = GetComponent<CircleCollider2D>();
            circleCollider.radius = colliderRadius;

            // event-driven sistem: BeamManager bizi tani diye kendimizi register ederiz
            if (BeamManager.Instance != null) BeamManager.Instance.RegisterPrism(this);
        }

        private void OnDestroy()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.UnregisterPrism(this);
        }

        // LevelLoader runtime'da spawn ederken cagirir
        public void SetConfig(LightColorData color, Direction direction)
        {
            outputColorData = color;
            outputDirection = direction;
        }

        // isin geldi: accumulate et
        // tum isiklar geldikten sonra GetOutputColor cagrildiginda dogrulama yapilir
        public void ReceiveLight(LightColor incoming)
        {
            if (lightsReceivedThisFrame == 0)
            {
                accumulatedColor = incoming;
            }
            else
            {
                accumulatedColor = LightColorData.Mix(accumulatedColor, incoming);
            }
            lightsReceivedThisFrame++;
        }

        // sadece beklenen renk olustuysa cikar, yoksa None doner (isin tikanir)
        public LightColor GetOutputColor()
        {
            return accumulatedColor == ExpectedOutput ? ExpectedOutput : LightColor.None;
        }

        // cikis yapabilir mi
        public bool HasValidOutput()
        {
            return accumulatedColor == ExpectedOutput;
        }

        public bool HasInput() => lightsReceivedThisFrame > 0;

        public void ResetFrame()
        {
            accumulatedColor = LightColor.None;
            lightsReceivedThisFrame = 0;
        }
    }
}
