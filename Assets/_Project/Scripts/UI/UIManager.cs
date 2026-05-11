using UnityEngine;

namespace Prism
{
    // tum UI panellerini koordine eder
    // GameManager event'lerine subscribe olur, ilgili paneli acar/kapatir
    //
    // MIMARI: panel mantigi UIManager'da degil, her panelin kendi class'inda
    // UIManager sadece "su event olunca su paneli ac" diye yonlendirir
    // SOLID -> single responsibility
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Panel Referanslari")]
        [SerializeField] private SplashPanel splashPanel;
        [SerializeField] private HUDPanel hudPanel;
        [SerializeField] private LevelCompletePanel levelCompletePanel;

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
            // GameManager event'lerine subscribe ol
            // Start tum Awake'lerden sonra calistigi icin Instance set edilmis olmali
            if (GameManager.Instance == null)
            {
                Debug.LogError("[UIManager] GameManager.Instance bulunamadi. Sahnede GameManager var mi?");
                return;
            }

            GameManager.Instance.OnLevelLoaded   += HandleLevelLoaded;
            GameManager.Instance.OnLevelComplete += HandleLevelComplete;
            GameManager.Instance.OnLevelRestart  += HandleLevelRestart;

            // splash panel start butonunu dinle, basinca HUD'i goster
            // splash acikken HUD gizli olmali ("LEVEL 1" yazisi splash arkasindan peeklemesin)
            if (splashPanel != null)
            {
                splashPanel.OnStartPressed += HandleSplashStarted;
                if (hudPanel != null) hudPanel.Hide();
            }

            if (levelCompletePanel != null) levelCompletePanel.Hide();
        }

        private void OnDestroy()
        {
            // memory leak olmasin diye unsubscribe
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelLoaded   -= HandleLevelLoaded;
                GameManager.Instance.OnLevelComplete -= HandleLevelComplete;
                GameManager.Instance.OnLevelRestart  -= HandleLevelRestart;
            }

            if (splashPanel != null) splashPanel.OnStartPressed -= HandleSplashStarted;
        }

        // ---- EVENT HANDLERS ----

        // splash panel start butonu basildi, oyuna gec
        private void HandleSplashStarted()
        {
            if (hudPanel != null) hudPanel.Show();
        }

        private void HandleLevelLoaded(LevelData data)
        {
            if (hudPanel != null) hudPanel.UpdateLevelNumber(data.levelNumber);
            if (levelCompletePanel != null) levelCompletePanel.Hide();
        }

        private void HandleLevelComplete()
        {
            // son level ise final mod ile ac (All Done mesaji)
            bool isFinal = GameManager.Instance != null && GameManager.Instance.IsOnLastLevel;
            if (levelCompletePanel != null) levelCompletePanel.Show(isFinal);
        }

        private void HandleLevelRestart()
        {
            if (levelCompletePanel != null) levelCompletePanel.Hide();
        }
    }
}
