using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Protocol.Protocols;
using SignalRStudyServer.Databases.Redis;
using SignalRStudyServer.Services;

namespace SignalRStudyServer.Authentication;

public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>, IAuthenticationSignOutHandler
{
    private readonly DataProcessService _dataProcessService;
    private readonly RedisClient _redisClient;

    public TestAuthHandler(
        IOptionsMonitor<TestAuthOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder,
        DataProcessService dataProcessService,
        RedisClient redisClient) : base(options, logger, encoder)
    {
        _dataProcessService = dataProcessService;
        _redisClient = redisClient;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        long suid = 0;
        string authToken = null;

        if (Request.Headers.TryGetValue("suid", out var suidValues))
        {
            long.TryParse(suidValues.FirstOrDefault(), out suid);
        }

        authToken = Request.Headers.Authorization.FirstOrDefault();
        
        if (string.IsNullOrEmpty(authToken) || suid == 0)
        {
            return AuthenticateResult.NoResult();
        }
        
        if (authToken.StartsWith("Bearer "))
        {
            authToken = authToken.Substring("Bearer ".Length);
        }
        
        var result = await _redisClient.GetUserAsync(suid);
        if (!result.flag) return AuthenticateResult.NoResult();
        
        if (result.data.AuthToken != authToken) 
        {
            return AuthenticateResult.Fail("Invalid token");
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, suid.ToString()),
            new Claim("AuthToken", authToken),
            new Claim("IsAdmin", result.data.IsAdmin.ToString()),
        };
        
        TimeSpan t = result.expTime.HasValue ? result.expTime.Value : TimeSpan.Zero;
        
        // 이게 있어야 Claim이 유효기간이 만료된걸 확인이 가능함
        var properties = new AuthenticationProperties
        {
            ExpiresUtc = DateTimeOffset.UtcNow.Add(t),
            IssuedUtc = result.data.IssuedTime
        };
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, properties, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }

    public Task SignOutAsync(AuthenticationProperties properties)
    {
        return Task.CompletedTask;
    }
}