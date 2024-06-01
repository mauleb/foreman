using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Protocol.Requests;

public record SemanticTokensFullRequest : BaseRequest<SemanticTokensParams> {}

public record SemanticTokensParams {
    public required TextDocumentIdentifier TextDocument { get; init; }
}