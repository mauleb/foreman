using System.Text.Json;
using Foreman.LanguageServer.Protocol.Notifications;

namespace Foreman.LanguageServer.Protocol.Types;

public abstract class NotificationHandlerBase<TNotification> where TNotification : ILspNotification {
    internal Task Invoke(Span<byte> contentBytes, JsonSerializerOptions serializerOptions, IDebugLogger debugLogger) {
        TNotification? notification = JsonSerializer.Deserialize<TNotification>(contentBytes, serializerOptions);
        if (notification == null) {
            debugLogger.Write("Unable to parse message (): " + contentBytes.AsString());
            throw LanguageServerException.FromCode(ErrorCode.UnableToParseMessage);
        }

        return Handle(notification);
    }

    public abstract Task Handle(TNotification notification);
}