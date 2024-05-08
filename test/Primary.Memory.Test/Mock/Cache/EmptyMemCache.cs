using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Primary.Memory.Abstraction;
using Primary.Memory.Core;

namespace Primary.Memory.Test.Mock.Cache;
public class EmptyMemCache : CacheBase, ICacheFactory
{
    public IMemoryCache Memory => new EmptyMemoryCache();

    public IDistributedCache Distributed => new EmptyDistributedCache();
}
