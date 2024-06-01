using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Semantics;
using Foreman.CodeAnalysis.Text;
using Foreman.LanguageServer.Protocol.Requests;
using Foreman.LanguageServer.Protocol.Responses;
using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Handlers.Requests;

public class SemanticTokens : RequestHandler<SemanticTokensFullRequest, SemanticTokensResult>
{
    private readonly IDebugLogger _logger;
    private readonly DocumentStore _documentStore;

    public SemanticTokens(IDebugLogger logger, DocumentStore documentStore)
    {
        _logger = logger;
        _documentStore = documentStore;

    }

    private static readonly SemanticTokensResult None = new() {
        Data = []
    };

    public override Task<SemanticTokensResult> Handle(SemanticTokensFullRequest request) {
        if (request.Params == null) {
            return Task.FromResult(None);
        }

        MultiLineString? source = _documentStore.GetSource(request.Params.TextDocument.Uri);
        DocumentSyntax? document = _documentStore.GetDocument(request.Params.TextDocument.Uri);
        if (document == null || source == null) {
            _logger.Write("Unable to parse");
            return Task.FromResult(None);
        }

        _logger.Write("FOUND DOCUMENT");

        IEnumerable<uint> data = XmlDocumentSemanticsBuilder
            .GetSemanticTokens(source, document)
            .Select(token => token.AsDetails())
            .WithRelativeOffsets()
            .Serialize();
        
        SemanticTokensResult result = new() {
            Data = data.ToArray()
        };

        _logger.Write("SEMANTIC TOKEN COUNT = " + result.Data.Length / 5);

        return Task.FromResult(result);
    }
}