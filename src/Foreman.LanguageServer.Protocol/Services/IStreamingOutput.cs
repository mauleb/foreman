using Foreman.LanguageServer.Protocol.Notifications;
using Foreman.LanguageServer.Protocol.Responses;

namespace Foreman.LanguageServer.Protocol.Services;

internal interface IStreamingOutput {
    public Task WriteResponseAsync<TResponse>(TResponse response) where TResponse : ILspResponse;
    public Task WriteNotificationAsync<TNotification>(TNotification notification) where TNotification : ILspNotification;
}
