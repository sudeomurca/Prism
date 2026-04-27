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
        public const float DefaultEase          = 0.15f;

        // bir transform'u smooth donduren animasyon
        // hedef rotation Z derece olarak verilir
        // baska bir tap gelirse onceki animasyon kesilir, yenisi baslar
        public static void RotateTo(Transform target, float zAngle, float duration = MirrorRotateDuration)
        {
            if (target == null) return;

            // bu hedefin onceki rotation tween'ini durdur
            target.DOKill(complete: false);

            // smooth donus, OutBack hafif bounce verir, mekanik hissi guzellestirir
            target.DORotate(new Vector3(0, 0, zAngle), duration, RotateMode.Fast)
                  .SetEase(Ease.OutBack);
        }

        // pulse efekti , scale up ve geri don, dikkat cekmek icin
        // kristal aktiflenince veya buton basildiginda kullanilir
        public static void Pulse(Transform target, float scaleMultiplier = 1.3f, float duration = CrystalPulseDuration)
        {
            if (target == null) return;

            target.DOKill(complete: false);

            Vector3 originalScale = target.localScale;
            // scale'i once buyut sonra orijinale don,iki kademeli sequence
            Sequence seq = DOTween.Sequence();
            seq.Append(target.DOScale(originalScale * scaleMultiplier, duration * 0.4f).SetEase(Ease.OutQuad));
            seq.Append(target.DOScale(originalScale, duration * 0.6f).SetEase(Ease.OutBounce));
        }

        // bir parca sahneye dogarken, scale 0 dan 1 e yumusak gecis
        // level baslarken parcalar sirayla canlanir hissi verir
        public static void SpawnPop(Transform target, float delay = 0f, float duration = SpawnDuration)
        {
            if (target == null) return;

            Vector3 originalScale = target.localScale;
            target.localScale = Vector3.zero;

            target.DOScale(originalScale, duration)
                  .SetDelay(delay)
                  .SetEase(Ease.OutBack);
        }

        // bir SpriteRenderer'in alpha'sini smooth degistirir
        // level transition icin kullanilir
        public static void FadeSprite(SpriteRenderer sr, float targetAlpha, float duration = DefaultEase)
        {
            if (sr == null) return;
            sr.DOKill(complete: false);
            sr.DOFade(targetAlpha, duration);
        }
    }
}
