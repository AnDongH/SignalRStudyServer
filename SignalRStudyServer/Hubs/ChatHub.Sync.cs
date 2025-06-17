using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRStudyServer.Hubs;

[Authorize]
public partial class ChatHub
{
    private static Dictionary<string, List<string>> _gameRooms = new ();
    private static SemaphoreSlim _gameRoomNamesLock = new (1,1);
    
    public async Task SyncPlayerPositionAsync(string groupName, float x, float y, float z)
    {
        await Clients.OthersInGroup(groupName).SendAsync("SyncPlayerPosition", x, y, z, Context.User?.Identity?.Name);
    }
    
    public async Task EnterGameRoomAsync(string roomName)
    {
        try
        {
            await _gameRoomNamesLock.WaitAsync();
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);       
            if (_gameRooms.ContainsKey(roomName))
            {
                if (_gameRooms[roomName].Contains(Context.User?.Identity?.Name))
                {
                    await Clients.Caller.SendAsync("OnErrored", "이미 그룹에 속해있습니다.");
                    return;
                }
                _gameRooms[roomName].Add(Context.User?.Identity?.Name);
            }
            else
            {
                _gameRooms[roomName] = new List<string> { Context.User?.Identity?.Name };
            }
        }
        finally
        {
            _gameRoomNamesLock.Release();
        }
        
        await Clients.Group(roomName).SendAsync("OnEnteredGameRoom", Context.User?.Identity?.Name, _gameRooms[roomName], roomName);
    }

    public async Task LeaveGameRoomAsync(string roomName)
    {
        try
        {
            await _gameRoomNamesLock.WaitAsync();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            if (_gameRooms.ContainsKey(roomName))
            {
                if (_gameRooms[roomName].Contains(Context.User?.Identity?.Name))
                {
                    _gameRooms[roomName].Remove(Context.User?.Identity?.Name);   
                }
                else
                {
                    await Clients.Caller.SendAsync("OnErrored", "그룹에 속해있지 않습니다.");
                    return;
                }
                
                if (_gameRooms[roomName].Count == 0)
                {
                    _gameRooms.Remove(roomName);
                }
            }
        } 
        finally
        {
            _gameRoomNamesLock.Release();
        }
        
        var task1 = Clients.Group(roomName).SendAsync("OnLeftGameRoom", Context.User?.Identity?.Name);
        var task2 = Clients.Caller.SendAsync("OnLeftGameRoom", Context.User?.Identity?.Name);
        await Task.WhenAll(task1, task2);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            await _gameRoomNamesLock.WaitAsync();
            var roomNames = new List<string>();
            foreach (var room in _gameRooms)
            {
                if (room.Value.Contains(Context.User?.Identity?.Name))
                {
                    room.Value.Remove(Context.User?.Identity?.Name);
                    if (room.Value.Count == 0)
                    {
                        roomNames.Add(room.Key);
                    }
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Key);
                    await Clients.Group(room.Key).SendAsync("OnLeftGameRoom", Context.User?.Identity?.Name);
                }
            }
            
            foreach (var roomName in roomNames)
            {
                _gameRooms.Remove(roomName);
            }
        }
        finally
        {
            _gameRoomNamesLock.Release();
        }
    }
}