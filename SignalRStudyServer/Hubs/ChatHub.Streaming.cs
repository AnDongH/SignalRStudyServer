using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalRStudyServer.Hubs;

/*
 * SignalR은 기본적으로 스트리밍을 지원함
 * 스트리밍을 쓰는 경우는,
 *
 * 대규모 데이터 로딩과 같은 곳에 쓰인다.
 * 연속성과, 대규모 데이터 처리가 중요하다.
 *
 * AI 봇 경로 재생, 리플레이 모드, 튜토리얼 재생 등등에도 쓰일 수 있다.
 * 중요한 점은 실시간 동기화에는 비효율적인 점이 많아 쓰이지 않는다.
 *
 * 항상 클라이언트가 중간에 구독을 취소하는 cancellationToken을 사용해야 한다.
 */
[Authorize]
public partial class ChatHub
{
    
    #region Server to Client

    // IAsyncEnumerable 을 반환하는 스트리밍 허브 메서드
    // 비동기 스트림을 지원하여 지연평가로 메모리 효율적임
    // 단순한 데이터에 적합함
    public async IAsyncEnumerable<int> CounterIAsyncEnumerable(int count, int delay, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < count; i++)
        {
            
            // 클라이언트의 구독 취소를 체크해야함. 그래야 서버가 불필요한 작업을 하지 않음
            cancellationToken.ThrowIfCancellationRequested();

            yield return i;

            // cancellationToken을 다른 API에도 전달
            await Task.Delay(delay, cancellationToken);
        }
    }
    
    // ChannelReader를 반환하는 스트리밍 허브 메서드
    // 복잡한 버퍼링 전략이 필요할 때,
    // 더 높은 처리량과, 낮은 지연시간 가짐
    public ChannelReader<int> CounterChannelReader(int count, int delay, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<int>();

        // Reader를 클라이언트에게 최대한 빨리 리턴해줘야함.
        // 그렇지 않으면 클라이언트가 데이터가 전부 처리될때까지 대기하게 됨
        _ = WriteItemsAsync(channel.Writer, count, delay, cancellationToken);

        return channel.Reader;
    }

    private async Task WriteItemsAsync(ChannelWriter<int> writer, int count, int delay, CancellationToken cancellationToken)
    {
        Exception localException = null;
        try
        {
            for (var i = 0; i < count; i++)
            {
                await writer.WriteAsync(i, cancellationToken);
                
                await Task.Delay(delay, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            localException = ex;
        }
        finally
        {
            writer.Complete(localException);
        }
    }

    #endregion
    
    #region Client to Server
    // 대규모 데이터를 업로드 할 때 쓰이는 듯
    
    // 클라이언트에서 보낸 스트리밍 데이터를 처리하는 메서드
    // IAsyncEnumerable 버전
    // 클라이언트로부터 받은 IAsyncEnumerable를 처리
    public async Task UploadStreamIAsyncEnumerable(IAsyncEnumerable<int> stream)
    {
        await foreach (var item in stream)
        {
            Console.WriteLine(item);
        }
    }
    
    // 클라이언트에서 보낸 스트리밍 데이터를 처리하는 메서드
    // ChannelReader 버전
    // 클라이언트로부터 받은 ChannelReader를 처리
    public async Task UploadStreamChannelReader(ChannelReader<int> stream)
    {
        while (await stream.WaitToReadAsync())
        {
            while (stream.TryRead(out var item))
            {
                Console.WriteLine(item);
            }
        }
    }
    
    #endregion
}