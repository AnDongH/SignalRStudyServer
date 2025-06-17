using MessagePack;

namespace Protocol.Packets;

[MessagePackObject]
public class TestPacket() : BasePacket(PacketId.None)
{
    [Key(2)] public string Message { get; set; }
}