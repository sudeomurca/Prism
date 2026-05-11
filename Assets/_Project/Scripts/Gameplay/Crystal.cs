using DG.Tweening;
using UnityEngine;

namespace Prism
{
    // belirli bir renk beklenir, isin o renkle carparsa aktiflesir
    // aktif/pasif durumuna gore sprite tinting yapar (grileşir ya da beyazlar)
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Crystal : MonoBehaviour
    {
        [Header("Kristal Ayarlari")]
        [Tooltip("Bu kristal hangi rengi bekliyor.")]
        [SerializeField] private LightColorData requiredColor;

        [Tooltip("Collider yaricapi (grid hucresine gore kucuk olmali).")]
        [SerializeField] private float colliderRadius = 0.3f;

        [Header("Aktif/Pasif Goruntu")]
        [Tooltip("Pasif haldeyken sprite'in tonu (gri/sonuk).")]
        [SerializeField] private Color inactiveTint = new Color(0.4f, 0.4f, 0.4f, 1f);

        [Tooltip("Aktif haldeyken sprite'in tonu (beyaz = kendi rengi).")]
        [SerializeField] private Color activeTint = Color.white;

        [Header("Debug")]
        [Tooltip("Scene view'da gizmo ciz. Sprite kullanildiktan sonra kapatilabilir.")]
        [SerializeField] private bool showGizmo = false;

        public bool IsActivated { get; private set; }
        public LightColorData RequiredColor => requiredColor;

        // bu frame'de bir isin gelip gelmedigini takip eder
        // BeamManager once Deactivate cagiriyor (flag false), sonra isin gelirse ReceiveLight
        // flag true yapiyor. Tum isinlar islendikten sonra ResolvePendingState calisiyor:
        //   - flag true ise zaten dogru durumda, dokunma
        //   - flag false ise hic isin gelmedi demek, kristali pasif yap
        // bu yapi sayesinde her recompute'ta "yeniden yandi" gibi davranmiyoruz (animasyon/ses spam yok)
        private bool receivedBeamThisFrame = false;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            // collider config'i lokal degisken ile yapip biraktik, field tutmaya gerek yok
            var col = GetComponent<CircleCollider2D>();
            col.radius = colliderRadius;
            col.isTrigger = false;

            spriteRenderer = GetComponent<SpriteRenderer>();

            // baslangicta pasif goruntu
            UpdateTint();

            // event-driven sistem: BeamManager bizi tani diye kendimizi register ederiz
            if (BeamManager.Instance != null) BeamManager.Instance.RegisterCrystal(this);
        }

        private void OnDestroy()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.UnregisterCrystal(this);

            // aktif tween'leri durdur (Pulse animasyonu ve sprite color tween'leri)
            // level reload sirasinda DOTween "target null" uyarisi vermesin diye
            transform.DOKill();
            if (spriteRenderer != null) spriteRenderer.DOKill();
        }

        // LevelLoader runtime'da spawn ederken cagirir
        public void SetRequiredColor(LightColorData color)
        {
            requiredColor = color;

            // pasif state'e komple reset (yeni level basliyor)
            IsActivated = false;
            receivedBeamThisFrame = false;

            // renk asset'inde ozel kristal sprite varsa onu kullan
            // (orn. yesil kristal icin Color_Green.crystalSprite atanmis olabilir)
            if (color != null && color.crystalSprite != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = color.crystalSprite;
            }

            UpdateTint();
        }

        // isin kristale geldi, renk dogru mu kontrol et
        // BeamManager her recompute'ta once Deactivate sonra ReceiveLight cagiriyor.
        // Sadece PASIFTEN AKTIFE gecerken animasyon ve ses tetiklenir.
        public void ReceiveLight(LightColor incomingColor)
        {
            if (requiredColor == null) return;

            // bu frame'de en az bir isin geldi flag'i (renk dogru olsun olmasin)
            // ResolvePendingState bu flag'e gore "isin yok = pasif" karari verir
            receivedBeamThisFrame = true;

            bool isCorrectColor = incomingColor == requiredColor.color;
            bool wasActivated = IsActivated;
            IsActivated = isCorrectColor;

            // durum degistiyse sprite rengini guncelle
            if (IsActivated != wasActivated)
            {
                UpdateTint();

                // pasiften aktife geciyorsa pulse + ses (sadece bir kere)
                if (IsActivated)
                {
                    AnimationHelper.Pulse(transform);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.CrystalActivate);
                }
            }
        }

        // BeamManager her recompute'un BASINDA cagirir, frame state'ini sifirlar
        // IsActivated'a DOKUNMUYORUZ, ReceiveLight gelmeyince ResolvePendingState halledecek
        public void Deactivate()
        {
            receivedBeamThisFrame = false;
        }

        // BeamManager butun isinlari isledikten SONRA cagirir
        // bu frame hic isin gelmediyse kristal pasif olur, geldiyse zaten dogru durumda
        public void ResolvePendingState()
        {
            if (!receivedBeamThisFrame && IsActivated)
            {
                IsActivated = false;
                UpdateTint();
            }
        }

        // duruma gore sprite rengini ayarla
        private void UpdateTint()
        {
            if (spriteRenderer == null) return;
            spriteRenderer.color = IsActivated ? activeTint : inactiveTint;
        }

        // opsiyonel debug gorunumu, sprite geldiginde kapatilabilir
        private void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Color c = requiredColor != null ? requiredColor.displayColor : Color.white;
            Gizmos.color = c;

            if (Application.isPlaying && IsActivated)
                Gizmos.DrawSphere(transform.position, colliderRadius);
            else
                Gizmos.DrawWireSphere(transform.position, colliderRadius);
        }
    }
}
