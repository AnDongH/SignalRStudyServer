using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Protocol.Protocols;
using SignalRStudyServer.Hubs;
using SignalRStudyServer.Services;

namespace SignalRStudyServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class HubContextTestController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly DataProcessService _dataProcessService;
        private readonly ILogger<HubContextTestController> _logger;
        
        public HubContextTestController(IHubContext<ChatHub> hubContext, DataProcessService dataProcessService, ILogger<HubContextTestController> logger)
        {
            _hubContext = hubContext;
            _dataProcessService = dataProcessService;
            _logger = logger;
        }

        [HttpPost("hub-test")]
        public async Task HubTest()
        {
            try
            {
                var req = await _dataProcessService.DeSerializeProtocolAsync<HubTestReq>(Request);
                if (req == null) return;

                // 컨트롤러에서 허브를 호출하는 방법. 해당 허브에 연결되어있는 클라이언트들에 대하여 메세징 가능.
                // 이곳 말고도, 미들웨어에서도 접근이 가능
                await _hubContext.Clients.All.SendAsync("Notify", $"{GetType().Name}에서 호출"); 
                
                var res = new HubTestRes()
                {
                    ProtocolResult = ProtocolResult.Success
                };

                await _dataProcessService.SerializeAndSendProtocolAsync(Response, res);
            }
            catch (Exception ex)
            {
                var res = new HubTestRes
                {
                    ProtocolResult = ProtocolResult.Error,
                };
                _logger.LogError(ex, "hub controller test error");
                await _dataProcessService.SerializeAndSendProtocolAsync(Response, res);
            }
        }
    }
}
