using UnityEngine;

namespace Prism
{
    
    // baska scriptler GameManager'a instance uzerinden erisir
    public class GameManager : MonoBehaviour
    {
        
        // degistirilemez ama her yerden okunur
        public static GameManager Instance { get; private set; }

        
        public GameState CurrentState { get; private set; } = GameState.Playing;

        private void Awake()
        {
            // birden fazla instance olamaz amac bu singletonda
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            
        }

        
        public void CompleteLevel()
        {
            CurrentState = GameState.LevelComplete;
            Debug.Log("Level tamamlandi!");
            // TODO: UI'a level complete ekranını göster
        }

        
        public void RestartLevel()
        {
            CurrentState = GameState.Playing;
            Debug.Log("Level yeniden baslatildi.");
            // TODO: LevelManager'a sahneyi sıfırla de
        }
    }

    // sinirli oyun durumlari ile kontrol sagla
    public enum GameState
    {
        Playing,
        LevelComplete,
        Paused
    }
}
