using System.Net;
using MessagePack;
using Microsoft.AspNetCore.SignalR;
using SignalRStudyServer.Authentication;
using SignalRStudyServer.Authorizations;
using SignalRStudyServer.BackgroundServices;
using SignalRStudyServer.Databases;
using SignalRStudyServer.Databases.PgSql;
using SignalRStudyServer.Databases.Redis;
using SignalRStudyServer.Hubs;
using SignalRStudyServer.Hubs.Filters;
using SignalRStudyServer.Middlewares;
using SignalRStudyServer.Services;
using StackExchange.Redis;

namespace SignalRStudyServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 여기선 웹 서비스와 같이 구현하긴 했지만 가능한 SignalR과 웹 서비스를 분리하는게 좋음
        builder.Services.AddSignalR(options =>
        {
            // 필터들은 싱글톤처럼 사용됨
            // 하지만 허브 호출당 생성 삭제되므로, 진짜 싱글톤으로 사용할거면 싱글톤 DI 등록해야함
            //options.AddFilter<GlobalTestFilter>();
            //options.AddFilter(typeof(GlobalTestFilter));
            //options.AddFilter(new GlobalTestFilter());
        }).AddHubOptions<ChatHub>(options =>
        {
            // 필터들은 싱글톤처럼 사용됨
            //options.AddFilter<ChatHubLocalTestFilter>();
            options.AddFilter<LanguageFilter>();
        }).AddMessagePackProtocol(options =>
        {
            options.SerializerOptions = MessagePackSerializerOptions.Standard
                .WithSecurity(MessagePackSecurity.UntrustedData);
        }).AddStackExchangeRedis("localhost:6379", options =>
        {
            // 레디스 백플레인
            // 만약 다른 signalR 앱에서도 해당 redis를 사용한다면, 접두사를 다르게 해줘야함
            options.Configuration.ChannelPrefix = RedisChannel.Literal("SignalRStudy");
            
            // 레디스 연결 실패에 따른 이벤트
            options.ConnectionFactory = async writer =>
            {
                var config = new ConfigurationOptions
                {
                    AbortOnConnectFail = false
                };
                config.EndPoints.Add(IPAddress.Loopback, 0);
                config.SetDefaultPorts();
                var connection = await ConnectionMultiplexer.ConnectAsync(config, writer);
                connection.ConnectionFailed += (_, e) =>
                {
                    Console.WriteLine("Connection to Redis failed.");
                };

                if (!connection.IsConnected)
                {
                    Console.WriteLine("Did not connect to Redis.");
                }

                return connection;
            };
        });
        builder.Services.AddControllers();
        builder.Services.Configure<DBOptions>(builder.Configuration.GetSection(DBOptions.DbConfig));

        builder.Services.AddTransient<PgSqlClient>();
        builder.Services.AddSingleton<RedisClient>();
        builder.Services.AddTransient<DataProcessService>();
        builder.Services.AddTransient<LoginService>();
        builder.Services.AddSingleton<IUserIdProvider, NameBasedUserIdProvider>(); // SignalR에서 UserIdProvider를 사용하기 위한 설정
        
        // 필터 내부에서 전역적으로 사용하는 변수를 재할당하는 것을 막기 위해 싱글톤 등록
        builder.Services.AddSingleton<LanguageFilter>();
        
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "TestAuth";
            options.DefaultChallengeScheme = "TestAuth";
        })
        .AddScheme<TestAuthOptions, TestAuthHandler>("TestAuth", options =>
        {
            
        });
        
        // 권한 정책 추가. 디폴트 정척 + 추가 정책
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
            {
                policy.Requirements.Add(new AdminRequirement());
            });
        });

        // 백그라운드 서비스를 이용한 시그널 R 이벤트 처리도 가능
        builder.Services.AddHostedService<ClockWorker>();
        
        var app = builder.Build();
        
        app.UsePathLoggingMiddleware();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapHub<ChatHub>("/signalR/chatHub", options =>
        {
            options.CloseOnAuthenticationExpiration = true;
        });
        app.MapControllers();

        app.Run();
    }
}