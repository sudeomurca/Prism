using UnityEngine;

namespace Prism
{
    
    // farkli yonlerden gelen isiklari toplar renk karisimi yapip tek cikisa gonder
    [RequireComponent(typeof(CircleCollider2D))]
    public class LightPrism : MonoBehaviour
    {
        [Header("Prizma Ayarlari")]
        [Tooltip("Karisim isigi hangi yone gonderilecek.")]
        [SerializeField] private Direction outputDirection = Direction.Up;

        [Tooltip("Collider yaricapi.")]
        [SerializeField] private float colliderRadius = 0.3f;

        public Direction OutputDirection => outputDirection;
        public Vector3 Position => transform.position;

        // bu frame'de gelen isiklar
        // frame basinda resetlenir,isik geldikce accumulate olur
        private LightColor accumulatedColor = LightColor.None;
        private int lightsReceivedThisFrame = 0;

        private CircleCollider2D circleCollider;

        private void Awake()
        {
            circleCollider = GetComponent<CircleCollider2D>();
            circleCollider.radius = colliderRadius;
        }

        // LightBeam bu fonksiyonu cagirir, prizmaya isik gelince
        // tek bir renkse olduğu gibi kaydet , ikinci bir renk gelirse mix
        public void ReceiveLight(LightColor incoming)
        {
            if (lightsReceivedThisFrame == 0)
            {
                accumulatedColor = incoming;
            }
            else
            {
                // ikinci isik geldi,karistir
                accumulatedColor = LightColorData.Mix(accumulatedColor, incoming);
            }

            lightsReceivedThisFrame++;
        }

        // prizmanin su anki cikis rengi
        public LightColor GetOutputColor()
        {
            return accumulatedColor;
        }

        // hiç isin geliyor mu,min 1
        public bool HasInput()
        {
            return lightsReceivedThisFrame > 0;
        }

        // frame basinda resetle
        public void ResetFrame()
        {
            accumulatedColor = LightColor.None;
            lightsReceivedThisFrame = 0;
        }

        
        private void OnDrawGizmos()
        {
            
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, colliderRadius);

            // cikis yonunu ok gibi goster
            Vector3 dirVec = outputDirection.ToVector();
            Gizmos.DrawLine(transform.position, transform.position + dirVec * 0.5f);
        }
    }
}
