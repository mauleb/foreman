namespace Foreman.LanguageServer.Protocol.Services;

internal interface IStreamingInput {
    protected Stream Current { get; }
    public void ReadExactly(IDebugLogger debugLogger, Span<byte> buffer) {
        try {
            Current.ReadExactly(buffer);
        } catch(Exception ex) {
            debugLogger.Write("STREAM EXCEPTION: " + ex.ToString());
        }
    }
}
