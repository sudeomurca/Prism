using System.Collections;
using UnityEngine;

namespace Prism
{
    // konfeti efekti, GameManager.OnCelebrationStart event'i tetiklendiginde patlar
    //
    // NEDEN AYRI SCRIPT: Crystal'in icine yazsak her kristal kendi konfetisini olusturur (overkill)
    // tek noktada, tek ParticleSystem -> performans dostu
    //
    // PATLAMA NOKTASI: BeamManager kristallerin centroid'ini hesaplar,
    // GameManager event uzerinden Vector3 olarak iletir, bu script kendini oraya konumlandirir
    [RequireComponent(typeof(ParticleSystem))]
    public class ConfettiBurst : MonoBehaviour
    {
        private ParticleSystem ps;
        private bool isSubscribed = false;

        private void Awake()
        {
            ps = GetComponent<ParticleSystem>();

            // baslangicta otomatik oynamasin, sadece event'le tetiklensin
            // NOT: ParticleSystem.main bir "MainModule" wrapper'i; ona yazmak
            // aslinda ParticleSystem'in kendisini modifiye eder (Unity'nin API tasarimi)
            var main = ps.main;
            main.playOnAwake = false;
            ps.Stop();
        }

        // Start'i Coroutine olarak yazariz, GameManager hazir olana kadar
        // bir kac frame bekler ve subscribe olur. Boylece Update'i her frame
        // calisir tutmamiza gerek kalmaz, subscribe sonrasi bu coroutine olur.
        private IEnumerator Start()
        {
            // GameManager hazir olana kadar bekle (Script Execution Order garantisi yok)
            while (GameManager.Instance == null)
            {
                yield return null; // bir frame bekle, sonra tekrar kontrol et
            }

            GameManager.Instance.OnLevelComplete += PlayBurst;
            isSubscribed = true;
        }

        private void OnDisable()
        {
            // memory leak olmasin diye unsubscribe
            if (isSubscribed && GameManager.Instance != null)
            {
                GameManager.Instance.OnLevelComplete -= PlayBurst;
                isSubscribed = false;
            }
        }

        // event tetiklenince konfeti patlat
        // ConfettiBurst sahnedeki kendi pozisyonunda kalir
        private void PlayBurst()
        {
            if (ps == null) return;

            ps.Clear();
            ps.Play();
        }
    }
}
