namespace Primary.Memory.Extensions;

public static class CacheFactoryExtensions
{
    private static readonly Random Randomizer;

    static CacheFactoryExtensions()
    {
        Randomizer = new Random(GetSeed());
    }

    /// <summary>
    /// Seed for Random object.
    /// </summary>
    /// <returns>Random 32 bit seed</returns>
    private static int GetSeed()
    {
        byte[] arr = Guid.NewGuid().ToByteArray();
        int i1 = BitConverter.ToInt32(arr, 0);
        int i2 = BitConverter.ToInt32(arr, 4);
        int i3 = BitConverter.ToInt32(arr, 8);
        int i4 = BitConverter.ToInt32(arr, 12);
        long val = (long)i1 + i2 + i3 + i4;
        while (val > int.MaxValue)
            val -= int.MaxValue;
        return (int)val;
    }

    /// <summary>
    /// Generates a 64 bit random generation number (version key)
    /// </summary>
    /// <returns>Random 64 bit number</returns>
    private static ulong Random()
    {
        byte[]? buffer = new byte[sizeof(ulong)];//Buffer Size is 8 byte
        Randomizer.NextBytes(buffer);
        var value = BitConverter.ToUInt64(buffer, 0);

        return value == 0 ? value : ulong.MaxValue;
    }

    /// <summary>
    /// Tries to read a value from local cache. If it is not found there, tries the distributed cache. 
    /// If neither contains the specified key, produces value by calling a loader function and adds the
    /// value to local and distributed cache for a given expiration time. By using a group key, 
    /// all items on both cache types that are members of this group can be expired at once. </summary>
    /// <remarks>
    /// To not check group generation every time an item is requested, generation number itself is also
    /// cached in local cache. Thus, when a generation number changes, local cached items might expire
    /// after about 5 seconds. This means that, if you use this strategy in a web farm setup, when a change 
    /// occurs in one server, other servers might continue to use old local cached data for 5 seconds more.
    /// If this is a problem for your configuration, use DistributedCache directly.
    /// </remarks>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="cache">MemCache cache</param>
    /// <param name="cacheKey">The item key for local and distributed cache</param>
    /// <param name="localExpiration">Local expiration</param>
    /// <param name="remoteExpiration">Distributed cache expiration (is usually same with local expiration)</param>
    /// <param name="groupKey">Group key that will hold generation (version). Can be used to expire all items
    /// that depend on it. This can be a table name. When a table changes, you change its version, and all
    /// cached data that depends on that table is expired.</param>
    /// <param name="factory">The delegate that will be called to generate value, if not found in local cache,
    /// or distributed cache, or all found items are expired.</param>
    public static T? Get<T>(this ICacheFactory cache, string cacheKey, TimeSpan localExpiration, TimeSpan remoteExpiration,
        string groupKey, Func<T?> factory)
        where T : class
    {
        return Get(cache, cacheKey, localExpiration, remoteExpiration,
            groupKey, factory, localOnly: false, forceReload: false);
    }

    /// <summary>
    /// Tries to read a value from local cache. If it is not found there, tries the distributed cache. 
    /// If neither contains the specified key, produces value by calling a loader function and adds the
    /// value to local and distributed cache for a given expiration time. By using a group key, 
    /// all items on both cache types that are members of this group can be expired at once. </summary>
    /// <remarks>
    /// To not check group generation every time an item is requested, generation number itself is also
    /// cached in local cache. Thus, when a generation number changes, local cached items might expire
    /// after about 5 seconds. This means that, if you use this strategy in a web farm setup, when a change 
    /// occurs in one server, other servers might continue to use old local cached data for 5 seconds more.
    /// If this is a problem for your configuration, use DistributedCache directly.
    /// </remarks>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="cache">MemCache cache</param>
    /// <param name="cacheKey">The item key for local and distributed cache</param>
    /// <param name="expiration">Local and remote expiration</param>
    /// <param name="groupKey">Group key that will hold generation (version). Can be used to expire all items
    /// that depend on it. This can be a table name. When a table changes, you change its version, and all
    /// cached data that depends on that table is expired.</param>
    /// <param name="factory">The delegate that will be called to generate value, if not found in local cache,
    /// or distributed cache, or all found items are expired.</param>
    public static T? Get<T>(this ICacheFactory cache, string cacheKey, TimeSpan expiration, string groupKey, Func<T?> factory)
        where T : class
    {
        return Get(cache, cacheKey, expiration, expiration,
            groupKey, factory, localOnly: false, forceReload: false);
    }

