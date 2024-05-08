namespace Primary.Memory.Core;
public abstract class CacheBase
{
    /// <summary>
    /// Expiration timeout for cache keys
    /// </summary>
    public static TimeSpan CacheExpiration => TimeSpan.FromSeconds(5);

    /// <summary>
    /// Suffix for cache keys
    /// </summary>
    public const string Suffix = "~Gen";

    /// <summary>
    /// Prefix for cache keys
    /// </summary>
    public const string Prefix = "Gen~";
}
