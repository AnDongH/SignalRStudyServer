using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Protocol.Packets;
using SignalRStudyServer.Hubs.Filters.Attributes;

namespace SignalRStudyServer.Hubs;

// 허브는 일시적임. 호출할 때마다 새 인스턴스가 생겨남.
// 허브가 아닌 다른곳에서 클라이언트에 메세지를 보내고싶으면, IHubContext를 주입받아서 사용하면 됨.
// 비동기 호출은 무조건 await으로 대기해야함 허브가 종료되면 호출이 실패할 수 있음
// 허브마다 연결이 생김. 또한 유저마다 연결 id가 다름.
[Authorize]
public partial class ChatHub : Hub
{
    
    private static Dictionary<string, List<string>> _groups = new ();
    private static Lock _groupNamesLock = new ();
    
    // 연결된 모든 클라이언트에게 메시지 전송
    // SignalR은 마지막으로 요청된 WebSocket 업그레이드 Http 요청에서 AuthenticateResult가 결정됨
    // 그 말은 연결만 계속 유지한다면 토큰이 만료된다 하더라도, 연결된 클라이언트는 계속해서 메시지를 받을 수 있다는 것임.
    // 따라서 주기적으로 연결 갱신이 필요함. 여기서는 일단 패스ㅋ
    // 매개변수가 늘어나면 클라이언트와, 서버 둘 다 고치는 것이 번거로움.
    // 따라서 매개변수는 1개의 래핑 데이타 클래스에 담는 것이 효율적임
    // ClientChatPacket처럼
    [LanguageFilter(0)]
    [Authorize("Admin")] // 해당 허브 메서드에 접근하기 위해서는 Admin 권한이 필요함
    public async Task SendAllMessage(ClientChatPacket req)
    {
        await Clients.All.SendAsync("ReceiveAllMessage", req?.Message);

        // SendAsync는 확장 메서드이고 이건 기본 지원 메서드. .NET은 SendAsync를 권장하고, 인자를 가변 배열로 처리해야 할 때 사용함.
        //await Clients.All.SendCoreAsync("ReceiveAllMessage", [user, message]);


        // var context = Context.GetHttpContext();
        // 해당 기능은 HttpContext에 접근할 수 있는 기능임.
        // 해당 함수는 HTTP 관련 요청이여야만 null 이 아니기에 null 체크를 해줘야함.
        // 웬만하면 HttpContext를 사용하지 말자.
    }
    
    // 해당 메서드를 호출한 클라이언트를 제외한 연결된 모든 클라이언트에게 메시지 전송
    public async Task SendOthersMessage(ClientChatPacket req)
    {
        await Clients.Others.SendAsync("ReceiveOthersMessage", req?.Message);
    }
    
    // 해당 메서드를 호출한 클라이언트에게만 메시지 전송
    public async Task SendCallerMessage(ClientChatPacket req)
    {
        await Clients.Caller.SendAsync("ReceiveCallerMessage", req?.Message);
    }
    
    // 특정 커넥션들을 제외한 모든 클라이언트에게 메시지 전송
    public async Task SendAllExceptMessage(string[] connectionIds, ClientChatPacket req)
    {
        await Clients.AllExcept(connectionIds).SendAsync("ReceiveAllExceptMessage", req?.Message);
    }
    
    // 특정 커넥션에게만 메시지 전송
    public async Task SendClientMessage(string connectionId, ClientChatPacket req)
    {
        await Clients.Client(connectionId).SendAsync("ReceiveClientMessage", req?.Message);
    }
    
    // 특정 그룹에 속한 모든 클라이언트에게 메시지 전송
    public async Task SendGroupMessage(string groupName, ClientChatPacket req)
    {
        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", req?.Message);
    }
    
    // 특정 그룹에 속한 모든 클라이언트에게 메시지 전송 (여러 그룹)
    public async Task SendGroupsMessage(string[] groupNames, ClientChatPacket req)
    {
        await Clients.Groups(groupNames).SendAsync("ReceiveGroupsMessage", req?.Message);
    }
    
