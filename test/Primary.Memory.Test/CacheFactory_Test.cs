using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Primary.Memory.Abstraction;
using Primary.Memory.Core;
using Primary.Memory.Extensions;

namespace Primary.Memory.Test;
public class CacheFactory_Test
{
    [Fact]
    public void Employee_Should_Not_Null_When_Get_From_EmptyCache()
    {
        //Arrange
        var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        var distributedCache = new MemoryDistributedCache(
            new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));

        var memCache = new CacheFactory(memoryCache, distributedCache);
        bool fromFactory = false;

        //Act
        var result = Get_Employee_From_Both_Cache(memCache, "1", ref fromFactory);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(1, result?.Id);
        Assert.Equal("Jon Limon", result?.Name);
        Assert.Equal("jon@example.com", result?.Email);
        Assert.Null(result?.Gender);
        Assert.True(fromFactory);
    }
    [Fact]
    public void Employee_Should_Not_Null_From_DistributedCache_When_Null_In_MemoryCache()
    {
        //Arrange
        var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        memoryCache.Set("UserId_1" + CacheBase.Suffix, (ulong)7512309567);//Cache Generation
        memoryCache.Set("Gen~Employee", (ulong)7512309567);//Cache Group
        memoryCache.Set<Employee?>("UserId_1", null);//Cache Item

        var distributedCache = new MemoryDistributedCache(
            new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));

        distributedCache.Set("UserId_1" + CacheBase.Suffix, BitConverter.GetBytes((ulong)7512309567));//Cache Generation
        distributedCache.Set("Gen~Employee", BitConverter.GetBytes((ulong)7512309567));//Cache Group
        distributedCache.SetJson("UserId_1", GetMockEmployeeCreate());//Cache Item

        var memCache = new CacheFactory(memoryCache, distributedCache);
        bool fromFactory = false;

        //Act
        var result = Get_Employee_From_Both_Cache(memCache, "UserId_1", ref fromFactory);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(1, result?.Id);
        Assert.Equal("Jon Limon", result?.Name);
        Assert.Equal("jon@example.com", result?.Email);
        Assert.Null(result?.Gender);
        Assert.False(fromFactory);
    }

    [Fact]
    public void Employee_Should_Not_Null_From_MemoryCache()
    {
        //Arrange
        var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        memoryCache.Set("UserId_1" + CacheBase.Suffix, (ulong)7512309567);//Cache Generation
        memoryCache.Set("Gen~Employee", (ulong)7512309567);//Cache Group
        memoryCache.Set<Employee?>("UserId_1", GetMockEmployeeCreate());//Cache Item

        var distributedCache = new MemoryDistributedCache(
            new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
        var memCache = new CacheFactory(memoryCache, distributedCache);
        bool fromFactory = false;

        //Act
        var result = Get_Employee_From_Memory_Cache(memCache, "UserId_1", ref fromFactory);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(1, result?.Id);
        Assert.Equal("Jon Limon", result?.Name);
        Assert.Equal("jon@example.com", result?.Email);
        Assert.Null(result?.Gender);
        Assert.False(fromFactory);
    }
    [Fact]
    public void Employee_Should_Override_With_New_Employee_Data()
    {
        //Arrange
        var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        var distributedCache = new MemoryDistributedCache(
            new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));

        var memCache = new CacheFactory(memoryCache, distributedCache);
        bool fromFactory = false;

        //Act
        var OldData = Get_Employee_From_Both_Cache(memCache, "UserId_1", ref fromFactory);

        var result = Override_Employee_With_New_Data(memCache, "UserId_1", "Gen~Employee");

        //Assert
        Assert.NotNull(OldData);
        Assert.NotNull(result);
        Assert.Equal(1, result?.Id);
        Assert.NotEqual(OldData?.Name, result?.Name);//Name Updated Here 
        Assert.Equal("jon@example.com", result?.Email);
        Assert.Null(result?.Gender);
        Assert.True(fromFactory);
    }
    [Fact]
    public void Employee_Should_Override_With_New_Employee_Data_When_Both_Cache_Expire_Time_Are_Same()
    {
        //Arrange
        var memoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));

        var distributedCache = new MemoryDistributedCache(
            new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));

        var memCache = new CacheFactory(memoryCache, distributedCache);
        bool fromFactory = false;

        //Act
        var OldData = Get_Employee_From_Both_Cache(memCache, "UserId_1", ref fromFactory);
        //Updated Data
        var result = Override_Employee_With_New_Data_When_Expire_Time_Same(memCache, "UserId_1", "Gen~Employee");

        //Assert
        Assert.NotNull(OldData);
        Assert.NotNull(result);
        Assert.Equal(1, result?.Id);
        Assert.NotEqual(OldData?.Name, result?.Name);//Name Updated Here 
        Assert.Equal("jon@example.com", result?.Email);
        Assert.Null(result?.Gender);
        Assert.True(fromFactory);
    }
    public Employee GetMockEmployeeCreate()
    {
        return new Employee { Id = 1, Name = "Jon Limon", Email = "jon@example.com" };
    }

    public Employee? Get_Employee_From_Both_Cache(ICacheFactory cache, string key, ref bool fromFactory)
    {
        bool factory = false;
        var result = cache.Get(key, TimeSpan.Zero, TimeSpan.FromDays(1), "Gen~Employee",
            () =>
            {
                factory = true;
                return GetMockEmployeeCreate();
            });

        fromFactory = factory;
        return result;
    }

    public Employee? Get_Employee_From_Memory_Cache(ICacheFactory cache, string key, ref bool fromFactory)
    {
        bool factory = false;
        var result = cache.GetLocal(key, TimeSpan.Zero, "Gen~Employee",
            () =>
            {
                factory = true;
                return GetMockEmployeeCreate();
            });

        fromFactory = factory;
        return result;
    }

    public Employee? Override_Employee_With_New_Data(ICacheFactory cache, string key, string groupKay)
    {
        var employee = GetMockEmployeeCreate();
        employee.Name = "Lisa";
        var result = cache.Set(key, TimeSpan.Zero, TimeSpan.FromDays(1), groupKay, employee);

        return result;
    }
    public Employee? Override_Employee_With_New_Data_When_Expire_Time_Same(ICacheFactory cache, string key, string groupKay)
    {
        var employee = GetMockEmployeeCreate();
        employee.Name = "Lisa";
        var result = cache.Set(key, TimeSpan.Zero, groupKay, employee);

        return result;
    }

    public Employee? Override_Employee_In_Local_Memory_Only(ICacheFactory cache, string key, string groupKay)
    {
        var employee = GetMockEmployeeCreate();
        employee.Name = "Lisa";
        var result = cache.SetLocal(key, TimeSpan.Zero, groupKay, employee);

        return result;
    }
    public class Employee
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Gender { get; set; }
        public string? Email { get; set; }
    }
}
