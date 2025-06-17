using Protocol.Protocols;

namespace Protocol.Router;

public class ProtocolRouter
{
    public static Dictionary<ProtocolId, string> RouterMap = new Dictionary<ProtocolId, string>
    {
        { ProtocolId.Test , ""},
        { ProtocolId.Login , "login/login"},
        { ProtocolId.Register , "login/register"},
        { ProtocolId.HubTest , "hubcontexttest/hub-test"},
    };
}