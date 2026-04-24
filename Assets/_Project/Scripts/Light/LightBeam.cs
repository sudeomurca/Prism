using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // isinin cizilmesini yonetir
    // raycast ile aynayi bulur, Reflect() cagirir, yonu degistirir.
    [RequireComponent(typeof(LineRenderer))]
    public class LightBeam : MonoBehaviour
    {
        [Header("Isin Ayarlari")]
        [Tooltip("Her segmentin maksimum uzunlugu.")]
        [SerializeField] private float maxSegmentLength = 15f;

        [Tooltip("Isinin kalinligi.")]
        [SerializeField] private float width = 0.01f;

        [Tooltip("Max yansima sayisi. Sonsuz dongude donmesin diye.")]
        [SerializeField] private int maxReflections = 10;

        // aynadan cikarken raycast'i bu kadar ileri baslatiriz ki
        // ayni aynayi tekrar algilamayalim (sonsuz dongu koruma)
        private const float RAY_OFFSET = 0.1f;

        private LineRenderer lineRenderer;
        private LightSource source;

        // kirilma noktalari list ile tutulur, lineRenderer'a verilir
        private readonly List<Vector3> beamPoints = new List<Vector3>(16);

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            source = GetComponent<LightSource>();
            SetupLineRenderer();
        }

        private void Start()
        {
            DrawBeam();
        }

        private void SetupLineRenderer()
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.useWorldSpace = true;

            if (source != null && source.ColorData != null)
            {
                Color c = source.ColorData.displayColor;
                lineRenderer.startColor = c;
                lineRenderer.endColor = c;
            }
        }

        // isini cizdirmek icin kaynaktan basla, her ayna carpisinda yon degistir, devam et.
        private void DrawBeam()
        {
            if (source == null) return;

            beamPoints.Clear();

            Vector3 currentPos = source.Position;
            Direction currentDir = source.Direction;

            // ilk nokta,kaynagin pozisyonu
            beamPoints.Add(currentPos);

            for (int i = 0; i < maxReflections; i++)
            {
                Vector2 dirVec = currentDir.ToVector();

                // raycast baslangicini biraz ileri kaydir (ayni collider'i tekrar algilamayi engeller)
                Vector2 rayStart = (Vector2)currentPos + dirVec * RAY_OFFSET;
                RaycastHit2D hit = Physics2D.Raycast(rayStart, dirVec, maxSegmentLength);

                if (hit.collider != null)
                {
                    Mirror mirror = hit.collider.GetComponent<Mirror>();

                    if (mirror != null)
                    {
                        // aynaya kadar nokta ekle
                        beamPoints.Add(hit.point);

                        // yonu degistir, yeni pozisyondan devam et
                        currentDir = mirror.Reflect(currentDir);
                        currentPos = hit.collider.transform.position;
                        continue;
                    }

                    // kristal mi?
                    Crystal crystal = hit.collider.GetComponent<Crystal>();
                    if (crystal != null && source.ColorData != null)
                    {
                        // kristale isigi ver, rengi kontrol etsin
                        crystal.ReceiveLight(source.ColorData.color);
                    }

                    // ayna disinda bir seye carptiysa orada bitir
                    beamPoints.Add(hit.point);
                    break;
                }

                // hicbir seye carpmazsa max mesafeye kadar ciz,bitir
                Vector3 endPoint = currentPos + (Vector3)(dirVec * maxSegmentLength);
                beamPoints.Add(endPoint);
                break;
            }

            // lineRenderer'a noktalari ver
            lineRenderer.positionCount = beamPoints.Count;
            for (int i = 0; i < beamPoints.Count; i++)
            {
                lineRenderer.SetPosition(i, beamPoints[i]);
            }
        }
    }
}
