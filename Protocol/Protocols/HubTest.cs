using MessagePack;

namespace Protocol.Protocols;

[MessagePackObject]
public class HubTestReq() : ProtocolReq(ProtocolId.HubTest)
{
    
}

[MessagePackObject]
public class HubTestRes : ProtocolRes
{
}