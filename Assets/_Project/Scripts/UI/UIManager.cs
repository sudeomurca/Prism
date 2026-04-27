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
            // GameManager Singleton olmasi gerek, Awake'te Instance set edilmis olmali
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelLoaded   += HandleLevelLoaded;
                GameManager.Instance.OnLevelComplete += HandleLevelComplete;
                GameManager.Instance.OnLevelRestart  += HandleLevelRestart;
            }

            // baslangicta level complete paneli kapali, HUD acik
            if (levelCompletePanel != null) levelCompletePanel.Hide();
            if (hudPanel != null) hudPanel.Show();
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
        }

        // ---- EVENT HANDLERS ----

        private void HandleLevelLoaded(LevelData data)
        {
            if (hudPanel != null) hudPanel.UpdateLevelNumber(data.levelNumber);
            if (levelCompletePanel != null) levelCompletePanel.Hide();
        }

        private void HandleLevelComplete()
        {
            if (levelCompletePanel != null) levelCompletePanel.Show();
        }

        private void HandleLevelRestart()
        {
            if (levelCompletePanel != null) levelCompletePanel.Hide();
        }
    }
}
