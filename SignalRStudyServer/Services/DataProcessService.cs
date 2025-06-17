using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Protocol;
using Protocol.Protocols;

namespace SignalRStudyServer.Services;

public class DataProcessService
{
    public async Task SerializeAndSendProtocolAsync(HttpResponse httpRes, ProtocolRes protocolRes)
    {
        var bytes = MessagePackSerializer.Serialize(protocolRes);
        var writer = httpRes.BodyWriter;
        await writer.WriteAsync(bytes);
        await writer.FlushAsync();
    }
    
    public async Task<T> DeSerializeProtocolAsync<T>(HttpRequest httpReq, bool pipeRead = false) where T : ProtocolReq
    {
        if (pipeRead)
        {
            var reader = httpReq.BodyReader;
            var completeMessage = new List<byte>();
        
            while (true)
            {
                var readResult = await reader.ReadAsync();
                var buffer = readResult.Buffer;
        
                completeMessage.AddRange(buffer.ToArray());
                
                reader.AdvanceTo(buffer.End);
                
                if (readResult.IsCompleted || readResult.IsCanceled)
                    break;
            }
        
            return MessagePackSerializer.Deserialize<ProtocolReq>(completeMessage.ToArray())  as T;
        }
        
        httpReq.EnableBuffering();
        
        using var memoryStream = new MemoryStream();
        await httpReq.Body.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        httpReq.Body.Position = 0;
        
        return MessagePackSerializer.Deserialize<ProtocolReq>(memoryStream.ToArray())  as T;
    }
}