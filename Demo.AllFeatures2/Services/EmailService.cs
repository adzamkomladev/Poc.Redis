using System.Text.Json;
using StackExchange.Redis;

namespace Demo.AllFeatures2.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IDatabase _db;

    public EmailService(IConnectionMultiplexer connectionMultiplexer, ILogger<EmailService> logger)
    {
        _logger = logger;
        _db = connectionMultiplexer.GetDatabase(0);
    }


    public async Task SendEmailAsync(string email)
    {
        // Send Email
        _logger.LogInformation("EMAIL SENT TO: {email}", email);

        var emailLog = JsonSerializer.Serialize(new
        {
            Email = email,
            SentOn = DateTime.Now,
            Status = "SUCCESS"
        });

        var id = Guid.NewGuid().ToString();

        await _db.StringSetAsync($"email:logs:{id}", emailLog);
        await _db.StringIncrementAsync("email:success", 1);
    }

    public async Task FailEmailAsync(string email)
    {
        // Send Email
        Console.WriteLine("EMAIL FAILED: {email}", email);

        var emailLog = JsonSerializer.Serialize(new
        {
            Email = email,
            SentOn = DateTime.Now,
            Status = "FAILED"
        });

        var id = Guid.NewGuid().ToString();

        await _db.StringSetAsync($"email:logs:{id}", emailLog);
        await _db.StringIncrementAsync("email:failed", 1);
    }
}