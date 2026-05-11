using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Prism
{
    // her zaman gorunen UI: level numarasi ve restart butonu
    // UIManager tarafindan kontrol edilir, kendi basina decision vermez
    public class HUDPanel : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Level numarasini gosteren TMP Text.")]
        [SerializeField] private TMP_Text levelNumberText;

        [Tooltip("Restart butonu.")]
        [SerializeField] private Button restartButton;

        [Header("Format")]
        [Tooltip("Level metninin formati. {0} level numarasi ile yer degisir.")]
        [SerializeField] private string levelTextFormat = "Level {0}";

        private void Awake()
        {
            // restart butonu, GameManager'i cagirsin
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }
        }

        // ---- PUBLIC API (UIManager kullanir) ----

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void UpdateLevelNumber(int number)
        {
            if (levelNumberText != null)
            {
                levelNumberText.text = string.Format(levelTextFormat, number);
            }
        }

        // ---- BUTTON CALLBACKS ----

        private void OnRestartClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.UiClick);
            if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
        }
    }
}
