using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRStudyServer.Authorizations;

// 특정 권한에 대한 요구 사항을 정의하는 클래스
public class AdminRequirement : 
    AuthorizationHandler<AdminRequirement, HubInvocationContext>,
    IAuthorizationRequirement
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        AdminRequirement requirement,
        HubInvocationContext resource)
    {
        var claim = context.User.FindFirst("IsAdmin");
        if (claim != null && 
            !string.IsNullOrEmpty(claim.Value) && 
            IsUserAllowedToDoThis(resource.HubMethodName, claim.Value))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
    
    private bool IsUserAllowedToDoThis(string hubMethodName, string currentUsername)
    {
        return currentUsername.Equals("true", StringComparison.OrdinalIgnoreCase) && 
               hubMethodName.Equals("SendAllMessage", StringComparison.OrdinalIgnoreCase);
    }
}