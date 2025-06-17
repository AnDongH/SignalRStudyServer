using Microsoft.AspNetCore.SignalR;

namespace SignalRStudyServer.Hubs.Filters;

// 필터들은 싱글톤처럼 사용됨
public class ChatHubLocalTestFilter : IHubFilter
{
    // 이녀석은 허브 필터를 만들거면 필수이고,
    public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
    {
        Console.WriteLine("ChatHubLocalTestFilter");
        try
        {
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception calling '{invocationContext.HubMethodName}': {ex}");
            throw;
        }
    }

    // Optional method
    // 이건 허브에 연결할 때 호출됨
    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        return next(context);
    }

    // Optional method
    // 이건 허브에 연결 해제할 때 호출됨
    public Task OnDisconnectedAsync(
        HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        return next(context, exception);
    }
}