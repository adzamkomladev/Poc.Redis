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

        var newUser = JsonSerializer.Serialize<User>(user);

        await _db.StringSetAsync($"users:{user.Id}", newUser);
        await _db.PublishAsync("UserRegistration", newUser);

        if (!(await _db.KeyExistsAsync("user.visits")) ||
            (await _db.StreamGroupInfoAsync("user.visits")).All(x => x.Name != "visit.consumers"))
        {
            await _db.StreamCreateConsumerGroupAsync("user.visits", "visit.consumers", "0-0", true);
        }

        await _db.StreamAddAsync("user.visits",
            new NameValueEntry[]
            {
                new("user_id", user.Id),
                new NameValueEntry("retries", 0)
            });

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