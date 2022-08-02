using System.Text.Json;
using Demo.AllFeatures2.Data.Dtos;
using StackExchange.Redis;

namespace Demo.AllFeatures2.Services;

public class VisitationService : IVisitationService
{
    private readonly ILogger<VisitationService> _logger;
    private readonly IDatabase _db;

    public VisitationService(IConnectionMultiplexer connectionMultiplexer, ILogger<VisitationService> logger)
    {
        _logger = logger;
        _db = connectionMultiplexer.GetDatabase(0);
    }

    public async Task SaveVisitation(string userId)
    {
        _logger.LogInformation("SAVING OF VISITATION BEGUN FOR USER ID: {userId}", userId);
        if (DateTime.Now.Second % 9 != 0 && DateTime.Now.Second % 9 != 5 && DateTime.Now.Second % 9 != 2)
            throw new Exception("Visitation failed to record!");

        var value = await _db.StringGetAsync($"users:{userId}");
        var user = JsonSerializer.Deserialize<VisitationUser>(value);

        if (user is null)
            throw new Exception("User does not exists anymore!");

        user.Visits++;

        await _db.StringSetAsync($"users:{userId}", JsonSerializer.Serialize(user));

        _logger.LogInformation(" VISITATION SAVED FOR USER ID: {userId}", userId);
    }
}