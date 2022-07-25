using System.Diagnostics;
using System.Text.Json;
using Demo.AllFeatures.Data.Dtos;
using Demo.AllFeatures.Models;
using StackExchange.Redis;

namespace Demo.AllFeatures.Services;

public class UserService : IUserService
{
    private readonly IDatabase _db;

    public UserService(IConnectionMultiplexer connectionMultiplexer)
    {
        _db = connectionMultiplexer.GetDatabase(0);
    }

    public async Task<UserCreated> CreateUserAsync(CreateUser body)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Name = body.Name,
            Email = body.Email,
            Visits = 1,
            CreatedAt = DateTime.Now,
        };

        //var entries = user.GetType()
        //    .GetProperties()
        //    .Select(prop => new HashEntry(prop.Name, (RedisValue)prop.GetValue(user, null)!))
        //    .ToArray();

        await _db.StringSetAsync($"users:{user.Id}", JsonSerializer.Serialize<User>(user));

        return new UserCreated
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Visits = user.Visits,
            CreatedAt = user.CreatedAt,
        };
    }

    public async Task<FindUser?> FindUserViaIdAsync(string id)
    {
        var value = await _db.StringGetAsync($"users:{id}");

        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<FindUser>(value);
    }
}