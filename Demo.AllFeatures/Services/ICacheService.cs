namespace Demo.AllFeatures.Services;

public interface ICacheService
{
    public Task<object> GetAsync(string key);
    public void Set(string key, object value);
}

