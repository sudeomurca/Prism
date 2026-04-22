using UnityEngine;

namespace Prism
{
    // Oyunun genel durumunu yöneten merkezi class.
    // Singleton pattern kullanıyoruz: sahnede sadece tek bir GameManager olmalı,
    // başka scriptler GameManager.Instance üzerinden erişiyor.
    public class GameManager : MonoBehaviour
    {
        // Her yerden erişilebilen tek instance.
        // "private set" çünkü dışarıdan kimse bunu değiştiremesin, sadece okuyabilsin.
        public static GameManager Instance { get; private set; }

        // Oyunun anlık durumu.
        public GameState CurrentState { get; private set; } = GameState.Playing;

        private void Awake()
        {
            // Eğer başka bir GameManager zaten varsa, bu fazlalık demektir — yok et.
            // Bu Singleton'ın duplicate olmasını engeller.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            // Gun 1 sonu test: renk karisim sistemi calisiyor mu kontrol.
            // Bu kismi yarin silecegiz, simdilik dogrulama icin burada.
            Debug.Log("GameManager calisiyor. Renk karisim testi:");
            Debug.Log($"Red + Green = {LightColorData.Mix(LightColor.Red, LightColor.Green)}");
            Debug.Log($"Red + Blue  = {LightColorData.Mix(LightColor.Red, LightColor.Blue)}");
            Debug.Log($"Green + Blue = {LightColorData.Mix(LightColor.Green, LightColor.Blue)}");
        }

        // Level tamamlandığında çağırılacak.
        public void CompleteLevel()
        {
            CurrentState = GameState.LevelComplete;
            Debug.Log("Level tamamlandi!");
            // TODO: UI'a level complete ekranını göster
        }

        // Level yeniden başlatılmak istendiğinde.
        public void RestartLevel()
        {
            CurrentState = GameState.Playing;
            Debug.Log("Level yeniden baslatildi.");
            // TODO: LevelManager'a sahneyi sıfırla de
        }
    }

    // Oyunun olabileceği durumlar. Enum tutmak string karşılaştırmaktan hem hızlı hem güvenli.
    public enum GameState
    {
        Playing,
        LevelComplete,
        Paused
    }
}
