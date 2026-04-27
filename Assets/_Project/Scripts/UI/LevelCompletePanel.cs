using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Prism
{
    // level bittiginde acilan panel
    // "Level Complete" mesaji + "Next Level" butonu icerir
    // animasyonlu show/hide gecisleri DOTween ile
    public class LevelCompletePanel : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Panelin ana CanvasGroup'u, fade icin.")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Tooltip("Icerigi tutan transform, scale animasyonu icin.")]
        [SerializeField] private RectTransform contentTransform;

        [Tooltip("Bir sonraki level'a gecis butonu.")]
        [SerializeField] private Button nextButton;

        [Tooltip("Restart butonu (opsiyonel).")]
        [SerializeField] private Button restartButton;

        [Header("Animasyon")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.2f;

        private void Awake()
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextClicked);
            }
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            // baslangicta gizli
            if (canvasGroup != null) canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (nextButton != null) nextButton.onClick.RemoveListener(OnNextClicked);
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        // ---- PUBLIC API ----

        public void Show()
        {
            gameObject.SetActive(true);

            // fade-in + scale-up combo
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
            // fade-out, sonra deactivate
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

        // ---- BUTTON CALLBACKS ----

        private void OnNextClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.NextLevel();
        }

        private void OnRestartClicked()
        {
            if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
        }
    }
}
