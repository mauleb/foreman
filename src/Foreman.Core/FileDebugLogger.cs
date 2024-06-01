using System.Text;

namespace Foreman.Core;

// TODO: should this be here?

public class FileDebugLogger : IDebugLogger, IDisposable {
    private readonly Stream _outputStream;

    public FileDebugLogger() {
        long ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (!Directory.Exists("/Users/maule/workspace/lsp-thesequel/out")) {
            Directory.CreateDirectory("/Users/maule/workspace/lsp-thesequel/out");
        }

        string outPath = string.Format("/Users/maule/workspace/lsp-thesequel/out/{0}", ms);
        _outputStream = File.Create(outPath);
    }

    public void Dispose()
    {
        _outputStream.Dispose();
    }

    public void Write(string message)
    {
        string formattedMessage = string.Format("[{0}] {1}\n",
            DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
            message
        );
        _outputStream.Write(Encoding.UTF8.GetBytes(formattedMessage));
        _outputStream.Flush();
    }
}
