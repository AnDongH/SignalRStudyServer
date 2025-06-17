using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRStudyServer.Hubs;

[Authorize]
public partial class ChatHub
{
    public async Task SendTimeToClients(string dateTime)
    {
        await Clients.All.SendAsync("ShowTime", dateTime);
    }
}