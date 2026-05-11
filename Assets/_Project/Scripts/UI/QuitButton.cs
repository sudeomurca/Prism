using UnityEngine;
using UnityEngine.UI;

namespace Prism
{
    //  butona basinca oyun kapatilir
    [RequireComponent(typeof(Button))]
    public class QuitButton : MonoBehaviour
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();

            // WebGLde quit calismadigindan butonu tamamen gizle
#if UNITY_WEBGL && !UNITY_EDITOR
            gameObject.SetActive(false);
            return;
#endif

            button.onClick.AddListener(OnQuitClicked);
        }

        private void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnQuitClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySfx(SfxId.UiClick);

            // editorde play modunu durdurur, buildde uygulamayi kapatir
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
