global using Foreman.Core;
global using Lsp = Foreman.LanguageServer.Protocol.Types;

using Foreman.LanguageServer;
using Foreman.LanguageServer.Protocol;
using Foreman.LanguageServer.Protocol.Notifications;
using Foreman.LanguageServer.Protocol.Requests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Handlers = Foreman.LanguageServer.Handlers;


HostApplicationBuilder builder = LSP.StdioBuilder(args);

builder.Services.AddRequest<InitializeRequest, Handlers.Requests.Initialize>();
builder.Services.AddNotification<DidOpenTextDocumentNotification, Handlers.Notifications.DidOpen>();
builder.Services.AddNotification<DidChangeTextDocumentNotification, Handlers.Notifications.DidChange>();
builder.Services.AddRequest<SemanticTokensFullRequest, Handlers.Requests.SemanticTokens>();

builder.Services.AddSingleton<DocumentStore>();

builder.Build().Run();