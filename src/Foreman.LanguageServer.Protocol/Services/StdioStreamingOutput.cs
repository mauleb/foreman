using System.Text;
using System.Text.Json;
using Foreman.LanguageServer.Protocol.Notifications;
using Foreman.LanguageServer.Protocol.Responses;

namespace Foreman.LanguageServer.Protocol.Services;

internal class StdioStreamingOutput : IStreamingOutput, IDisposable {
    private static readonly byte[] HEADING_BYTES = Encoding.UTF8.GetBytes("Content-Length: ");
    private const byte SEPARATOR_0 = (byte)'\r';
    private const byte SEPARATOR_1 = (byte)'\n';
    private readonly Stream _stream;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _serializerOptions;

    public StdioStreamingOutput() {
        _stream = Console.OpenStandardOutput();
        _serializerOptions = new() {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void Dispose()
    {
        _stream.Dispose();
    }

    public async Task WriteResponseAsync<TResponse>(TResponse response) where TResponse : ILspResponse
        => await WriteAsync(response);

    public async Task WriteNotificationAsync<TNotification>(TNotification notification) where TNotification : ILspNotification
        => await WriteAsync(notification);

    private Task WriteAsync(object data) {
        byte[] contentBytes = JsonSerializer.SerializeToUtf8Bytes(data, _serializerOptions);
        byte[] contentLength = Encoding.UTF8.GetBytes(contentBytes.Length.ToString());

        lock (_lock) {
            _stream.Write(HEADING_BYTES);
            _stream.Write(contentLength);
            _stream.Write([SEPARATOR_0,SEPARATOR_1,SEPARATOR_0,SEPARATOR_1]);
            _stream.Write(contentBytes);
        }

        return Task.CompletedTask;
    }
}
