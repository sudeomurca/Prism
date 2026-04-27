using UnityEngine;

namespace Prism
{
    // sahnedeki parcalar uzerine eklenir, fare tikinda Mirror.OnTap() cagirir
    // Unity'nin OnMouseDown event'i Collider2D olan herhangi bir GameObject'te calisir
    [RequireComponent(typeof(Collider2D))]
    public class Tappable : MonoBehaviour
    {
        // tap olunca tetiklenecek hedef component referansi
        // boylece ayna disinda baska seyleri de tappable yapabiliriz ileride
        private Mirror mirror;

        private void Awake()
        {
            mirror = GetComponent<Mirror>();
        }

        // unity OnMouseDown event ,collider'a tiklayinca otomatik tetiklenir
        private void OnMouseDown()
        {
            if (mirror != null)
            {
                mirror.OnTap();
            }
        }
    }
}
