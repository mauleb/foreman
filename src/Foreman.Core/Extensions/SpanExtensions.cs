using System.Text;

namespace Foreman.Core;

public static class SpanExtensions {
    public static string AsString(this Span<byte> bytes) {
        return Encoding.UTF8.GetString(bytes);
    }
}