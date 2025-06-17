using MessagePack;

namespace Protocol.Protocols;

[MessagePackObject]
public class LoginReq() : ProtocolReq(ProtocolId.Login)
{
    [Key(2)] public string Id { get; set; }
    [Key(3)] public string Password { get; set; }
    [Key(4)] public bool IsAdmin { get; set; }
}

[MessagePackObject]
public class LoginRes : ProtocolRes
{
    [Key(1)] public long Suid { get; set; }
    [Key(2)] public string AuthToken { get; set; }
}