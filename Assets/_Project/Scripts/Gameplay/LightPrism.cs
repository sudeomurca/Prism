using DG.Tweening;
using UnityEngine;

namespace Prism
{
    // 2 isigi birlestiren prizma
    // SADECE belirli renk kombinasyonunu kabul eder, baska renk girerse cikis vermez
    // ornek: Magenta prizma sadece Red+Blue ister, Green girerse tikanir
    //
    // ONEMLI: prizma 'iki ayri yonden tam 2 isin' bekler (sol+sag)
    // ayni yonden 2 isin gelse veya tek yonden 1 isin gelse cikis yok
    // bu sayede yanlis level konfigurasyonunda kristal yanlislikla tetiklenmez
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

        [Header("Gorsel Feedback")]
        [Tooltip("Sol yarinin SpriteRenderer'i. Soldan gelen isin geldiginde renklenir.")]
        [SerializeField] private SpriteRenderer prismLeft;

        [Tooltip("Sag yarinin SpriteRenderer'i. Sagdan gelen isin geldiginde renklenir.")]
        [SerializeField] private SpriteRenderer prismRight;

        [Tooltip("Tint yogunlugu, 0=beyaz kalir, 1=tam isin rengi olur. Yari saydam his icin 0.5-0.7 onerilir.")]
        [Range(0f, 1f)]
        [SerializeField] private float tintStrength = 0.6f;

        public Direction OutputDirection => outputDirection;
        public LightColor ExpectedOutput => outputColorData != null ? outputColorData.color : LightColor.None;
        public Color DisplayColor => outputColorData != null ? outputColorData.displayColor : Color.gray;
        public Vector3 Position => transform.position;

        // bu frame'de gelen isiklar - karisim hesabi icin
        private LightColor accumulatedColor = LightColor.None;
        private int lightsReceivedThisFrame = 0;

        // bu frame'de hangi yarilara isin geldi - mantiksal validasyon ve gorsel tint icin
        private bool leftReceivedThisFrame = false;
        private bool rightReceivedThisFrame = false;

        private void Awake()
        {
            // collider config'i lokal degisken ile yapip biraktik, field tutmaya gerek yok
            var col = GetComponent<CircleCollider2D>();
            col.radius = colliderRadius;

            // event-driven sistem: BeamManager bizi tani diye kendimizi register ederiz
            if (BeamManager.Instance != null) BeamManager.Instance.RegisterPrism(this);
        }

        private void OnDestroy()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.UnregisterPrism(this);

            // sol/sag yari sprite'lari uzerindeki tint tween'lerini durdur
            // level reload sirasinda DOTween "target null" uyarisi vermesin diye
            if (prismLeft != null) prismLeft.DOKill();
            if (prismRight != null) prismRight.DOKill();
            transform.DOKill();
        }

        // LevelLoader runtime'da spawn ederken cagirir
        public void SetConfig(LightColorData color, Direction direction)
        {
            outputColorData = color;
            outputDirection = direction;
        }

        // isin geldi: accumulate et, geliste yonune gore ilgili yariyi tint et
        // tum isiklar geldikten sonra GetOutputColor cagrildiginda dogrulama yapilir
        public void ReceiveLight(LightColor incoming, Direction incomingDir, Color incomingDisplay)
        {
            if (lightsReceivedThisFrame == 0)
                accumulatedColor = incoming;
            else
                accumulatedColor = LightColorData.Mix(accumulatedColor, incoming);

            lightsReceivedThisFrame++;

            // gorsel tint: isin yonune gore hangi yariyi boyayacagini bul
            // SOLDAN gelen isin (Right yonunde hareket) -> sol yariyi boya
            // SAGDAN gelen isin (Left yonunde hareket)  -> sag yariyi boya
            // (level tasariminda sadece bu iki yon kullanilacak, yukari/asagi yok)
            Color tinted = Color.Lerp(Color.white, incomingDisplay, tintStrength);

            if (incomingDir == Direction.Right)
            {
                AnimationHelper.TintColor(prismLeft, tinted);
                leftReceivedThisFrame = true;
            }
            else if (incomingDir == Direction.Left)
            {
                AnimationHelper.TintColor(prismRight, tinted);
                rightReceivedThisFrame = true;
            }
        }

        // prizmanin cikis rengini dondurur:
        //   - iki yariya da isin gelmediyse None (tek tarafli aktivasyon kabul edilmez)
        //   - karisim beklenen renk degilse None (yanlis renk kombinasyonu)
        //   - aksi takdirde beklenen renk
        // BeamManager bunun donusunu kontrol eder, None ise prizma cikisi cizilmez
        public LightColor GetOutputColor()
        {
            if (!leftReceivedThisFrame || !rightReceivedThisFrame) return LightColor.None;
            return accumulatedColor == ExpectedOutput ? ExpectedOutput : LightColor.None;
        }

        // BeamManager her recompute'un BASINDA cagirir
        // gorsel tint'leri hemen beyaza ceker, ReceiveLight tekrar gelirse ustune yazilir
        // (eskiden flag'leri sona kadar tutuyorduk, eski state'e bakip yanlis karar veriyorduk)
        public void ResetFrame()
        {
            accumulatedColor = LightColor.None;
            lightsReceivedThisFrame = 0;
            leftReceivedThisFrame = false;
            rightReceivedThisFrame = false;

            // her iki yariyi da beyaza dondur, ReceiveLight gelirse ustune yazilir
            AnimationHelper.TintColor(prismLeft, Color.white);
            AnimationHelper.TintColor(prismRight, Color.white);
        }
    }
}
