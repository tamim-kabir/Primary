namespace Primary.Memory.Abstraction;
public interface ICacheFactory
{
    /// <summary>
    /// Gets the memory cache
    /// </summary>
    IMemoryCache Memory { get; }

    /// <summary>
    /// Gets the distributed cache
    /// </summary>
    IDistributedCache Distributed { get; }
}
