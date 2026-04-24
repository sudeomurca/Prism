using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // isin izleme mantigi icin paylasilan helper class
    // hem kaynaktan cikan isinlar hem de prizmadan cikan isinlar ayni mantigi kullaniyor
    // o yuzden tek yerde, static fonksiyon olarak tuttuk
    public static class BeamTracer
    {
        // belirli pozisyondan,yonden bir isini izle
        // ayna bulursa yansit, prizma veya kristal bulursa feed et, durduur
        public static void Trace(
            Vector3 startPos,
            Direction startDir,
            LightColor color,
            int maxReflections,
            float maxSegmentLength,
            float rayOffset,
            List<Vector3> outPoints)
        {
            outPoints.Clear();
            outPoints.Add(startPos);

            Vector3 currentPos = startPos;
            Direction currentDir = startDir;

            for (int i = 0; i < maxReflections; i++)
            {
                Vector2 dirVec = currentDir.ToVector();

                // raycast baslangicini biraz ileri kaydir,ayni collider'i tekrar algilamayi engeller
                Vector2 rayStart = (Vector2)currentPos + dirVec * rayOffset;
                RaycastHit2D hit = Physics2D.Raycast(rayStart, dirVec, maxSegmentLength);

                if (hit.collider != null)
                {
                    // ayna mi?
                    Mirror mirror = hit.collider.GetComponent<Mirror>();
                    if (mirror != null)
                    {
                        outPoints.Add(hit.point);
                        currentDir = mirror.Reflect(currentDir);
                        currentPos = hit.collider.transform.position;
                        continue;
                    }

                    // prizma mi?
                    LightPrism prism = hit.collider.GetComponent<LightPrism>();
                    if (prism != null)
                    {
                        prism.ReceiveLight(color);
                        outPoints.Add(prism.Position);
                        break;
                    }

                    // kristal mi?
                    Crystal crystal = hit.collider.GetComponent<Crystal>();
                    if (crystal != null)
                    {
                        crystal.ReceiveLight(color);
                    }

                    outPoints.Add(hit.point);
                    break;
                }

                // hicbir seye carpmazsa max mesafeye kadar ciz,bitir
                Vector3 endPoint = currentPos + (Vector3)(dirVec * maxSegmentLength);
                outPoints.Add(endPoint);
                break;
            }
        }
    }
}
