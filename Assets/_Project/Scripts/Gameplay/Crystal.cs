using UnityEngine;

namespace Prism
{
    // belirli bir renk beklenir, isin o renkle carparsa aktiflesir
    [RequireComponent(typeof(CircleCollider2D))]
    public class Crystal : MonoBehaviour
    {
        [Header("Kristal Ayarlari")]
        [Tooltip("Bu kristal hangi rengi bekliyor.")]
        [SerializeField] private LightColorData requiredColor;

        [Tooltip("Collider yaricapi (grid hucresine gore kucuk olmali).")]
        [SerializeField] private float colliderRadius = 0.3f;

        
        public bool IsActivated { get; private set; }

        public LightColorData RequiredColor => requiredColor;

        private CircleCollider2D circleCollider;

        private void Awake()
        {
            circleCollider = GetComponent<CircleCollider2D>();
            circleCollider.radius = colliderRadius;
            circleCollider.isTrigger = false; // raycast gorebilsin diye trigger degil
        }

        // isin kristale geldi, renk dogru mu kontrol et
        public void ReceiveLight(LightColor incomingColor)
        {
            if (requiredColor == null) return;

            bool wasActivated = IsActivated;
            IsActivated = incomingColor == requiredColor.color;

            // yeni aktiflendiyse log
            if (IsActivated && !wasActivated)
            {
                Debug.Log($"Kristal aktif! Renk: {incomingColor}");
            }
        }

        // isin gelmeyi kesti , aktivasyonu sifirla
        public void Deactivate()
        {
            IsActivated = false;
        }

        private void OnDrawGizmos()
        {
            Color c = requiredColor != null ? requiredColor.displayColor : Color.white;

            // aktif ise dolu kure, degilse sadece cerceve
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
