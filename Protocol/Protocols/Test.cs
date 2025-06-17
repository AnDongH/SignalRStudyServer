using MessagePack;

namespace Protocol.Protocols;

[MessagePackObject]
public class TestReq() : ProtocolReq(ProtocolId.Test)
{
    
}

[MessagePackObject]
public class TestRes() : ProtocolRes
{
    [Key(1)] public DateTime Time { get; set; }
}