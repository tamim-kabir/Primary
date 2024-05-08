namespace Primary.Memory.Extensions;
/// <summary>
/// Contains extensions methods to work with IDistributedCache provider.
/// </summary>
public static class DistributedCacheExtensions
{
    /// <summary>
    /// Sets a value in the cache as byte[], string, or using JSON serialization depending on type of TValue
    /// </summary>
    /// <typeparam name="TValue">Value type to set in cache</typeparam>
    /// <param name="cache">The <see cref="IDistributedCache"/> in which to store the data.</param>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache. If null then remove from cache</param>
    /// <exception cref="ArgumentNullException">When <see cref="IDistributedCache"/> is null</exception>
    public static void SetJson<TValue>(this IDistributedCache cache, string key, TValue? value) where TValue : class
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        if (value is null)
            cache.Remove(key);
        else if (value is string s)
            cache.Set(key, Encoding.UTF8.GetBytes(s));
        else if (value is byte[] bytes)
            cache.Set(key, bytes);
        else
            cache.SetString(key, JSON.Stringify(value));
    }
    /// <summary>
    /// Sets a value in the cache as byte[], string, or using JSON serialization depending on type of TValue
    /// Async version of SetJson
    /// </summary>
    /// <typeparam name="TValue">Value type to set in cache</typeparam>
    /// <param name="cache">The <see cref="IDistributedCache"/> in which to store the data.</param>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The value to set in the cache. If null then remove from cache</param>
    /// <exception cref="ArgumentNullException">When <see cref="IDistributedCache"/> is null</exception>
    public static async Task SetJsonAsync<TValue>(this IDistributedCache cache, string key, TValue? value, CancellationToken cancellationToken) where TValue : class
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        if (value is null)
            cache.Remove(key);
        else if (value is string s)
            cache.Set(key, Encoding.UTF8.GetBytes(s));
        else if (value is byte[] bytes)
            cache.Set(key, bytes);
        else
            await cache.SetStringAsync(key, JSON.Stringify(value), cancellationToken);
    }

    /// <summary>
    /// Sets a value in the cache as byte[], string, or using JSON serialization depending on type of TValue with expiration time
    /// </summary>
    /// <typeparam name="TValue">Value type to set in cache</typeparam>
    /// <param name="cache">The <see cref="IDistributedCache"/> in which to store the data.</param>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The <c>TValue</c> of value to set in the cache. If null then remove from cache</param>
    /// <param name="expiration">The cache options value expiration time span. if less then zero then remove cache</param>
    /// <exception cref="ArgumentNullException">When <see cref="IDistributedCache"/> is null</exception>
    public static void SetJson<TValue>(this IDistributedCache cache, string key, TValue? value, TimeSpan expiration) where TValue : class
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        if (value is null || expiration < TimeSpan.Zero)
        {
            cache.Remove(key);
            return;
        }

        if (expiration == TimeSpan.Zero)
        {
            cache.SetJson(key, value);
            return;
        }

        DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        if (value is string s)
            cache.Set(key, Encoding.UTF8.GetBytes(s), options);

        else if (value is byte[] value2)
            cache.Set(key, value2, options);

        else
            cache.SetString(key, JSON.Stringify(value), options);

    }
    /// <summary>
    /// Sets a value in the cache as byte[], string, or using JSON serialization depending on type of TValue with expiration time
    /// </summary>
    /// <typeparam name="TValue">Value type to set in cache</typeparam>
    /// <param name="cache">The <see cref="IDistributedCache"/> in which to store the data.</param>
    /// <param name="key">A string identifying the requested value.</param>
    /// <param name="value">The <c>TValue</c> of value to set in the cache. If null then remove from cache</param>
    /// <param name="expiration">The cache options value expiration time span. if less then zero then remove cache</param>
    /// <exception cref="ArgumentNullException">When <see cref="IDistributedCache"/> is null</exception>
    public static Task SetJsonAsync<TValue>(this IDistributedCache cache, string key, TValue? value, TimeSpan expiration, CancellationToken cancellationToken) where TValue : class
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        if (value is null || expiration < TimeSpan.Zero)
        {
            return cache.RemoveAsync(key);
        }

        if (expiration == TimeSpan.Zero)
        {
            return cache.SetJsonAsync(key, value, cancellationToken);
        }

        DistributedCacheEntryOptions options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };
        if (value is string s)
            return cache.SetAsync(key, Encoding.UTF8.GetBytes(s), options, cancellationToken);

        else if (value is byte[] value2)
            return cache.SetAsync(key, value2, options, cancellationToken);

        else
            return cache.SetStringAsync(key, JSON.Stringify(value), options, cancellationToken);

    }

    /// <summary>
    /// Sets a value from the cache as byte[], string, or using JSON deserialization depending on type.
    /// </summary>
    /// <typeparam name="TValue">Type for deserialize</typeparam>
    /// <param name="cache">The <see cref="IDistributedCache"/> in which to get the data.</param>
    /// <param name="key">A string identifying the requested value.</param>
    /// <returns>The value <c>TValue</c> that was set.</returns>
    /// <exception cref="ArgumentNullException">When <see cref="IDistributedCache"/> is null</exception>
    public static TValue? GetJson<TValue>(this IDistributedCache cache, string key) where TValue : class
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        byte[]? array = cache.Get(key);
        if (array is null)
        {
            return null;
        }

        if (typeof(TValue) == typeof(byte[]))
        {
            return (TValue)(object)array;
        }

        string @string = Encoding.UTF8.GetString(array);
        if (typeof(TValue) == typeof(string))
        {
            return (TValue)(object)@string;
        }

        return JSON.Parse<TValue>(@string);
    }

    /// <summary>
    /// Sets a value from the cache as byte[], string, or using JSON deserialization depending on type.
    /// Async version GetJson
    /// </summary>
    /// <typeparam name="TValue">Type for deserialize</typeparam>
    /// <param name="cache">The <see cref="IDistributedCache"/> in which to get the data.</param>
    /// <param name="key">A string identifying the requested value.</param>
    /// <returns>The value <c>TValue</c> that was set.</returns>
    /// <exception cref="ArgumentNullException">When <see cref="IDistributedCache"/> is null</exception>
    public static async Task<TValue?> GetJsonAsync<TValue>(this IDistributedCache cache, string key, CancellationToken cancellationToken) where TValue : class
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(cache));

        byte[]? array = await cache.GetAsync(key, cancellationToken);
        if (array is null)
        {
            return null;
        }

        if (typeof(TValue) == typeof(byte[]))
        {
            return (TValue)(object)array;
        }

        string @string = Encoding.UTF8.GetString(array);
        if (typeof(TValue) == typeof(string))
        {
            return (TValue)(object)@string;
        }

        return JSON.Parse<TValue>(@string);
    }
}