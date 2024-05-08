using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace Primary.Memory.Test.Mock.Cache;
public class EmptyMemoryCache : IMemoryCache
{
    public ICacheEntry CreateEntry(object key)
    {
        return new EmptyCacheEntry { Key = key };
    }

    public void Dispose()
    {

    }

    public void Remove(object key)
    {

    }

    public bool TryGetValue(object key, out object? value)
    {
        value = null;
        return false;
    }
    class EmptyCacheEntry : ICacheEntry
    {
        public required object Key { get; set; }

        public object? Value { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }

        public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } = new List<PostEvictionCallbackRegistration>();

        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {

        }
    }
}