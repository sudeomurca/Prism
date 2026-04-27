using System;
using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // bir level'in tum icerigini tutan ScriptableObject asset
    // her level Assets/_Project/ScriptableObject/Levels/ klasoru altinda ayri bir asset
    //
    // NEDEN ScriptableObject:
    //   - Inspector'da gorsel duzenleme (JSON gibi metin degil, tiklamali)
    //   - Type-safe (Unity validation)
    //   - Asset referanslari direkt baglanir (LightColorData gibi)
    //   - Edit modunda hot-reload, Play'de aninda goruyorsun
    [CreateAssetMenu(fileName = "Level_New", menuName = "Prism/Level Data", order = 0)]
    public class LevelData : ScriptableObject
    {
        [Header("Level Bilgisi")]
        [Tooltip("UI'da gosterilecek level numarasi.")]
        public int levelNumber = 1;

        [Tooltip("Opsiyonel level adi/temasi (ornek: 'First Light').")]
        public string levelName;

        [Header("Level Icerigi")]
        public List<LightSourceConfig> lightSources = new();
        public List<MirrorConfig>      mirrors      = new();
        public List<CrystalConfig>     crystals     = new();
        public List<PrismConfig>       prisms       = new();
    }

    // her parca icin yerlesim ayarlari
    // Serializable -> Inspector'da nested gorunur

    [Serializable]
    public class LightSourceConfig
    {
        [Tooltip("World position.")]
        public Vector2 position;

        [Tooltip("Hangi yonde isin gondersin.")]
        public Direction direction = Direction.Up;

        [Tooltip("Renk asset'i (LightColorData ScriptableObject).")]
        public LightColorData colorData;
    }

    [Serializable]
    public class MirrorConfig
    {
        public Vector2 position;

        [Tooltip("0=/sol-ust  1=\\sol-alt  2=/sag-alt  3=\\sag-ust")]
        [Range(0, 3)]
        public int rotationIndex = 0;
    }

    [Serializable]
    public class CrystalConfig
    {
        public Vector2 position;

        [Tooltip("Aktif olmasi icin gereken renk.")]
        public LightColorData requiredColor;
    }

    [Serializable]
    public class PrismConfig
    {
        public Vector2 position;

        [Tooltip("Karisim cikisi hangi yone gider.")]
        public Direction outputDirection = Direction.Up;

        [Tooltip("Bu prizma hangi rengi uretir? Magenta/Cyan/Yellow gibi.")]
        public LightColorData outputColorData;
    }
}
