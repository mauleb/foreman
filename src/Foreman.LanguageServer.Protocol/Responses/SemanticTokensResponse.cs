namespace Foreman.LanguageServer.Protocol.Responses;

public record SemanticTokensResponse : BaseResponse<SemanticTokensResult> {}

public record SemanticTokensResult : ILspResult {
    public required uint[] Data { get; set; }

    ILspResponse ILspResult.AsResponse(long id)
        => new SemanticTokensResponse() {
            Id = id,
            Result = this
        };
}