namespace Primary.Memory.Extensions;

/// <summary>
/// Contains extensions methods to work with IMemoryCache provider.
/// </summary>
public static class MemoryCacheExtensions
{
    /// <summary>
    /// Adds a value to cache with a given key
    /// </summary>
    /// <param name="cache">The <see cref="IMemoryCache"/> in which to store the data.</param>
    /// <param name="key">The key of the entry to add.</param>
    /// <param name="value">The value <c>TItem</c> to associate with the key.</param>
    /// <param name="expiration">Expire time (Use TimeSpan.Zero to hold value with no expiration). 
    /// If expiration is negative value nothing is added to the cache but removed if exists.</param>
    /// <returns>The value <c>TItem</c> that was set.</returns>
    /// <exception cref="ArgumentNullException">When <see cref="IMemoryCache"/> is null</exception>
    public static TItem Add<TItem>(this IMemoryCache cache, object key, TItem value, TimeSpan expiration)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        if (expiration == TimeSpan.Zero)
            return cache.Set(key, value);

        if (expiration > TimeSpan.Zero)
            return cache.Set(key, value, expiration);

        cache.Remove(key);

        return value;
    }

    /// <summary>
    /// Reads the value with specified key from the <c>IMemoryCache</c> cache. If it doesn't exists in cache, calls the <c>factory</c> 
    /// function to generate value (from database, file etc.) and adds it to the cache. If <c>factory</c> returns a null value, 
    /// it is not written to the cache.
    /// </summary>
    /// <typeparam name="TItem">The type of the object to get.</typeparam>
    /// <param name="cache">The <see cref="IMemoryCache"/> instance this method extends.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="expiration">Expiration <see cref="TimeSpan"/> associated with this key.</param>
    /// <param name="factory">The factory that creates the <c>TItem</c> value, if the key does not exist in the cache.</param>
    /// <returns>The value <c>TItem</c> associated with this key, or <c>default(TItem)</c> if the key is not present.</returns>
    /// <exception cref="ArgumentNullException">When <see cref="IMemoryCache"/> is null</exception>
    public static TItem? Get<TItem>(this IMemoryCache cache, object key, TimeSpan expiration, Func<TItem?> factory) where TItem : class
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        var obj = cache.Get<object>(key);

        if (obj == DBNull.Value)
            return null;

        if (obj is null)
        {
            if (factory is null)
                return null;

            var item = factory();
            if (item is null)
                return null;

            cache.Add(key, (object?)item, expiration);
            return item;
        }

        return (TItem)obj;
    }

    /// <summary>
    /// Reads the value of given type with specified key from the local cache. If the value doesn't exist or not
    /// of given type, it returns null.
    /// </summary>
    /// <typeparam name="TItem">The type of the object to get.</typeparam>
    /// <param name="cache">The <see cref="IMemoryCache"/> instance this method extends.</param>
    /// <param name="key">The key of the value to get.</param>
    /// <returns>The value <c>TItem</c> associated with this key, or <c>default(TItem)</c> if the key is not present.</returns>
    /// <exception cref="ArgumentNullException">When <see cref="IMemoryCache"/> is null</exception>
    public static TItem? TryGet<TItem>(this IMemoryCache cache, string key) where TItem : class
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));
        return cache.Get<object>(key) as TItem;
    }

    /// <summary>
    /// Removes all items from the cache (avoid except unit tests).
    /// </summary>
    /// <param name="cache">The <see cref="IMemoryCache"/> instance this method extends.</param>
    /// <exception cref="ArgumentNullException">When <see cref="IMemoryCache"/> is null</exception>
    /// <exception cref="NotImplementedException">When <c>typeof(MemoryCache)</c> is not <c>IMemoryCache</c></exception>
    public static void RemoveAll(this IMemoryCache cache)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        if (cache is MemoryCache memCache)
        {
            memCache.Compact(1.0);
            return;
        }

        throw new NotImplementedException();
    }
}
