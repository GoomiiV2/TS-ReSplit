using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.TSFramework.Singletons
{
    // Cache genrated content so we don't have to regenrate it over and over
    public class Cache
    {
        private Dictionary<string, CacheItem> CachedItems = new Dictionary<string, CacheItem>();

        public T Get<T>(string Path)
        {
            if (CachedItems.TryGetValue(Path, out CacheItem cachedItem))
            {
                return (T)cachedItem.Item;
            }

            return default;
        }

        public void Set<T>(string Path, T Item, CacheType Type = CacheType.ClearOnLevelLoad)
        {
            if (!CachedItems.ContainsKey(Path))
            {
                CachedItems.Add(Path, new CacheItem()
                {
                    Path = Path,
                    Item = Item,
                    Type = Type
                });
            }
        }

        // Tries to get a cached item, and if its not cached uses the given Create function to create it and then cache it
        public T TryCache<T>(string Path, Func<string, T> Create, CacheType Type = CacheType.ClearOnLevelLoad)
        {
            if (!CachedItems.ContainsKey(Path))
            {
                var item = Create(Path);
                Set(Path, item, Type);

                Log($"Item {Path} wasn't cached, cahed now :>");
                return item;
            }

            return Get<T>(Path);
        }

        public void Clear(CacheType? TypeToClear = null)
        {
            if (TypeToClear == null)
            {
                CachedItems.Clear();
                Log($"Cleared cache");
            }
            else
            {
                var toRemove = CachedItems.Where(x => x.Value.Type == TypeToClear).Select(x => x.Key);
                foreach (var item in toRemove)
                {
                    CachedItems.Remove(item);
                }

                Log($"Removed {toRemove.Count()} items from the cache with cache type: {TypeToClear}");
            }
        }

        public int GetNumCached()
        {
            return CachedItems.Count;
        }

        private void Log(string Msg)
        {
            UnityEngine.Debug.Log($"[Cache] {Msg}");
        }
    }

    public struct CacheItem
    {
        public string Path;
        public object Item;
        public CacheType Type;
    }

    public enum CacheType
    {
        ClearOnLevelLoad
    }
}
