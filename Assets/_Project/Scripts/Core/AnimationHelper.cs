using DG.Tweening;
using UnityEngine;

namespace Prism
{
    // tum oyun animasyonlari icin merkezi static helper
    //
    // NEDEN STATIC: animasyonlar stateless, instance gerekmez
    // NEDEN AYRI CLASS: scriptlere DOTween cagrilarini dagitmak yerine tek noktada toplamak
    //   - DRY (don't repeat yourself): pulse/rotate kodu tekrar edilmez
    //   - tek noktadan ayar: sure veya easing degisecekse tek yerde
    //   - mock'lanabilir: ileride animasyon kapatilmak istenirse buradan kapatilir
    //
    // her metod onceki tween'i once Kill eder, double-tween olusup garip davranis yaratmasin
    public static class AnimationHelper
    {
        // animasyon sureleri tek yerde, kolay tune edilir
        public const float MirrorRotateDuration = 0.2f;
        public const float CrystalPulseDuration = 0.3f;
        public const float SpawnDuration        = 0.25f;
        public const float ShortDuration        = 0.15f; // fade, tint gibi kisa gecisler icin

        // bir transform'u smooth donduren animasyon
        // hedef rotation Z derece olarak verilir
        // baska bir tap gelirse onceki animasyon kesilir, yenisi baslar
        public static void RotateTo(Transform target, float zAngle, float duration = MirrorRotateDuration)
        {
            if (target == null) return;

            // bu hedefin onceki rotation tween'ini durdur
            target.DOKill(complete: false);

            // smooth donus, OutBack hafif bounce verir, mekanik hissi guzellestirir
            // RotateMode.Fast: kisa yoldan doner, 360 dereceden fazla donmez
            // (orn. 350 -> 10 derece donerken "-340" yerine "+20" yapar)
            target.DORotate(new Vector3(0, 0, zAngle), duration, RotateMode.Fast)
                  .SetEase(Ease.OutBack);
        }

        // pulse efekti , scale up ve geri don, dikkat cekmek icin
        // kristal aktiflenince veya buton basildiginda kullanilir
        //
        // ONEMLI: SpawnPop hala devam ediyorsa scale yarida kalmis olabilir.
        // O yuzden DOKill'den ONCE complete=true gecip animasyonu bitiriyoruz,
        // sonra original scale'i guvenle okuyoruz. Bu sayede SpawnPop+Pulse
        // ayni frame'de tetiklenirse kristal kucuk kalmaz.
        public static void Pulse(Transform target, float scaleMultiplier = 1.3f, float duration = CrystalPulseDuration)
        {
            if (target == null) return;

            // varsa onceki tween'i ATLA ve final value'ya zipla
            // boylece SpawnPop yarida kalsa bile scale tam halinde olur
            target.DOKill(complete: true);

            // hala (0,0,0) ise (animasyon hic baslamamis vs.) bir'e fall back et
            Vector3 originalScale = target.localScale;
            if (originalScale == Vector3.zero) originalScale = Vector3.one;

            // scale'i once buyut sonra orijinale don, iki kademeli sequence
            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOScale(originalScale * scaleMultiplier, duration * 0.4f).SetEase(Ease.OutQuad));
            seq.Append(target.DOScale(originalScale, duration * 0.6f).SetEase(Ease.OutBounce));
        }

        // bir parca sahneye dogarken, scale 0 dan 1 e yumusak gecis
        // level baslarken parcalar sirayla canlanir hissi verir
        //
        // ONEMLI: original scale OKU, sonra sifirla, sonra geri buyut
        // eger sifirladiktan sonra okusaydik (0,0,0) okurduk ve hic buyumezdi (bug!)
        public static void SpawnPop(Transform target, float delay = 0f, float duration = SpawnDuration)
        {
            if (target == null) return;

            // varsa onceki tween'i durdur, double-animation olusturmasin
            target.DOKill(complete: false);

            // mevcut scale (0,0,0) ise default Vector3.one'a fall back et
            // (prefab veya scene'de scale 0 olarak gelmis olabilir, koruma)
            Vector3 targetScale = target.localScale;
            if (targetScale == Vector3.zero) targetScale = Vector3.one;

            target.localScale = Vector3.zero;
            target.DOScale(targetScale, duration)
                  .SetDelay(delay)
                  .SetEase(Ease.OutBack);
        }

        // bir SpriteRenderer'in alpha'sini smooth degistirir
        // level transition icin kullanilir
        public static void FadeSprite(SpriteRenderer sr, float targetAlpha, float duration = ShortDuration)
        {
            if (sr == null) return;
            sr.DOKill(complete: false);
            sr.DOFade(targetAlpha, duration);
        }

        // bir SpriteRenderer'in rengini smooth degistirir
        // prizma yarisi isin geldiginde renk alir, kesilince beyaza doner
        public static void TintColor(SpriteRenderer sr, Color targetColor, float duration = ShortDuration)
        {
            if (sr == null) return;
            sr.DOKill(complete: false);
            sr.DOColor(targetColor, duration);
        }
    }
}
