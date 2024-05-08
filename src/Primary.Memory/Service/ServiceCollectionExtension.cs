using Microsoft.Extensions.DependencyInjection;

namespace Primary.Memory.Service;
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Add both MemoryCache & DistributedMemoryCache with few Configuration and Extension Methods
    /// </summary>
    /// <param name="services">This <c>IServiceCollection</c> extension</param>
    /// <returns><c>IServiceCollection</c></returns>
    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(services));

        services.AddMemoryCache();
        services.AddDistributedMemoryCache();
        services.AddSingleton<ICacheFactory, CacheFactory>();
        return services;
    }
}
