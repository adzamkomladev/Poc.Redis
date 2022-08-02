using Demo.AllFeatures2.Services;
using StackExchange.Redis;

namespace Demo.AllFeatures2.HostedServices;

public class UserVisitedHostedService : BackgroundService
{
    private readonly ILogger<UserVisitedHostedService> _logger;
    private readonly IVisitationService _visitationService;
    private readonly IDatabase _db;

    public UserVisitedHostedService(ILogger<UserVisitedHostedService> logger, IConnectionMultiplexer connectionMultiplexer, IVisitationService visitationService)
    {
        _logger = logger;
        _visitationService = visitationService;
        _db = connectionMultiplexer.GetDatabase(0);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create stream and group
        if (!(await _db.KeyExistsAsync("user.visits")) ||
            (await _db.StreamGroupInfoAsync("user.visits")).All(x => x.Name != "visit.consumers"))
        {
            await _db.StreamCreateConsumerGroupAsync("user.visits", "visit.consumers", "0-0", true);
        }

        _logger.LogInformation("Start things up");
        var id = string.Empty;
        Dictionary<string, string> dict = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Begin new streaming");
                if (!string.IsNullOrEmpty(id))
                {
                    await _db.StreamAcknowledgeAsync("user.visits", "visit.consumers", id);
                    id = string.Empty;
                }

                var result =
                    await _db.StreamReadGroupAsync("user.visits", "visit.consumers", "visit.consumers-1", ">", 1);

                if (result.Any())
                {
                    _logger.LogInformation("Begin work on new streaming");

                    id = result.First().Id;
                    dict = ParseResult(result.First());

                    _logger.LogInformation("ITEMS BEING RETURNED INCLUDE USERID: {userId} and RETRIES: {retries}", dict["user_id"], dict["retries"]);

                    if (int.Parse(dict["retries"]) > 3)
                        continue;

                    await _visitationService.SaveVisitation(dict["user_id"]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Something failed somewhere {exception}", ex);
                if (dict is null)
                    return;

                await _db.StreamAddAsync("user.visits",
                    new NameValueEntry[]
                    {
                        new("user_id", dict["user_id"]),
                        new NameValueEntry("retries", int.Parse(dict["retries"]) + 1)
                    });

            }

            //await Task.Delay(200, stoppingToken);
        }

    }

    private static Dictionary<string, string> ParseResult(StreamEntry entry) => entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
}