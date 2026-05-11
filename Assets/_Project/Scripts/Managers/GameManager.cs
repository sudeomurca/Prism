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

        [Header("Win Timing")]
        [Tooltip("Level complete olduktan sonra panelin acilmasi icin bekleme suresi (saniye). Oyuncu kazandigini hissetsin diye.")]
        [SerializeField] private float winDelay = 0.8f;

        // PlayerPrefs anahtari, herkese acik degil
        private const string PrefsKeyCurrentLevel = "Prism_CurrentLevel";

        // oyunun anlik durumu
        public GameState CurrentState { get; private set; } = GameState.Playing;

        // su anki level index'i (0-based)
        public int CurrentLevelIndex { get; private set; }

        // son level mi diye kontrol, UI bunu kullanir
        // levelDatabase boslarsa Count-1 = -1 olur, false donmesi icin Count > 0 sarti da var
        public bool IsOnLastLevel =>
            levelDatabase != null
            && levelDatabase.Count > 0
            && CurrentLevelIndex >= levelDatabase.Count - 1;

        // event-driven mimari, baska scriptler bu event'lere subscribe olabilir
        // UI bu event'leri dinleyip ekran guncellemesi yapacak
        public System.Action OnLevelComplete;
        public System.Action OnLevelRestart;
        public System.Action<LevelData> OnLevelLoaded;

        // celebration: kristal yandigi an hemen tetiklenir, konfeti/ses icin
        // Vector3 parametresi ile pozisyon iletir, konfeti orada patlasin
        // OnLevelComplete'ten farkli, OnLevelComplete winDelay sonra gelir
        public System.Action<Vector3> OnCelebrationStart;

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
        // UI son leveldan sonra Next butonu gostermiyor (FINAL mod) ama yine de
        // savunma amaciyla burada kontrol var, son leveldan sonra cagrilirsa hicbir sey yapmaz
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
            }
        }

        // mevcut level'i restart et
        public void RestartLevel()
        {
            CurrentState = GameState.Playing;
            if (LevelLoader.Instance != null) LevelLoader.Instance.ReloadCurrentLevel();
            OnLevelRestart?.Invoke();
        }

        // tum oyunu basa al, ilk leveldan baslat
        // "Restart from Level 1" butonu icin
        public void RestartFromBeginning()
        {
            LoadLevelByIndex(0);
        }

        // tum kristaller aktif olunca BeamManager bunu cagirir
        // celebrationPos: konfeti vs. spawn noktasi (kristallerin ortalama pozisyonu)
        public void CompleteLevel(Vector3 celebrationPos)
        {
            // zaten complete ise tekrar tetikleme
            if (CurrentState == GameState.LevelComplete) return;

            CurrentState = GameState.LevelComplete;

            // hemen panel acmak yerine belirli sure bekle, oyuncu kazandigini hissetsin
            // bu sirada konfeti, ses gibi feedback'ler oynar
            // Coroutine ile basit ve okunabilir, async/await kullanmadan
            StartCoroutine(InvokeLevelCompleteAfterDelay(celebrationPos));
        }

        private System.Collections.IEnumerator InvokeLevelCompleteAfterDelay(Vector3 celebrationPos)
        {
            // celebration event hemen tetiklenir (konfeti, ses), panel sonra acilir
            OnCelebrationStart?.Invoke(celebrationPos);

            // win sesi celebration ile birlikte calar
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.LevelComplete);

            yield return new WaitForSeconds(winDelay);

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
