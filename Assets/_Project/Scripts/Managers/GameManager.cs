using UnityEngine;

namespace Prism
{
    // oyunun genel durumunu yoneten merkezi class
    // baska scriptler GameManager.Instance uzerinden erisir
    //
    // sorumluluklari:
    //   - oyun state machine (Playing / LevelComplete / Paused)
    //   - level yukleme, ilerleme, restart (LevelLoader uzerinden)
    //   - PlayerPrefs ile player'in en son hangi level'da kaldigini hatirla
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Level Sistemi")]
        [Tooltip("Tum levellari iceren database asset.")]
        [SerializeField] private LevelDatabase levelDatabase;

        [Tooltip("Oyun acildiginda hangi level'dan baslasin (0 = ilk level).")]
        [SerializeField] private int startingLevelIndex = 0;

        [Tooltip("Player'in en son kaldigi level'i kaydet/yukle.")]
        [SerializeField] private bool usePlayerPrefs = true;

        // PlayerPrefs anahtari, herkese acik degil
        private const string PrefsKeyCurrentLevel = "Prism_CurrentLevel";

        // oyunun anlik durumu
        public GameState CurrentState { get; private set; } = GameState.Playing;

        // su anki level index'i (0-based)
        public int CurrentLevelIndex { get; private set; }

        // event-driven mimari, baska scriptler bu event'lere subscribe olabilir
        // UI bu event'leri dinleyip ekran guncellemesi yapacak
        public System.Action OnLevelComplete;
        public System.Action OnLevelRestart;
        public System.Action<LevelData> OnLevelLoaded;

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
            // PlayerPrefs'ten son level'i yukle, yoksa starting level
            CurrentLevelIndex = usePlayerPrefs
                ? PlayerPrefs.GetInt(PrefsKeyCurrentLevel, startingLevelIndex)
                : startingLevelIndex;

            // valid range icine sigdir
            CurrentLevelIndex = Mathf.Clamp(CurrentLevelIndex, 0,
                Mathf.Max(0, (levelDatabase != null ? levelDatabase.Count : 1) - 1));

            LoadLevelByIndex(CurrentLevelIndex);
        }

        // ---- PUBLIC API ----

        // belirli bir level index'ini yukle
        public void LoadLevelByIndex(int index)
        {
            if (levelDatabase == null)
            {
                Debug.LogError("[GameManager] LevelDatabase atanmamis.");
                return;
            }

            LevelData data = levelDatabase.GetLevel(index);
            if (data == null)
            {
                Debug.LogWarning($"[GameManager] Level index {index} bulunamadi.");
                return;
            }

            CurrentLevelIndex = index;
            CurrentState = GameState.Playing;

            if (LevelLoader.Instance != null) LevelLoader.Instance.LoadLevel(data);

            // PlayerPrefs'e kaydet
            if (usePlayerPrefs)
            {
                PlayerPrefs.SetInt(PrefsKeyCurrentLevel, index);
                PlayerPrefs.Save();
            }

            OnLevelLoaded?.Invoke(data);
        }

        // bir sonraki level'e gec
        // son level'de ise basa donmez (UI farkli ekran gostersin)
        public void NextLevel()
        {
            int next = CurrentLevelIndex + 1;
            if (levelDatabase != null && next < levelDatabase.Count)
            {
                LoadLevelByIndex(next);
            }
            else
            {
                Debug.Log("[GameManager] Tum levellar tamamlandi.");
                // ileride: "All Done" ekrani veya loop
            }
        }

        // mevcut level'i restart et
        public void RestartLevel()
        {
            CurrentState = GameState.Playing;
            if (LevelLoader.Instance != null) LevelLoader.Instance.ReloadCurrentLevel();
            OnLevelRestart?.Invoke();
        }

        // tum kristaller aktif olunca BeamManager bunu cagirir
        public void CompleteLevel()
        {
            // zaten complete ise tekrar tetikleme
            if (CurrentState == GameState.LevelComplete) return;

            CurrentState = GameState.LevelComplete;
            OnLevelComplete?.Invoke();
        }
    }

    // sinirli oyun durumlari
    public enum GameState
    {
        Playing,
        LevelComplete,
        Paused
    }
}
