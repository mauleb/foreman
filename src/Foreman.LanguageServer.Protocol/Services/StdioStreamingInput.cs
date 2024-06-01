namespace Foreman.LanguageServer.Protocol.Services;

internal class StdioStreamingInput : IStreamingInput, IDisposable
{
    private readonly Stream _stream;
    Stream IStreamingInput.Current => _stream;

    public StdioStreamingInput() {
        _stream = Console.OpenStandardInput();
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}
