using Microsoft.AspNetCore.SignalR;
using Protocol.Packets;
using SignalRStudyServer.Hubs.Filters.Attributes;

namespace SignalRStudyServer.Hubs.Filters;

public class LanguageFilter : IHubFilter
{
    // populated from a file or inline
    // 어트리뷰트를 이용해서 특정 메서드에 필터 적용하는법
    private List<string> bannedPhrases = new List<string> { "fuck", "poob" };

    public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, 
        Func<HubInvocationContext, ValueTask<object>> next)
    {
        var languageFilter = (LanguageFilterAttribute)Attribute.GetCustomAttribute(invocationContext.HubMethod, typeof(LanguageFilterAttribute));
        if (languageFilter != null &&
            invocationContext.HubMethodArguments.Count > languageFilter.FilterArgument &&
            invocationContext.HubMethodArguments[languageFilter.FilterArgument] is ClientChatPacket packet)
        {
            foreach (var bannedPhrase in bannedPhrases)
            {
                packet.Message = packet.Message.Replace(bannedPhrase, "***");
            }

            var arguments = invocationContext.HubMethodArguments.ToArray();
            arguments[languageFilter.FilterArgument] = packet;
            invocationContext = new HubInvocationContext(invocationContext.Context,
                invocationContext.ServiceProvider,
                invocationContext.Hub,
                invocationContext.HubMethod,
                arguments);
        }

        return await next(invocationContext);
    }
}