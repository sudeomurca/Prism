using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // isinin cizilmesini yonetir, raycast ile ayna/prizma/kristal bulur
    // NOT: artik kendi Start() icinde cizmiyor, BeamManager her frame cagiriyor
    [RequireComponent(typeof(LineRenderer))]
    public class LightBeam : MonoBehaviour
    {
        [Header("Isin Ayarlari")]
        [Tooltip("Her segmentin maksimum uzunlugu.")]
        [SerializeField] private float maxSegmentLength = 20f;

        [Tooltip("Isinin kalinligi.")]
        [SerializeField] private float width = 0.15f;

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

            // event-driven sistem: BeamManager bizi tani diye kendimizi register ederiz
            if (BeamManager.Instance != null) BeamManager.Instance.RegisterBeam(this);
        }

        private void OnDestroy()
        {
            if (BeamManager.Instance != null) BeamManager.Instance.UnregisterBeam(this);
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

        // artik BeamManager bu fonksiyonu cagiriyor
        public void DrawBeam()
        {
            if (source == null || source.ColorData == null) return;

            // once kendi rengini tazele (editor'da degisirse de hemen gozuksun)
            Color c = source.ColorData.displayColor;
            lineRenderer.startColor = c;
            lineRenderer.endColor = c;

            // isini cizmek icin ortak fonksiyonu cagiriyor
            BeamTracer.Trace(
                startPos: source.Position,
                startDir: source.Direction,
                color: source.ColorData.color,
                maxReflections: maxReflections,
                maxSegmentLength: maxSegmentLength,
                rayOffset: RAY_OFFSET,
                outPoints: beamPoints
            );

            // lineRenderer'a noktalari ver
            lineRenderer.positionCount = beamPoints.Count;
            for (int i = 0; i < beamPoints.Count; i++)
            {
                lineRenderer.SetPosition(i, beamPoints[i]);
            }
        }
    }
}
