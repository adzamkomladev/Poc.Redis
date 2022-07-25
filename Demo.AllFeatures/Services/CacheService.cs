using System.Text.Json;
using StackExchange.Redis;

namespace Demo.AllFeatures.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase _db;

    public CacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase(0);
    }
    public async Task<object> GetAsync(string key)
    {
        return await _db.StringGetAsync(key);
    }

    public void Set(string key, object value)
    {
        throw new NotImplementedException();
    }
}