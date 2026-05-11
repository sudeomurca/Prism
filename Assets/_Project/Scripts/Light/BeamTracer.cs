using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // isin izleme mantigi icin paylasilan helper class
    // hem kaynaktan cikan isinlar hem de prizmadan cikan isinlar ayni mantigi kullaniyor
    // o yuzden tek yerde, static fonksiyon olarak tuttuk (DRY)
    public static class BeamTracer
    {
        // gecici array, cok raycast yapacagiz, her cagrida alloc olmasin diye class-level
        // NOT: static ama Unity tek thread'de calistigi icin (main thread) thread-safe sorunu yok
        // boyut 8: bir ray uzerinde max 8 collider adayi, oyunumuz icin fazlasiyla yeterli
        private static readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[8];

        // ContactFilter2D, raycast'in nasil filtre edilecegini soyler
        // useTriggers = false: trigger collider'lari ignore eder
        // useLayerMask = false: tum layerlardaki collider'lari kabul eder
        private static readonly ContactFilter2D contactFilter = new ContactFilter2D
        {
            useTriggers = false,
            useLayerMask = false
        };

        // belirli pozisyondan, yonden bir isini izle
        // ayna bulursa yansit, prizma veya kristal bulursa feed et, durdur
        // ignoreCollider: bu collider hic algilanmayacak (prizma cikisinda kendi collider'ini ignore etmek icin)
        // displayColor: prizma yarisini boyamak icin gorsel renk (LightColor enum'undan ayri)
        public static void Trace(
            Vector3 startPos,
            Direction startDir,
            LightColor color,
            Color displayColor,
            int maxReflections,
            float maxSegmentLength,
            float rayOffset,
            List<Vector3> outPoints,
            Collider2D ignoreCollider = null)
        {
            outPoints.Clear();
            outPoints.Add(startPos);

            Vector3 currentPos = startPos;
            Direction currentDir = startDir;
            Collider2D currentIgnore = ignoreCollider;

            for (int i = 0; i < maxReflections; i++)
            {
                Vector2 dirVec = currentDir.ToVector();
                Vector2 rayStart = (Vector2)currentPos + dirVec * rayOffset;

                // Unity 6 onerilen API: Physics2D.Raycast(buffer overload) ile contactFilter
                // RaycastNonAlloc deprecated edildi, yerine bu kullaniliyor
                int hitCount = Physics2D.Raycast(rayStart, dirVec, contactFilter, hitBuffer, maxSegmentLength);

                // en yakin hit'i bul, currentIgnore olan hit'leri atla
                RaycastHit2D bestHit = default;
                float bestDistance = float.MaxValue;
                bool foundHit = false;

                for (int h = 0; h < hitCount; h++)
                {
                    RaycastHit2D candidate = hitBuffer[h];
                    if (candidate.collider == currentIgnore) continue;
                    if (candidate.distance < bestDistance)
                    {
                        bestDistance = candidate.distance;
                        bestHit = candidate;
                        foundHit = true;
                    }
                }

                // hicbir seye carpmadi, max mesafeye kadar ciz ve isi bitir
                if (!foundHit)
                {
                    Vector3 endPoint = currentPos + (Vector3)(dirVec * maxSegmentLength);
                    outPoints.Add(endPoint);
                    return;
                }

                // ayna mi?
                Mirror mirror = bestHit.collider.GetComponent<Mirror>();
                if (mirror != null)
                {
                    // gorsel olarak isin ayna MERKEZINE kadar cizilsin, orada kirilsin
                    // boylece L seklinde temiz bir yansima olur, kenara snap atmaz
                    Vector3 mirrorCenter = bestHit.collider.transform.position;
                    outPoints.Add(mirrorCenter);

                    // arkadan gelen isini yansitmaz, orada durur (emilir)
                    if (!mirror.CanReflect(currentDir))
                        return;

                    currentDir = mirror.Reflect(currentDir);
                    currentPos = mirrorCenter;
                    // bu ayna bir sonraki raycast'te tekrar algilanmasin
                    // collider buyuk oldugu icin onun icinden cikan ray ayni collider'a tekrar carpiyordu
                    currentIgnore = bestHit.collider;
                    continue;
                }

                // prizma mi?
                LightPrism prism = bestHit.collider.GetComponent<LightPrism>();
                if (prism != null)
                {
                    // prizmaya isin geldigini, hangi yonden geldigini ve gorsel rengini bildir
                    prism.ReceiveLight(color, currentDir, displayColor);
                    outPoints.Add(prism.Position);
                    return;
                }

                // kristal mi?
                Crystal crystal = bestHit.collider.GetComponent<Crystal>();
                if (crystal != null)
                {
                    crystal.ReceiveLight(color);
                }

                // kristal veya bilinmeyen collider: carpma noktasina kadar ciz ve dur
                // (savunma: sahnede beklenmeyen bir collider olursa isin orada emilir)
                outPoints.Add(bestHit.point);
                return;
            }

            // max yansima sinirina ulastik (cok ayna art arda),
            // son segmenti de ciz ki isin "havada" bitmis gibi gozukmesin
            Vector2 lastDir = currentDir.ToVector();
            Vector3 fallbackEnd = currentPos + (Vector3)(lastDir * maxSegmentLength);
            outPoints.Add(fallbackEnd);
        }
    }
}