    /// <summary>
    /// Tries to read a value from local cache. If it is not found there produces value by calling a loader 
    /// function and adds the value to local cache for a given expiration time. By using a generation 
    /// (item version) key, all items on local cache that are members of this group can be expired 
    /// at once. </summary>
    /// <remarks>
    /// The difference between this and Get method is that this one only caches items in local cache, but 
    /// uses distributed cache for versioning. To not check group generation every time an item is requested, 
    /// generation number itself is also cached in local cache. Thus, when a generation number changes, local 
    /// cached items might expire after about 5 seconds. This means that, if you use this strategy in a web farm 
    /// setup, when a change occurs in one server, other servers might continue to use old local cached data for 
    /// 5 seconds more. If this is a problem for your configuration, use DistributedCache directly.
    /// </remarks>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="cache">MemCache cache</param>
    /// <param name="cacheKey">The item key for local and distributed cache</param>
    /// <param name="localExpiration">Local expiration</param>
    /// <param name="groupKey">Group key that will hold generation (version). Can be used to expire all items
    /// that depend on it. This can be a table name. When a table changes, you change its version, and all
    /// cached data that depends on that table is expired.</param>
    /// <param name="factory">The delegate that will be called to generate value, if not found in local cache,
    /// or distributed cache, or all found items are expired.</param>
    public static T? GetLocal<T>(this ICacheFactory cache, string cacheKey, TimeSpan localExpiration,
        string groupKey, Func<T?> factory)
        where T : class
    {
        return Get(cache, cacheKey, localExpiration, TimeSpan.FromSeconds(0),
            groupKey, factory, localOnly: true, forceReload: false);
    }

    private static T? Get<T>(ICacheFactory cache, string cacheKey, TimeSpan localExpiration, TimeSpan remoteExpiration, string groupKey,
        Func<T?> factory, bool localOnly, bool forceReload) where T : class
    {
        ulong? groupGenValue = null;
        ulong? groupGenCacheValue = null;

        string itemKey = cacheKey + CacheBase.Suffix;

        var memoryCache = cache.Memory;
        var distributedCache = cache.Distributed;

        //Get random group generation value from distributed cache if exist else generate and set lazily
        ulong getGroupValue()
        {
            if (groupGenValue is not null)
                return groupGenValue.Value;

            var bytes = distributedCache.Get(groupKey);

            //If not null then Length Should be 8 byte
            groupGenValue = bytes is null || bytes.Length is not 8 ? null : BitConverter.ToUInt64(bytes);

            if (groupGenValue is null || groupGenValue is 0)
            {
                groupGenValue = Random();
                distributedCache.Set(groupKey, BitConverter.GetBytes(groupGenValue.Value));
            }

            groupGenCacheValue = groupGenValue.Value;

            //Add to memory cache, use 5 seconds from there
            memoryCache.Add(groupKey, groupGenCacheValue, CacheBase.CacheExpiration);

            return groupGenValue.Value;
        }

        //Get the random group value from memory cache group if null then get from groupValue lazily
        ulong getGroupCacheValue()
        {
            if (groupGenCacheValue is not null)
                return groupGenCacheValue.Value;

            //Check cached local value of group key 
            //It expires in 5 seconds and read from server again
            groupGenCacheValue = memoryCache.Get<object>(groupKey) as ulong?;

            //If its in local cache, return it
            if (groupGenCacheValue is not null)
                return groupGenCacheValue.Value;

            return getGroupValue();
        }

        //If not force reloaded
        if (!forceReload)
        {
            //First check local memory cache, Because local memory cache are set for 5 sec
            var cachedObj = memoryCache.Get<object>(cacheKey);

            //If item exists and not expired (group version = item version) return it
            if (cachedObj is not null)
            {
                //Check local cache with the item key and get the group version
                var itemGenerationCache = memoryCache.Get<object>(itemKey) as ulong?;

                //If exists, compare group version with local memory cache
                if (itemGenerationCache is not null && itemGenerationCache == getGroupCacheValue())
                {
                    //Local memory cached item is not expired yet
                    if (cachedObj == DBNull.Value)
                        return null;

                    return (T)cachedObj;
                }

                //Local memory cached group version item is expired, remove all information
                if (itemGenerationCache != null)
                    memoryCache.Remove(itemKey);

                memoryCache.Remove(cacheKey);
            }

            //If the request is not only for local memory cache
            //There aren't any item in local cache or expired,
            if (!localOnly)
            {
                //Now check distributed cache with the item key and get the group version
                var bytes = distributedCache.Get(itemKey);

                //If not null then Length Should be 8 byte
                var groupingVersionValue = bytes is null || bytes.Length is not 8 ? (ulong?)null : BitConverter.ToUInt64(bytes);

                //If item has version number in distributed cache and this is equal to group version
                if (groupingVersionValue is not null && groupingVersionValue.Value == getGroupValue())
                {
                    // get item from distributed cache
                    cachedObj = distributedCache.GetJson<T>(cacheKey);
                    // if item exists in distributed cache or not expired
                    if (cachedObj is not null)
                    {
                        memoryCache.Add(cacheKey, cachedObj, localExpiration);
                        memoryCache.Add(itemKey, getGroupValue(), localExpiration);
                        return (T)cachedObj;
                    }
                }
            }
        }

        if (factory is null)
            return null;

        // couldn't find valid item in local or distributed cache, produce value by calling loader
        var item = factory();

        //Add item and its version to cache
        memoryCache.Add(cacheKey, (object?)item ?? DBNull.Value, localExpiration);
        memoryCache.Add(itemKey, getGroupValue(), localExpiration);

        if (!localOnly)
        {
            // add item and generation to distributed cache
            if (remoteExpiration == TimeSpan.Zero)
            {
                distributedCache.SetJson(cacheKey, item);
                distributedCache.Set(itemKey, BitConverter.GetBytes(getGroupValue()));
            }
            else if (remoteExpiration < TimeSpan.Zero)
            {
                distributedCache.Remove(cacheKey);
                distributedCache.Remove(itemKey);
            }
            else
            {
                distributedCache.SetJson(cacheKey, item, remoteExpiration);
                distributedCache.Set(itemKey, BitConverter.GetBytes(getGroupValue()), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = remoteExpiration
                });
            }
        }

