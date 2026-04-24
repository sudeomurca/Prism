using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // her frame sahnedeki isin sistemini yonetir
    // akis: once hepsini resetle, sonra kaynaklari izle, sonra prizma cikislarini izle
    public class BeamManager : MonoBehaviour
    {
        [Header("Isin Ayarlari")]
        [SerializeField] private float prismOutputLength = 20f;
        [SerializeField] private float prismOutputWidth = 0.15f;
        [SerializeField] private int maxPrismHops = 5;

        [Header("Material")]
        [Tooltip("Prizma cikisi icin kullanilacak material. Bos ise sahnedeki LightSource'tan kopyalar.")]
        [SerializeField] private Material beamMaterial;

        // sahnedeki tum parcalar,sahne yuklenince bulur, her frame arama yapmaz
        private LightSource[] sources;
        private LightPrism[] prisms;
        private Crystal[] crystals;
        private LightBeam[] beams;

        // prizma cikis isinlarini cizmek icin LineRenderer havuzu
        private readonly Dictionary<LightPrism, LineRenderer> prismOutputLines = new();

        // geri kullanilabilir liste,her frame yeni liste allocate etmemek icin
        private readonly List<Vector3> tempPoints = new List<Vector3>(16);

        private void Start()
        {
            // sahnedeki tum ilgili objeleri bir defa bul, cache'le
            sources = FindObjectsByType<LightSource>(FindObjectsSortMode.None);
            prisms = FindObjectsByType<LightPrism>(FindObjectsSortMode.None);
            crystals = FindObjectsByType<Crystal>(FindObjectsSortMode.None);
            beams = FindObjectsByType<LightBeam>(FindObjectsSortMode.None);

            // eger inspector'dan material atanmamissa,sahnedeki ilk LightBeam'den kopyala
            // boylece URP/built-in uyumsuzluk sorunu olmaz
            if (beamMaterial == null && beams.Length > 0)
            {
                LineRenderer firstLR = beams[0].GetComponent<LineRenderer>();
                if (firstLR != null && firstLR.sharedMaterial != null)
                {
                    beamMaterial = firstLR.sharedMaterial;
                }
            }

            // her prizma icin bir cikis line renderer hazirla
            foreach (var prism in prisms)
            {
                SetupPrismOutputLine(prism);
            }
        }

        // her prizma icin ayri bir GameObject + LineRenderer yarat
        private void SetupPrismOutputLine(LightPrism prism)
        {
            GameObject lineGO = new GameObject("PrismOutput");
            lineGO.transform.SetParent(prism.transform);

            LineRenderer lr = lineGO.AddComponent<LineRenderer>();
            lr.startWidth = prismOutputWidth;
            lr.endWidth = prismOutputWidth;
            lr.useWorldSpace = true;
            lr.positionCount = 0;

            // LightBeam'den aldigimiz materyali kullan
            if (beamMaterial != null)
            {
                lr.material = beamMaterial;
            }

            prismOutputLines[prism] = lr;
        }

        // her frame yeniden hesapla
        private void LateUpdate()
        {
            ResetAll();

            foreach (var beam in beams)
            {
                beam.DrawBeam();
            }

            ProcessPrismOutputs();
        }

        private void ResetAll()
        {
            foreach (var prism in prisms)
            {
                prism.ResetFrame();

                if (prismOutputLines.TryGetValue(prism, out LineRenderer lr))
                {
                    lr.positionCount = 0;
                }
            }

            foreach (var crystal in crystals)
            {
                crystal.Deactivate();
            }
        }

        // prizmalardan cikan isinlari izle
        private void ProcessPrismOutputs()
        {
            foreach (var prism in prisms)
            {
                if (!prism.HasInput()) continue;

                LightColor outputColor = prism.GetOutputColor();

                // prizma cikisi icin daha buyuk offset: cikis isini kendi collider'indan cikabilsin
                // prizma yariapi 0.3, ekstra pay ile 0.5 offset guvenli
                BeamTracer.Trace(
                    startPos: prism.Position,
                    startDir: prism.OutputDirection,
                    color: outputColor,
                    maxReflections: maxPrismHops,
                    maxSegmentLength: prismOutputLength,
                    rayOffset: 0.5f,
                    outPoints: tempPoints
                );

                if (prismOutputLines.TryGetValue(prism, out LineRenderer lr))
                {
                    lr.positionCount = tempPoints.Count;
                    for (int i = 0; i < tempPoints.Count; i++)
                    {
                        lr.SetPosition(i, tempPoints[i]);
                    }

                    Color displayColor = GetColorForLightColor(outputColor);
                    lr.startColor = displayColor;
                    lr.endColor = displayColor;
                }
            }
        }

        // basit LightColor -> Color donusumu
        private Color GetColorForLightColor(LightColor lc) => lc switch
        {
            LightColor.Red     => Color.red,
            LightColor.Green   => Color.green,
            LightColor.Blue    => Color.blue,
            LightColor.Yellow  => Color.yellow,
            LightColor.Magenta => Color.magenta,
            LightColor.Cyan    => Color.cyan,
            LightColor.White   => Color.white,
            _                  => Color.gray
        };
    }
}
