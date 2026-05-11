using System.Collections.Generic;
using UnityEngine;

namespace Prism
{
    // Generic object pool, herhangi bir Component tipi icin calisir.
    //
    // NEDEN: Mobil oyunlarda Instantiate/Destroy pahalidir, GC tetikler ve frame drop yapar.
    // Cozum: nesneleri yaratip yok etmek yerine havuzdan al, isi bittiyse geri ver.
    //
    // KULLANIM:
    //   var pool = new ObjectPool<LineRenderer>(prefab, parent: transform, initialSize: 10);
    //   LineRenderer lr = pool.Get();   // havuzdan cek, aktiflesir
    //   pool.Release(lr);               // havuza iade, deaktif olur
    //
    // ozellikler:
    //   - generic, herhangi bir Component tipi icin tekrar yazmaya gerek yok
    //   - havuz biterse otomatik buyur (lazy expand)
    //   - parent transform optional, hierarchy'de duzenli kalir
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Queue<T> available = new();
        private readonly HashSet<T> inUse = new(); // double-release koruma icin

        public int CountAvailable => available.Count;
        public int CountInUse => inUse.Count;
        public int CountTotal => CountAvailable + CountInUse;

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 0)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPool] prefab null verildi.");
                return;
            }

            this.prefab = prefab;
            this.parent = parent;

            // initial pool: oyun baslamadan, frame uzerinde olmadan onceden yarat
            // boylece runtime'da ilk Get() cagrisi smooth gecer
            for (int i = 0; i < initialSize; i++)
            {
                T instance = CreateInstance();
                instance.gameObject.SetActive(false);
                available.Enqueue(instance);
            }
        }

        // havuzdan bir nesne al
        // havuz bossa otomatik yeni yaratir (pool dynamic grows)
        // pool icindeki nesne destroy edilmisse (=null) onu atlayip yeni alir veya yaratir
        public T Get()
        {
            T instance = null;

            // pool icinde destroy edilmis nesneler olabilir, onlari atla
            while (available.Count > 0)
            {
                instance = available.Dequeue();
                if (instance != null) break;
                // null bulduk (destroy edilmis), bir sonrakine bak
                instance = null;
            }

            if (instance == null)
            {
                // havuz bos veya tum nesneler destroy edilmis, dinamik buyut
                instance = CreateInstance();
            }

            instance.gameObject.SetActive(true);
            inUse.Add(instance);
            return instance;
        }

        // nesneyi havuza geri ver
        // double-release durumunda uyari verir, hatasiz devam eder
        public void Release(T instance)
        {
            if (instance == null) return;

            if (!inUse.Remove(instance))
            {
                // ya bu pool'a ait degil ya da zaten release edilmis
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Release: zaten serbest veya bu pool'a ait degil.");
                return;
            }

            instance.gameObject.SetActive(false);
            available.Enqueue(instance);
        }

        // tum aktif nesneleri toplu olarak geri ver (level resetinde kullanisli)
        // Release()'i tek tek cagirmak yerine direkt foreach + Clear yapariz,
        // ayni isi yapar ama tek seferde HashSet temizlenir (daha hizli)
        public void ReleaseAll()
        {
            foreach (var instance in inUse)
            {
                if (instance != null)
                {
                    instance.gameObject.SetActive(false);
                    available.Enqueue(instance);
                }
            }
            inUse.Clear();
        }

        // havuzu komple temizle, tum nesneleri destroy et (sahne kapanirken)
        public void Clear()
        {
            foreach (var instance in available)
            {
                if (instance != null) Object.Destroy(instance.gameObject);
            }
            foreach (var instance in inUse)
            {
                if (instance != null) Object.Destroy(instance.gameObject);
            }
            available.Clear();
            inUse.Clear();
        }

        private T CreateInstance()
        {
            T instance = Object.Instantiate(prefab, parent);
            instance.gameObject.name = prefab.name; // "(Clone)" suffix gozukmesin
            return instance;
        }
    }
}