        return item;
    }

    /// <summary>
    /// Change a group value, so that all items that depend on it are expired.
    /// </summary>
    /// <param name="cache">Memory Cache</param>
    /// <param name="groupKey">Cache Group key</param>
    public static void ExpireGroupItems(this ICacheFactory cache, string groupKey)
    {
        cache.Memory.Remove(groupKey);
        cache.Distributed.Remove(groupKey);
    }

    /// <summary>
    /// Removes a key from local, distributed caches, and removes their generation version information.
    /// </summary>
    /// <param name="cache">Cache Factory</param>
    /// <param name="key">Cache key</param>
    public static void Remove(this ICacheFactory cache, string key)
    {
        string itemKey = key + CacheBase.Suffix;

        cache.Memory.Remove(key);
        cache.Memory.Remove(itemKey);
        cache.Distributed.Remove(key);
        cache.Distributed.Remove(itemKey);
    }

    /// <summary>
    /// Creates or overrides a specified entry in the local and distributed cache.
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="cache">Cache Factory</param>
    /// <param name="cacheKey">The item key for local and distributed cache</param>
    /// <param name="localExpiration">Local expiration</param>
    /// <param name="remoteExpiration">Distributed cache expiration (is usually same with local expiration)</param>
    /// <param name="groupKey">Group key that will hold generation (version). Can be used to expire all items
    /// that depend on it. This can be a table name. When a table changes, you change its version, and all
    /// cached data that depends on that table is expired.</param>
    /// <param name="value">Value to set.</param>
    public static T? Set<T>(this ICacheFactory cache, string cacheKey, TimeSpan localExpiration, TimeSpan remoteExpiration,
        string groupKey, T? value)
        where T : class
    {
        return Get(cache, cacheKey, localExpiration, remoteExpiration,
            groupKey, () => value, localOnly: false, forceReload: true);
    }

    /// <summary>
    /// Creates or overrides a specified entry in the local and distributed cache.
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="cache">Cache Factory</param>
    /// <param name="cacheKey">The item key for local and distributed cache</param>
    /// <param name="expiration">Local and distributed expiration</param>
    /// <param name="groupKey">Group key that will hold generation (version). Can be used to expire all items
    /// that depend on it. This can be a table name. When a table changes, you change its version, and all
    /// cached data that depends on that table is expired.</param>
    /// <param name="value">Value to set.</param>
    public static T? Set<T>(this ICacheFactory cache, string cacheKey, TimeSpan expiration, string groupKey, T value)
        where T : class
    {
        return Get(cache, cacheKey, expiration, expiration,
            groupKey, () => value, localOnly: false, forceReload: true);
    }

    /// <summary>
    /// Creates or overrides a specified entry in the local cache.
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    /// <param name="cache">Cache Factory</param>
    /// <param name="cacheKey">The item key for local and distributed cache</param>
    /// <param name="localExpiration">Local expiration</param>
    /// <param name="groupKey">Group key that will hold generation (version). Can be used to expire all items
    /// that depend on it. This can be a table name. When a table changes, you change its version, and all
    /// cached data that depends on that table is expired.</param>
    /// <param name="value">Value to set.</param>
    public static T? SetLocal<T>(this ICacheFactory cache, string cacheKey, TimeSpan localExpiration,
        string groupKey, T value)
        where T : class
    {
        return Get(cache, cacheKey, localExpiration, TimeSpan.FromSeconds(0),
            groupKey, () => value, localOnly: true, forceReload: true);
    }
}