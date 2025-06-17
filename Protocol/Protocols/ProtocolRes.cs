using MessagePack;

namespace Protocol.Protocols;

[MessagePackObject]
[Union(0, typeof(TestRes))]
[Union(1, typeof(LoginRes))]
[Union(2, typeof(RegisterRes))]
[Union(3, typeof(HubTestRes))]
public abstract class ProtocolRes
{ 
    [Key(0)] public ProtocolResult ProtocolResult { get; set; }
}