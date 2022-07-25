using System.Runtime.CompilerServices;
using System.Text.Json;
using Demo.AllFeatures2.Data.Dtos;
using Demo.AllFeatures2.Services;
using StackExchange.Redis;

namespace Demo.AllFeatures2.HostedServices;

public class UserRegisteredHostedService : BackgroundService
{
    private readonly ILogger<UserRegisteredHostedService> _logger;
    private readonly IEmailService _email;
    private readonly IDatabase _db;

    public UserRegisteredHostedService(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<UserRegisteredHostedService> logger,
        IEmailService email)
    {
        _logger = logger;
        _email = email;
        _db = connectionMultiplexer.GetDatabase(0);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _db.Multiplexer.GetSubscriber().SubscribeAsync("RegisteredUser", async (channel, message) =>
        {
            _logger.LogInformation("REGISTERED USER SUBSCRIBER EXECUTION");
            if (message.IsNullOrEmpty)
                return;

            await HandleRegisteredUser(message);
        });
    }


    private async Task HandleRegisteredUser(string message)
    {
        var user = JsonSerializer.Deserialize<RegisteredUser>(message);
        if (user?.Email is null)
            return;

        try
        {
            if (user?.Email != null) await _email.SendEmailAsync(user.Email);
        }
        catch (Exception e)
        {
            var retries = (int)(await _db.StringGetAsync($"emails:retries:{user?.Id}"));

            if (retries > 3)
            {
                await _email.FailEmailAsync(user.Email);
                await _db.StringGetDeleteAsync($"email:retries:{user?.Id}");
                return;
            }

            await _db.PublishAsync("RegisteredUser", message);
            await _db.StringIncrementAsync($"email:retries:{user?.Id}", 1);
        }
    }
}