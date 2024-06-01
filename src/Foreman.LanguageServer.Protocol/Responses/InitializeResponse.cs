using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Protocol.Responses;

public record InitializeResponse : BaseResponse<InitializeResult> {}

public record InitializeResult : ILspResult {
    public required ServerCapabilities Capabilities { get; init; }
    public ServerInfo? ServerInfo { get; init; }

    ILspResponse ILspResult.AsResponse(long id)
        => new InitializeResponse() {
            Id = id,
            Result = this
        };
}