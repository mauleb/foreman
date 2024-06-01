using System.Text.Json;

namespace Foreman.Core;

// TODO: should this be here?

public interface IDebugLogger {
    public void Write(string message);
    public void Write(dynamic obj)
        => Write(JsonSerializer.Serialize(obj));
}
