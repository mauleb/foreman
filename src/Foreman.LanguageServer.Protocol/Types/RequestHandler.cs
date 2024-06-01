using System.Text.Json;
using Foreman.LanguageServer.Protocol.Requests;
using Foreman.LanguageServer.Protocol.Responses;

namespace Foreman.LanguageServer.Protocol.Types;

public abstract class RequestHandler<TRequest, TResult> : RequestHandlerBase<TRequest> 
where TRequest : ILspRequest 
where TResult : ILspResult {
    internal override Task<ILspResponse> Invoke(Span<byte> contentBytes, JsonSerializerOptions serializerOptions, IDebugLogger debugLogger) {
       TRequest? request = JsonSerializer.Deserialize<TRequest>(contentBytes, serializerOptions);
       if (request == null) {
        debugLogger.Write("Unable to parse message (): " + contentBytes.AsString());
        throw LanguageServerException.FromCode(ErrorCode.UnableToParseMessage);
       }

       return WrapHandle(request);
    }

    private async Task<ILspResponse> WrapHandle(TRequest request) {
        TResult result = await Handle(request);
        return result.AsResponse(request.Id);
    }

    public abstract Task<TResult> Handle(TRequest request);
}
