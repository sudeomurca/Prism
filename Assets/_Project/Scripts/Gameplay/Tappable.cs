using UnityEngine;

namespace Prism
{
    //onmousedown eventi ile mirror class ontap metodunu bagla
    [RequireComponent(typeof(Collider2D))]
    public class Tappable : MonoBehaviour
    {
        private Mirror mirror;

        private void Awake()
        {
            mirror = GetComponent<Mirror>();
        }

        // Unity'nin OnMouseDown event'i, collider'a tiklayinca otomatik tetiklenir
        private void OnMouseDown()
        {
            if (mirror != null) mirror.OnTap();
        }
    }
}
