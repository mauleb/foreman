using System.Text.Json;
using Foreman.LanguageServer.Protocol.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Foreman.LanguageServer.Protocol.Services;

public interface IClientNotifier {
    public Task PublishAsync(ILspNotification notification);
}

public class ClientNotifier : IClientNotifier
{
    private readonly IStreamingOutput _streamingOutput;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IDebugLogger _logger;


    public ClientNotifier(IServiceProvider serviceProvider, JsonSerializerOptions serializerOptions, IDebugLogger logger) {
        _serializerOptions = serializerOptions;
        _logger = logger;

        _streamingOutput = serviceProvider.GetRequiredService<IStreamingOutput>();
    }

    public async Task PublishAsync(ILspNotification notification)
    {
        try {
            await _streamingOutput.WriteNotificationAsync(notification);
        } catch (Exception ex) {
            _logger.Write("FAILED TO PUBLISH: " + ex.Message);
        }
    }
}