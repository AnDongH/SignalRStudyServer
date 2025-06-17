using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Protocol.Protocols;
using SignalRStudyServer.Services;

namespace SignalRStudyServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly LoginService _loginService;
        private readonly ILogger<LoginController> _logger;
        private readonly DataProcessService _dataProcessService;
        
        public LoginController(LoginService loginService, ILogger<LoginController> logger, DataProcessService dataProcessService)
        {
            _loginService = loginService;
            _logger = logger;
            _dataProcessService = dataProcessService;
        }
        
        [HttpPost("login")]
        public async Task LoginAsync()
        {
            try
            {
                var req = await _dataProcessService.DeSerializeProtocolAsync<LoginReq>(Request);
                if (req == null) return;
                var loginInfo = await _loginService.LoginAsync(req);

                if (string.IsNullOrEmpty(loginInfo.authToken) || loginInfo.suid == 0)
                {
                    throw new Exception("_loginService.LoginAsync failed");
                }
                
                var res = new LoginRes
                {
                    AuthToken = loginInfo.authToken,
                    Suid = loginInfo.suid,
                    ProtocolResult = ProtocolResult.Success
                };

                await _dataProcessService.SerializeAndSendProtocolAsync(Response, res);
            }
            catch (Exception ex)
            {
                var res = new LoginRes
                {
                    ProtocolResult = ProtocolResult.Error,
                };
                _logger.LogError(ex, "Login error");
                await _dataProcessService.SerializeAndSendProtocolAsync(Response, res);
            }
        }
        
        [HttpPost("register")]
        public async Task RegisterAsync()
        {
            try
            {
                var req = await _dataProcessService.DeSerializeProtocolAsync<RegisterReq>(Request);
                if (req == null) return;
                var result = await _loginService.RegisterAsync(req.Id, req.Password);
                if (!result) return;
                var res = new RegisterRes
                {
                    ProtocolResult = ProtocolResult.Success
                };
                await _dataProcessService.SerializeAndSendProtocolAsync(Response, res);
            }
            catch (Exception ex)
            {
                var res = new RegisterRes()
                {
                    ProtocolResult = ProtocolResult.Error,
                };
                 _logger.LogError(ex, "register error");
                await _dataProcessService.SerializeAndSendProtocolAsync(Response, res);
            }
        }
    }
}
