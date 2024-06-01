using System.Text.Json;
using Foreman.CodeAnalysis.Semantics;
using Foreman.LanguageServer.Protocol.Requests;
using Foreman.LanguageServer.Protocol.Responses;
using Foreman.LanguageServer.Protocol.Types;

namespace Foreman.LanguageServer.Handlers.Requests;

public class Initialize : RequestHandler<InitializeRequest, InitializeResult>
{
    private readonly IDebugLogger _logger;
    private readonly JsonSerializerOptions _serializerOptions;

    public Initialize(IDebugLogger logger, JsonSerializerOptions serializerOptions)
    {
        _logger = logger;
        _serializerOptions = serializerOptions;
    }

    public override Task<InitializeResult> Handle(InitializeRequest request) { 
        _logger.Write(JsonSerializer.Serialize(request.Params?.Capabilities));

        InitializeResult result = new() {
            Capabilities = new() {
                TextDocumentSync = new() {
                    OpenClose = true,
                    // TODO: incremental support
                    // Change = TextDocumentSyncKind.Incremental
                    Change = TextDocumentSyncKind.Full
                },
                SemanticTokensProvider = new() {
                    Legend = new() {
                        TokenTypes = SemanticTokenExtensions
                            .TokenKinds()
                            .Select(kind => kind.GetName())
                            .ToArray(),
                        TokenModifiers = []
                    },
                    Range = false,
                    Full = new() {
                        Delta = false
                    },
                }
            },
            ServerInfo = new() {
                Name = "Foreman.LanguageServer",
                Version = "0.0.1"
            }
        };

        return Task.FromResult(result);
    }
}