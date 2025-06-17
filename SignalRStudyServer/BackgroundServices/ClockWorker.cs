using System.Globalization;
using Microsoft.AspNetCore.SignalR;
using SignalRStudyServer.Hubs;

namespace SignalRStudyServer.BackgroundServices;

public class ClockWorker : BackgroundService
{
    private readonly ILogger<ClockWorker> _logger;
    private readonly IHubContext<ChatHub> _chatHub;

    public ClockWorker(ILogger<ClockWorker> logger, IHubContext<ChatHub> chatHub)
    {
        _logger = logger;
        _chatHub = chatHub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            //_logger.LogInformation("Worker running at: {Time}", DateTime.Now);
            await _chatHub.Clients.All.SendAsync("ShowTime",DateTime.Now.ToString(CultureInfo.CurrentCulture), cancellationToken: stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }
}