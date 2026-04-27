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

        private CircleCollider2D circleCollider;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            circleCollider = GetComponent<CircleCollider2D>();
            circleCollider.radius = colliderRadius;
            circleCollider.isTrigger = false;

            spriteRenderer = GetComponent<SpriteRenderer>();

            // baslangicta pasif goruntu
            UpdateTint();

            // event-driven sistem: BeamManager bizi tani diye kendimizi register ederiz
            if (BeamManager.Instance != null) BeamManager.Instance.RegisterCrystal(this);
        }

        private void OnDestroy()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.UnregisterCrystal(this);
        }

        // LevelLoader runtime'da spawn ederken cagirir
        public void SetRequiredColor(LightColorData color)
        {
            requiredColor = color;
            // pasif tona reset et
            IsActivated = false;
            UpdateTint();
        }

        // isin kristale geldi, renk dogru mu kontrol et
        public void ReceiveLight(LightColor incomingColor)
        {
            if (requiredColor == null) return;

            bool wasActivated = IsActivated;
            IsActivated = incomingColor == requiredColor.color;

            // durum degistiyse sprite rengini guncelle
            if (IsActivated != wasActivated)
            {
                UpdateTint();

                // pasiften aktife geciyorsa pulse animasyonu, dikkat cekmek icin
                if (IsActivated)
                {
                    AnimationHelper.Pulse(transform);
                }
            }
        }

        // isin gelmeyi kesti , aktivasyonu sifirla
        public void Deactivate()
        {
            if (IsActivated)
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

        // opsiyonel debug gorunumu,sprite geldiginde kapatilabilir
        private void OnDrawGizmos()
        {
            if (!showGizmo) return;

            Color c = requiredColor != null ? requiredColor.displayColor : Color.white;

            if (Application.isPlaying && IsActivated)
            {
                Gizmos.color = c;
                Gizmos.DrawSphere(transform.position, colliderRadius);
            }
            else
            {
                Gizmos.color = c;
                Gizmos.DrawWireSphere(transform.position, colliderRadius);
            }
        }
    }
}
