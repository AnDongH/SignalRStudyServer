using Microsoft.AspNetCore.SignalR;

namespace SignalRStudyServer.Hubs.Filters;

// 모든 허브에 공통적으로 적용되는 필터.
// 필터는 미들웨어와 비슷한 기능을 함. 컨트롤러 == 허브, 필터 == 미들웨어 라고 보면 됨
// 필터들은 싱글톤처럼 사용됨
public class GlobalTestFilter : IHubFilter
{
    // 이녀석은 허브 필터를 만들거면 필수이고,
    public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
    {
        Console.WriteLine($"Calling hub method '{invocationContext.HubMethodName}'");
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