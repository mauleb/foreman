using System.Text.Json;
using Foreman.LanguageServer.Protocol.Requests;
using Foreman.LanguageServer.Protocol.Responses;

namespace Foreman.LanguageServer.Protocol.Types;

public abstract class RequestHandlerBase<TRequest> where TRequest : ILspRequest {
    internal abstract Task<ILspResponse> Invoke(Span<byte> contentBytes, JsonSerializerOptions serializerOptions, IDebugLogger debugLogger);
}