    // 특정 그룹에서 특정 커넥션 제외하고 메시지 전송
    public async Task SendGroupExceptMessage(string groupName, string connectionId, ClientChatPacket req)
    {
        await Clients.GroupExcept(groupName, [connectionId]).SendAsync("ReceiveGroupExceptMessage", req?.Message);
    }
    
    // 허브 메서드를 호출한 클라이언트 제외하고 특정 그룹 에 속한 모든 클라이언트에게 메시지 전송
    public async Task SendOthersInGroupMessage(string groupName, ClientChatPacket req)
    {
        await Clients.OthersInGroup(groupName).SendAsync("ReceiveOthersInGroupMessage", req?.Message);
    }
    
    // 특정 유저에게 연결된 모든 커넥션에 메시지 전송
    // 예를 들어서 유저가 Chrome, Edge, Firefox에서 서비스에 각각 연결되어있다면, 각각의 커넥션에 메시지를 전송함.
    // ASP.NET Core의 Identity 시스템과 같이 사용
    public async Task SendUserMessage(ClientChatPacket req)
    {
        // 이거는 하기 전에 커넥션 매핑 등록을 해줘야함. 여기서 내가 원하는 값으로 커넥션 매핑이 가능
        await Clients.User(Context.User?.Identity?.Name!).SendAsync("ReceiveUserMessage", req?.Message);
    }
    
    // 특정 유저에게 연결된 모든 커넥션에 메시지 전송 (여러 유저)
    public async Task SendUsersMessage(string[] userIds, ClientChatPacket req)
    {
        await Clients.Users(userIds).SendAsync("ReceiveUsersMessage", req?.Message);
    }
    
    public async Task EnterGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
        {
            await Clients.Caller.SendAsync("OnErrored", "그룹 이름이 비었습니다.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        bool flag;
        
        lock (_groupNamesLock)
        {
            if (!_groups.ContainsKey(groupName))
            {
                _groups[groupName] = new List<string>();
            }

            flag = _groups[groupName].Contains(Context.ConnectionId);
            if (!flag) 
                _groups[groupName].Add(Context.ConnectionId);
        }
        
        if (flag) await Clients.Caller.SendAsync("OnErrored", "이미 그룹에 속해있습니다.");
        else
        {
            await Clients.Group(groupName).SendAsync("OnEnteredGroupAll", $"{Context.User?.Identity?.Name}님이 {groupName} 그룹에 들어왔습니다.");
            await Clients.Caller.SendAsync("OnEnteredGroupCaller", groupName);
        }
    }
    
    public async Task LeaveGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
        {
            await Clients.Caller.SendAsync("OnErrored", "그룹 이름이 비었습니다.");
            return;
        }
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        bool flag;
        
        lock (_groupNamesLock)
        {
            flag = _groups.ContainsKey(groupName);
            if (flag)
            {
                if (_groups[groupName].Contains(Context.ConnectionId))
                {
                    _groups[groupName].Remove(Context.ConnectionId);   
                }
                else
                {
                    flag = false;
                }
                
                if (_groups[groupName].Count == 0)
                {
                    _groups.Remove(groupName);
                }
            }
        }
        
        if (flag)
        {
            await Clients.Group(groupName).SendAsync("OnLeftGroupAll", $"{Context.User?.Identity?.Name}님이 {groupName} 그룹에서 나갔습니다.");
            await Clients.Caller.SendAsync("OnLeftGroupCaller");
        }
        else await Clients.Caller.SendAsync("OnErrored", "그룹에 속하지 않거나, 그룹이 존재하지 않습니다.");
    }
    
    public async Task GetGroups()
    {
        await Clients.Caller.SendAsync("OnGetGroups", _groups.Keys.ToList());
    }
    
    // 이렇게 서버에서 클라이언트에게 결과를 요청하는 것도 가능
    public async Task ServerRequestClientResult()
    {
        var msg = await Clients.Caller.InvokeAsync<string>("OnServerRequestClientResult", CancellationToken.None);
        Console.WriteLine(msg);
    }
    
    // 유저마다 연결될 때 1번만 호출됨
    public override Task OnConnectedAsync()
    {
        return Task.CompletedTask;
    }
}