using MessagePack;

namespace Protocol.Packets;

[MessagePackObject]
[Union(2, typeof(ClientChatPacket))]
[Union(4, typeof(TestPacket))]
public abstract class BasePacket(PacketId packetId)
{
    [Key(0)] public PacketId PacketId { get; set; } = packetId;
    [Key(1)] public long Suid { get; set; }
}