using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // LevelData'yi alir, sahnede prefab'lardan instance'lar yaratir
    // eski level'i temizler, yeni level'i kurar
    //
    // GameManager bunu cagirir, BeamManager otomatik olarak self-registration ile dolar
    public class LevelLoader : MonoBehaviour
    {
        public static LevelLoader Instance { get; private set; }

        [Header("Prefab Referanslari")]
        [SerializeField] private LightSource lightSourcePrefab;
        [SerializeField] private Mirror      mirrorPrefab;
        [SerializeField] private Crystal     crystalPrefab;
        [SerializeField] private LightPrism  prismPrefab;

        [Header("Sahne Hierarchy")]
        [Tooltip("Yaratilan parcalar bu transform'un altina gelir. Hierarchy temiz kalsin diye.")]
        [SerializeField] private Transform levelContainer;

        // mevcut level'in objelerini takip et, sonra temizleyebilelim
        private readonly List<GameObject> spawnedObjects = new();

        // su an yuklu olan level
        public LevelData CurrentLevel { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // levelContainer atanmamissa kendi transform'umuzu kullan
            if (levelContainer == null) levelContainer = transform;
        }

        // ---- PUBLIC API ----

        // bir level'i sahnede kur
        // mevcut level varsa once temizler
        public void LoadLevel(LevelData data)
        {
            if (data == null)
            {
                Debug.LogError("[LevelLoader] LoadLevel: data null.");
                return;
            }

            ClearLevel();
            CurrentLevel = data;
            BuildLevel(data);

            // tum parcalar spawn olduktan sonra BeamManager'a recompute de
            // (parcalar Awake'te kendi kendilerine register oluyor zaten)
            if (BeamManager.Instance != null) BeamManager.Instance.RecomputeBeams();
        }

        // mevcut level'i temizle, yeniden yukle (restart)
        public void ReloadCurrentLevel()
        {
            if (CurrentLevel != null) LoadLevel(CurrentLevel);
        }

        // sahnedeki tum spawn edilmis parcalari yok et
        public void ClearLevel()
        {
            foreach (var go in spawnedObjects)
            {
                if (go != null) Destroy(go);
            }
            spawnedObjects.Clear();
            CurrentLevel = null;
        }

        // ---- INTERNAL ----

        private void BuildLevel(LevelData data)
        {
            // her prefab kendi listesini spawn ederken kontrol edilir
            // boylece bir prefab eksik olsa bile (ornek prizma henuz yok) hata vermez,
            // sadece o tipteki parcalar olusmaz
            if (data.lightSources.Count > 0) RequireOrWarn(lightSourcePrefab, "LightSource");
            if (data.mirrors.Count > 0)      RequireOrWarn(mirrorPrefab,      "Mirror");
            if (data.crystals.Count > 0)     RequireOrWarn(crystalPrefab,     "Crystal");
            if (data.prisms.Count > 0)       RequireOrWarn(prismPrefab,       "LightPrism");

            if (lightSourcePrefab != null) foreach (var cfg in data.lightSources) SpawnLightSource(cfg);
            if (mirrorPrefab      != null) foreach (var cfg in data.mirrors)      SpawnMirror(cfg);
            if (crystalPrefab     != null) foreach (var cfg in data.crystals)     SpawnCrystal(cfg);
            if (prismPrefab       != null) foreach (var cfg in data.prisms)       SpawnPrism(cfg);
        }

        // bir prefab eksikse uyari ver, ama hata atip durdurma
        private void RequireOrWarn(Component prefab, string label)
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[LevelLoader] {label} prefab atanmamis ama level icinde var. Bu parcalar yuklenmedi.");
            }
        }

        private void SpawnLightSource(LightSourceConfig cfg)
        {
            LightSource instance = Instantiate(
                lightSourcePrefab,
                cfg.position,
                Quaternion.identity,
                levelContainer
            );
            instance.SetConfig(cfg.colorData, cfg.direction);
            spawnedObjects.Add(instance.gameObject);
            AnimationHelper.SpawnPop(instance.transform, delay: GetSpawnDelay());
        }

        private void SpawnMirror(MirrorConfig cfg)
        {
            Mirror instance = Instantiate(
                mirrorPrefab,
                cfg.position,
                Quaternion.identity,
                levelContainer
            );
            instance.SetRotationIndex(cfg.rotationIndex);
            spawnedObjects.Add(instance.gameObject);
            AnimationHelper.SpawnPop(instance.transform, delay: GetSpawnDelay());
        }

        private void SpawnCrystal(CrystalConfig cfg)
        {
            Crystal instance = Instantiate(
                crystalPrefab,
                cfg.position,
                Quaternion.identity,
                levelContainer
            );
            instance.SetRequiredColor(cfg.requiredColor);
            spawnedObjects.Add(instance.gameObject);
            AnimationHelper.SpawnPop(instance.transform, delay: GetSpawnDelay());
        }

        private void SpawnPrism(PrismConfig cfg)
        {
            LightPrism instance = Instantiate(
                prismPrefab,
                cfg.position,
                Quaternion.identity,
                levelContainer
            );
            instance.SetConfig(cfg.outputColorData, cfg.outputDirection);
            spawnedObjects.Add(instance.gameObject);
            AnimationHelper.SpawnPop(instance.transform, delay: GetSpawnDelay());
        }

        // her parca biraz daha gecikmeli spawn olur, satisfying "sirayla canlanma" hissi verir
        // toplam spawn sayisina gore hesaplanir, ilk parca delay'siz baslar
        private float GetSpawnDelay()
        {
            return spawnedObjects.Count * 0.05f;
        }
    }
}
