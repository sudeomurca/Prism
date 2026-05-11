using UnityEngine;

namespace Prism
{
    // tum levellarin listesi
    // GameManager bu asset'ten leveli ceker
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Prism/Level Database", order = 1)]
    public class LevelDatabase : ScriptableObject
    {
        [Tooltip("Sirali levellar")]
        public LevelData[] levels;

        public int Count => levels != null ? levels.Length : 0;

        public LevelData GetLevel(int index)
        {
            if (levels == null || index < 0 || index >= levels.Length) return null;
            return levels[index];
        }
    }
}
