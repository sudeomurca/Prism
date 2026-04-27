using UnityEngine;

namespace Prism
{
    // tum levellarin listesi, sirali olarak
    // GameManager bu asset'ten "level 0, level 1, level 2..." ceker
    //
    // Inspector'da liste seklinde, drag-drop ile sirayi degistirebilirsin
    // yeni level eklemek = LevelData asset olustur, listeye sürükle, bitti
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Prism/Level Database", order = 1)]
    public class LevelDatabase : ScriptableObject
    {
        [Tooltip("Tum levellar, sirayla. Index 0 = ilk level.")]
        public LevelData[] levels;

        public int Count => levels != null ? levels.Length : 0;

        public LevelData GetLevel(int index)
        {
            if (levels == null || index < 0 || index >= levels.Length) return null;
            return levels[index];
        }
    }
}
