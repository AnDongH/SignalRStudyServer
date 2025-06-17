using MessagePack;

namespace Protocol.Protocols;

[MessagePackObject]
public class RegisterReq() : ProtocolReq(ProtocolId.Register)
{
    [Key(2)] public string Id { get; set; }
    [Key(3)] public string Password { get; set; }
}

[MessagePackObject]
public class RegisterRes : ProtocolRes
{
    
}