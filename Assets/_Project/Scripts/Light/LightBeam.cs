using UnityEngine;

namespace Prism
{
    // Bir Light Source'un uzantisi — isinin gorsel olarak cizilmesini saglar.
    // LineRenderer kullanir, her frame yerine sadece gerektiginde gunceller.
    [RequireComponent(typeof(LineRenderer))]
    public class LightBeam : MonoBehaviour
    {
        [Header("Isin Ayarlari")]
        [Tooltip("Isinin maksimum gidebilecegi uzunluk (dunya birimi).")]
        [SerializeField] private float maxLength = 20f;

        [Tooltip("Isinin kalinligi.")]
        [SerializeField] private float width = 0.15f;

        private LineRenderer lineRenderer;
        private LightSource source;

        private void Awake()
        {
            // GetComponent pahali, Awake'de bir kere al, cache'le.
            lineRenderer = GetComponent<LineRenderer>();
            source = GetComponent<LightSource>();

            SetupLineRenderer();
        }

        private void Start()
        {
            DrawBeam();
        }

        // LineRenderer'in baslangic ayarlari.
        private void SetupLineRenderer()
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;

            // Renk kaynagin renginden gelir.
            if (source != null && source.ColorData != null)
            {
                Color c = source.ColorData.displayColor;
                lineRenderer.startColor = c;
                lineRenderer.endColor = c;
            }
        }

        // Isini ciz: kaynaktan yonun dogrultusunda maxLength kadar git.
        private void DrawBeam()
        {
            if (source == null) return;

            Vector3 start = source.Position;
            Vector3 dir = (Vector3)source.Direction.ToVector();
            Vector3 end = start + dir * maxLength;

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
    }
}
