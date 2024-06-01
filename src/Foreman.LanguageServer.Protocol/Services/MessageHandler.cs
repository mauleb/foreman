using System.Text.Json;
using Foreman.LanguageServer.Protocol.Notifications;
using Foreman.LanguageServer.Protocol.Requests;
using Foreman.LanguageServer.Protocol.Responses;
using Foreman.LanguageServer.Protocol.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Foreman.LanguageServer.Protocol.Services;

internal class MessageHandler {
    private readonly IDebugLogger _logger;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IServiceProvider _serviceProvider;

    public MessageHandler(IDebugLogger logger, JsonSerializerOptions serializerOptions, IServiceProvider serviceProvider) {
        _logger = logger;
        _serializerOptions = serializerOptions;
        _serviceProvider = serviceProvider;
    }

    private AnonymousRequest ParseAnonymous(Span<byte> contentBytes) {
        try {
            var deserialized = JsonSerializer.Deserialize<AnonymousRequest>(contentBytes, _serializerOptions);
            if (deserialized == null) {
                _logger.Write("METHOD NOT FOUND: " + contentBytes.AsString());
                throw LanguageServerException.FromCode(ErrorCode.MethodNotFound);
            }
            return deserialized;
        } catch {
            _logger.Write("PANIC! Unable to parse anonymous: " + contentBytes.AsString());
            throw LanguageServerException.FromCode(ErrorCode.MalformedMessage);
        }
    }

    private Task HandleUnknown(string method) {
        _logger.Write("UNKNOWN METHOD: " + method);
        return Task.CompletedTask;
    }

    private Task HandleRequestAsync<TRequest>(AnonymousRequest anon, Span<byte> contentBytes, IStreamingOutput outputStream) where TRequest : ILspRequest {
        var handler = _serviceProvider.GetService<RequestHandlerBase<TRequest>>();
        if (handler == null) {
            return Task.CompletedTask;
        }

        _logger.Write(string.Format("MESSAGE: {0} ({1})", anon.Method, anon.Id));

        Task<ILspResponse> invokeTask = handler.Invoke(contentBytes, _serializerOptions, _logger);
        return Task.Run(async () => {
            ILspResponse response = await invokeTask;
            await outputStream.WriteResponseAsync(response);
        });
    }

    private Task HandleNotificationAsync<TNotification>(AnonymousRequest anon, Span<byte> contentBytes) where TNotification : ILspNotification {
        var handler = _serviceProvider.GetService<NotificationHandlerBase<TNotification>>();
        if (handler == null) {
            return Task.CompletedTask;
        }

        _logger.Write(string.Format("NOTIFICATION: {0}", anon.Method));

        Task invokeTask = handler.Invoke(contentBytes, _serializerOptions, _logger);
        return invokeTask;
    }

    public Task HandleMessageAsync(Span<byte> contentBytes, IStreamingOutput outputStream) {
        AnonymousRequest anon = ParseAnonymous(contentBytes);
        return anon.Method switch {
            "initialize" 
                => HandleRequestAsync<InitializeRequest>(anon, contentBytes, outputStream),
            "initialized" 
                => HandleNotificationAsync<InitializedNotification>(anon, contentBytes),
            "textDocument/didOpen"
                => HandleNotificationAsync<DidOpenTextDocumentNotification>(anon, contentBytes),
            "textDocument/didChange"
                => HandleNotificationAsync<DidChangeTextDocumentNotification>(anon, contentBytes),
            "textDocument/semanticTokens/full"
                => HandleRequestAsync<SemanticTokensFullRequest>(anon, contentBytes, outputStream),
            _ => HandleUnknown(anon.Method)
        };
    }
}
