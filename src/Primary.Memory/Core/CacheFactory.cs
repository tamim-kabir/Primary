namespace Primary.Memory.Core;
/// <summary>
/// Creates a new MemCache instance internal only
/// </summary>
/// <param name="memoryCache">Memory cache</param>
/// <param name="distributedCache">Distributed cache</param>
public sealed class CacheFactory(IMemoryCache memoryCache, IDistributedCache distributedCache) : CacheBase, ICacheFactory
{
    public IMemoryCache Memory => memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    public IDistributedCache Distributed => distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
}
