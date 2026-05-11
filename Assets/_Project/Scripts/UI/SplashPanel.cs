using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Prism
{
    // oyun acilisinda gorunen ilk panel
    // ortada oyun adi + Start butonu, arkaplan oyun sahnesindeki ile ayni
    // start butonuna basinca panel fade out olur, oyuna gecer
    //
    // NEDEN START BUTONU (tap-anywhere degil): netlik ve kontrol
    // kullanici neye tikladigini bilir, accidental skip yok
    public class SplashPanel : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Panelin ana CanvasGroup'u, fade icin.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Start butonu, oyunu baslatir.")]
        [SerializeField] private Button startButton;

        [Header("Animasyon")]
        [SerializeField] private float fadeOutDuration = 0.5f;

        // start butonuna basildiginda tetiklenir, UIManager dinler
        public System.Action OnStartPressed;

        private void Awake()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            // baslangicta tam gorunur
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartClicked);

            // event'e subscribe olanlari temizle, dangling reference olmasin
            OnStartPressed = null;
        }

        private void OnStartClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.UiClick);

            OnStartPressed?.Invoke();
            FadeOutAndHide();
        }

        // panel fade out olur, sonra deactivate olur
        private void FadeOutAndHide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill(complete: false);
                canvasGroup.DOFade(0f, fadeOutDuration)
                    .OnComplete(() => gameObject.SetActive(false));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
