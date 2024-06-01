using Foreman.CodeAnalysis.Text;
using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.LanguageServer.Protocol.Notifications;
using System.Collections.Immutable;
using Foreman.CodeAnalysis.Semantics;
using Foreman.LanguageServer.Protocol.Services;

namespace Foreman.LanguageServer;

public class DocumentStore {
    private readonly Dictionary<string, DocumentSyntax> _syntax;
    private readonly Dictionary<string, ImmutableArray<Diagnostic>> _diag;
    private readonly Dictionary<string, MultiLineString> _source;
    private readonly IDebugLogger _logger;
    private readonly IClientNotifier _clientNotifier;


    public DocumentStore(IDebugLogger logger, IClientNotifier clientNotifier) {
        _syntax = [];
        _diag = [];
        _source = [];
        _logger = logger;
        _clientNotifier = clientNotifier;
    }

    public async Task PublishDiagnosticsAsync(string uri) {
        if (!_diag.ContainsKey(uri)) {
            return;
        }

        ImmutableArray<Diagnostic> diagnostics = _diag[uri];

        PublishDiagnosticsParams @params = new() {
            Uri = uri,
            Diagnostics = diagnostics
                .Select(diag => new Lsp.Diagnostic() {
                    Source = "Foreman",
                    Severity = Lsp.DiagnosticSeverity.Error,
                    Message = diag.Message,
                    Range = diag.Span.AsRange()
                })
                .ToArray()
        };

        await _clientNotifier.PublishAsync(@params.Wrap());
    }

    public void StoreDocument(string uri, string contents) {
        _logger.Write(string.Format("STORE {0}", uri));

        MultiLineString mls = new(contents);
        _source[uri] = mls;
        
        XmlDocumentParsingContext context = new(mls);
        DocumentSyntax? document = XmlDocumentParser.ParseDocument(context);
        if (document == null) {
            _logger.Write("FAILED TO PARSE: " + uri);
            return;
        }

        _syntax[uri] = document;
        _diag[uri] = context.Diagnostics;
    }

    public DocumentSyntax? GetDocument(string uri) {
        if (_syntax.ContainsKey(uri)) {
            return _syntax[uri];
        }

        return null;
    }

    public MultiLineString? GetSource(string uri) {
        if (_source.ContainsKey(uri)) {
            return _source[uri];
        }

        return null;
    }
}