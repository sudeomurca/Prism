using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Prism
{
    // level bittiginde acilan panel
    // iki modu var:
    //   NORMAL: Level Complete basligi, Next butonu, ikon Restart butonu
    //   FINAL:  Level Complete basligi + All Done alt yazisi, sadece "Level 1'den yeniden oyna" butonu
    // hangi mod GameManager.IsOnLastLevel'a gore karar verilir
    public class LevelCompletePanel : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Panelin ana CanvasGroup'u, fade icin.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Icerigi tutan transform, scale animasyonu icin.")]
        [SerializeField] private RectTransform contentTransform;

        [Tooltip("All Done alt yazisi (sadece FINAL modda gorunur).")]
        [SerializeField] private GameObject allDoneText;

        [Tooltip("Bir sonraki level'a gecis butonu (sadece NORMAL modda gorunur).")]
        [SerializeField] private Button nextButton;

        [Tooltip("Ikon seklinde restart butonu (sadece NORMAL modda gorunur, current level'i restart eder).")]
        [SerializeField] private Button restartButtonIcon;

        [Tooltip("Text'li 'Level 1'den yeniden oyna' butonu (sadece FINAL modda gorunur).")]
        [SerializeField] private Button restartFromStartButton;

        [Header("Animasyon")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;

        private void Awake()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClicked);
            if (restartButtonIcon != null)
                restartButtonIcon.onClick.AddListener(OnRestartCurrentClicked);
            if (restartFromStartButton != null)
                restartFromStartButton.onClick.AddListener(OnRestartFromStartClicked);

            // baslangicta gizli
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (nextButton != null)
                nextButton.onClick.RemoveListener(OnNextClicked);
            if (restartButtonIcon != null)
                restartButtonIcon.onClick.RemoveListener(OnRestartCurrentClicked);
            if (restartFromStartButton != null)
                restartFromStartButton.onClick.RemoveListener(OnRestartFromStartClicked);
        }

        // ---- PUBLIC API ----

        public void Show(bool isFinal)
        {
            ApplyMode(isFinal);

            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.DOKill(complete: false);
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration);
            }

            if (contentTransform != null)
            {
                contentTransform.DOKill(complete: false);
                contentTransform.localScale = Vector3.zero;
                contentTransform.DOScale(Vector3.one, fadeInDuration).SetEase(Ease.OutBack);
            }
        }

        public void Hide()
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

        // ---- INTERNAL ----

        // moda gore butonlari ve all done yazisini ac/kapat
        // NORMAL: Next + ikon Restart goster, AllDone text ve Restart from Start gizle
        // FINAL: Next gizle, AllDone text + ikon Restart + Restart from Start goster
        // (FINAL'da ikon Restart ACIK kalir cunku oyuncu son leveli da yeniden oynamak isteyebilir)
        private void ApplyMode(bool isFinal)
        {
            if (allDoneText != null)            allDoneText.SetActive(isFinal);
            if (nextButton != null)             nextButton.gameObject.SetActive(!isFinal);
            if (restartButtonIcon != null)      restartButtonIcon.gameObject.SetActive(true); // her iki modda da gorunur
            if (restartFromStartButton != null) restartFromStartButton.gameObject.SetActive(isFinal);
        }

        // ---- BUTTON CALLBACKS ----

        private void OnNextClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.UiClick);
            if (GameManager.Instance != null) GameManager.Instance.NextLevel();
        }

        private void OnRestartCurrentClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.UiClick);
            if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
        }

        private void OnRestartFromStartClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.UiClick);
            if (GameManager.Instance != null) GameManager.Instance.RestartFromBeginning();
        }
    }
}
