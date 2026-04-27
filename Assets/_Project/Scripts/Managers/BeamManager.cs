using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // sahnedeki isin sistemini yonetir, EVENT-DRIVEN calisir
    // her frame raycast YAPMAZ, sadece bir sey degistiginde (tap, level load) yeniden hesaplar
    //
    // mimari: Mirror, LightSource, LightPrism, Crystal kendileri Awake'te buraya Register olur
    // FindObjectsByType yok, runtime'da yeni obje eklenebilir, baglanti otomatik
    //
    // PERFORMANCE: prizma cikis LineRenderer'lari ObjectPool'dan geliyor, Instantiate/Destroy yok
    // mobile build'de GC tetikleyen alloc'larin onune gecer
    public class BeamManager : MonoBehaviour
    {
        public static BeamManager Instance { get; private set; }

        [Header("Isin Ayarlari")]
        [SerializeField] private float prismOutputLength = 20f;
        [SerializeField] private float prismOutputWidth = 0.15f;
        [SerializeField] private int maxPrismHops = 5;

        [Header("Material")]
        [Tooltip("Prizma cikisi icin kullanilacak material. Bos birakilirsa runtime'da uretilir.")]
        [SerializeField] private Material beamMaterial;

        [Header("Object Pool")]
        [Tooltip("Prizma cikis isinlari icin onceden hazirlanacak LineRenderer sayisi.")]
        [SerializeField] private int initialPrismLinePoolSize = 4;

        // sahnedeki tum parcalar, kendileri register oluyor
        private readonly List<LightSource> sources  = new();
        private readonly List<LightPrism>  prisms   = new();
        private readonly List<Crystal>     crystals = new();
        private readonly List<LightBeam>   beams    = new();

        // her prizmaya hangi LineRenderer atandi, takip et (release icin gerekli)
        private readonly Dictionary<LightPrism, LineRenderer> prismOutputLines = new();

        // prizma cikis LineRenderer'lari icin generic pool
        // tek seferlik kurulur, level boyunca yeniden kullanilir
        private ObjectPool<LineRenderer> prismLinePool;

        // pool'un parent'i, hierarchy temiz kalsin diye
        private Transform poolParent;

        // geri kullanilabilir liste, allocation olmasin diye
        private readonly List<Vector3> tempPoints = new(16);

        // ilk frame'de henuz kimse register olmamis olabilir,
        // o yuzden Start'ta bir kere otomatik recompute yapariz
        private bool initialComputeDone = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            EnsureMaterial();
            InitPrismLinePool();

            // sahnedeki herkes kendini register etti, simdi ilk hesaplamayi yap
            // FALLBACK: birisi Instance henuz yokken Awake'te kosmus olabilir,
            // bu durumda kayit kacirmis olur. Garanti olarak sahneyi tarayip eksikleri ekleyelim.
            CollectMissingObjectsFromScene();

            initialComputeDone = true;
            RecomputeBeams();
        }

        // sahnede register olmamis parcalari yakalar (Awake sirasi yanlis gittiyse koruma)
        private void CollectMissingObjectsFromScene()
        {
            foreach (var s in FindObjectsByType<LightSource>(FindObjectsSortMode.None)) RegisterSource(s);
            foreach (var b in FindObjectsByType<LightBeam>(FindObjectsSortMode.None))   RegisterBeam(b);
            foreach (var c in FindObjectsByType<Crystal>(FindObjectsSortMode.None))     RegisterCrystal(c);
            foreach (var p in FindObjectsByType<LightPrism>(FindObjectsSortMode.None))  RegisterPrism(p);
        }

        // inspector'dan material atanmamissa runtime'da uret
        private void EnsureMaterial()
        {
            if (beamMaterial != null) return;

            Shader sh = Shader.Find("Sprites/Default");
            if (sh != null)
            {
                beamMaterial = new Material(sh);
            }
            else
            {
                Debug.LogWarning("[BeamManager] Sprites/Default shader bulunamadi.");
            }
        }

        // pool icin LineRenderer prefab'i runtime'da uretilir
        // Inspector'dan prefab istemiyoruz, kurulumu kolaylastirmak icin
        private void InitPrismLinePool()
        {
            // pool icindeki nesnelerin parent'i, hierarchy duzeni icin
            poolParent = new GameObject("[BeamLinePool]").transform;
            poolParent.SetParent(transform);

            // template GameObject yarat, pool bunu klonlayacak
            GameObject template = new GameObject("PrismOutputLine");
            LineRenderer templateLR = template.AddComponent<LineRenderer>();
            templateLR.startWidth = prismOutputWidth;
            templateLR.endWidth = prismOutputWidth;
            templateLR.useWorldSpace = true;
            templateLR.positionCount = 0;
            templateLR.sortingOrder = 2;
            if (beamMaterial != null) templateLR.material = beamMaterial;

            // template inactive bekler, pool ondan klonlayacak
            template.SetActive(false);
            template.transform.SetParent(poolParent);

            prismLinePool = new ObjectPool<LineRenderer>(
                prefab: templateLR,
                parent: poolParent,
                initialSize: initialPrismLinePoolSize
            );
        }

        // ---- REGISTRATION API ----
        // her parca kendi Awake'inde buraya kaydolur

        public void RegisterSource(LightSource s)  { if (!sources.Contains(s))  sources.Add(s); }
        public void RegisterBeam(LightBeam b)      { if (!beams.Contains(b))    beams.Add(b); }
        public void RegisterCrystal(Crystal c)     { if (!crystals.Contains(c)) crystals.Add(c); }

        public void RegisterPrism(LightPrism p)
        {
            if (prisms.Contains(p)) return;
            prisms.Add(p);
            // pool henuz kurulmamissa Start sonrasi register olunca SetupPrismOutputLine cagrilir
            if (prismLinePool != null) AssignLineToPrism(p);
        }

        // unregister: obje destroy olunca cagirilir
        public void UnregisterSource(LightSource s)  => sources.Remove(s);
        public void UnregisterBeam(LightBeam b)      => beams.Remove(b);
        public void UnregisterCrystal(Crystal c)     => crystals.Remove(c);

        public void UnregisterPrism(LightPrism p)
        {
            prisms.Remove(p);
            // LineRenderer'i destroy ETMIYORUZ, pool'a geri veriyoruz
            if (prismOutputLines.TryGetValue(p, out LineRenderer lr) && lr != null)
            {
                lr.positionCount = 0; // temizle
                prismLinePool.Release(lr);
            }
            prismOutputLines.Remove(p);
        }

        // ---- PUBLIC API ----
        // herhangi bir parca degistiginde (tap, level load) burayi cagir
        // bu butun isin sistemini yeniden hesaplar
        public void RecomputeBeams()
        {
            // henuz Start cagrilmadi ise material yok, bekle (Mirror Awake'te tetiklerse)
            if (!initialComputeDone) return;

            ResetState();

            // Start sonrasi register olmus prizmalar icin line eksikligi olabilir, kontrol et
            foreach (var prism in prisms)
            {
                if (prism != null && !prismOutputLines.ContainsKey(prism))
                {
                    AssignLineToPrism(prism);
                }
            }

            // tum kaynaklar isinlarini cizsin
            foreach (var beam in beams)
            {
                if (beam != null) beam.DrawBeam();
            }

            // prizmalardan cikan ikinci kademe isinlari cizsin
            ProcessPrismOutputs();

            // tum kristaller aktif mi diye kontrol et
            CheckWinCondition();
        }

        // ---- INTERNAL ----

        // bir prizma icin pool'dan LineRenderer al, prizma child'i yap
        private void AssignLineToPrism(LightPrism prism)
        {
            LineRenderer lr = prismLinePool.Get();
            lr.transform.SetParent(prism.transform);
            lr.transform.localPosition = Vector3.zero;
            lr.positionCount = 0;
            prismOutputLines[prism] = lr;
        }

        private void ResetState()
        {
            foreach (var prism in prisms)
            {
                if (prism == null) continue;
                prism.ResetFrame();

                if (prismOutputLines.TryGetValue(prism, out LineRenderer lr) && lr != null)
                {
                    lr.positionCount = 0;
                }
            }

            foreach (var crystal in crystals)
            {
                if (crystal != null) crystal.Deactivate();
            }
        }

        private void ProcessPrismOutputs()
        {
            foreach (var prism in prisms)
            {
                if (prism == null) continue;
                if (!prism.HasInput()) continue;
                if (!prism.HasValidOutput()) continue; // yanlis renk gelmis, tikanir

                LightColor outputColor = prism.GetOutputColor();
                Collider2D prismCollider = prism.GetComponent<Collider2D>();

                BeamTracer.Trace(
                    startPos: prism.Position,
                    startDir: prism.OutputDirection,
                    color: outputColor,
                    maxReflections: maxPrismHops,
                    maxSegmentLength: prismOutputLength,
                    rayOffset: 0.1f,
                    outPoints: tempPoints,
                    ignoreCollider: prismCollider
                );

                if (prismOutputLines.TryGetValue(prism, out LineRenderer lr) && lr != null)
                {
                    lr.positionCount = tempPoints.Count;
                    for (int i = 0; i < tempPoints.Count; i++)
                    {
                        lr.SetPosition(i, tempPoints[i]);
                    }

                    // renk direkt prizmanin kendi ScriptableObject'inden geliyor
                    lr.startColor = prism.DisplayColor;
                    lr.endColor = prism.DisplayColor;
                }
            }
        }

        private void CheckWinCondition()
        {
            if (GameManager.Instance == null) return;
            if (GameManager.Instance.CurrentState == GameState.LevelComplete) return;
            if (crystals.Count == 0) return;

            foreach (var crystal in crystals)
            {
                if (crystal == null || !crystal.IsActivated) return;
            }

            GameManager.Instance.CompleteLevel();
        }

        private void OnDestroy()
        {
            // pool'u temizle, sahne kapanirken
            prismLinePool?.Clear();
        }
    }
}
