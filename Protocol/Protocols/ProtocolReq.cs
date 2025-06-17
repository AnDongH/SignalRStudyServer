using MessagePack;

namespace Protocol.Protocols;

[MessagePackObject]
[Union(0, typeof(TestReq))]
[Union(1, typeof(LoginReq))]
[Union(2, typeof(RegisterReq))]
[Union(3, typeof(HubTestReq))]
public abstract class ProtocolReq(ProtocolId protocolId)
{
    [Key(0)]public long Suid { get; set; }
    [Key(1)] public ProtocolId ProtocolId { get; set; } = protocolId;
}