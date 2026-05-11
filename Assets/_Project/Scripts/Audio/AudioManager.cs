using UnityEngine;

namespace Prism
{
    // tum oyun seslerinin yonetimi
    // NEDEN ENUM: SfxId enum'u sayesinde ses tetiklerken string yazmiyoruz, typo riski yok
    // NEDEN INSPECTOR'DAN ATA: Resources.Load yerine direct referans, asset'ler build'e dahil olur
    //
    // mimari: SFX'ler tek shared AudioSource'tan PlayOneShot ile, ust uste binebilir
    // Music ayri AudioSource'ta, loop calar
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("SFX Klipleri")]
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip mirrorTapClip;
        [SerializeField] private AudioClip crystalActivateClip;
        [SerializeField] private AudioClip levelCompleteClip;

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusicClip;

        [Header("Volume Ayarlari")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.4f;

        // iki ayri AudioSource: SFX'ler PlayOneShot ile ust uste binebilsin,
        // Music tek track loop calsin
        private AudioSource sfxSource;
        private AudioSource musicSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // NOT: DontDestroyOnLoad kullanmiyoruz cunku oyunda tek sahne var (MainScene)
            // sahne degisimi yok, AudioManager zaten levelin sonuna kadar yasiyor
            // ileride multi-scene yapilacak olursa eklenmeli

            SetupAudioSources();
        }

        private void Start()
        {
            PlayMusic();
        }

        // iki AudioSource olustur, birini SFX birini Music icin
        private void SetupAudioSources()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }

        // ---- PUBLIC API ----

        // enum kullanan SFX cagrisi, kod icinde bu sekilde cagirilir
        // ornek: AudioManager.Instance.PlaySfx(SfxId.MirrorTap);
        public void PlaySfx(SfxId id)
        {
            AudioClip clip = GetClipFor(id);
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        // background music'i baslat (Start'ta otomatik cagrilir)
        public void PlayMusic()
        {
            if (backgroundMusicClip == null) return;
            if (musicSource.isPlaying && musicSource.clip == backgroundMusicClip) return;

            musicSource.clip = backgroundMusicClip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();

        // settings UI icin volume slider'lari kullanilabilir
        public void SetSfxVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
            sfxSource.volume = sfxVolume;
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            musicSource.volume = musicVolume;
        }

        // ---- INTERNAL ----

        // enum -> AudioClip mapping
        // switch ile yapilir, dictionary'ye gerek yok (sadece 4 ses)
        private AudioClip GetClipFor(SfxId id) => id switch
        {
            SfxId.UiClick         => uiClickClip,
            SfxId.MirrorTap       => mirrorTapClip,
            SfxId.CrystalActivate => crystalActivateClip,
            SfxId.LevelComplete   => levelCompleteClip,
            _                     => null
        };
    }

    // tum SFX'lerin enum tanimi, kod icinde tetiklerken bunlardan birini kullaniriz
    public enum SfxId
    {
        UiClick,
        MirrorTap,
        CrystalActivate,
        LevelComplete
    }
}

