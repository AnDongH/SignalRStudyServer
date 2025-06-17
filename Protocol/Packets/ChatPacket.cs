using MessagePack;

namespace Protocol.Packets;

[MessagePackObject]
public class ClientChatPacket() : BasePacket(PacketId.Chat)
{
    [Key(2)] public string Message { get; set; }
}