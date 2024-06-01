using System.Collections.Immutable;

using Foreman.Core;

namespace Fixtures;

public class InMemoryLogger : IDebugLogger {
    private readonly List<string> _messages = [];
    public ImmutableArray<string> Messages => _messages.ToImmutableArray();

    public void Write(string message)
    {
        _messages.Add(message);
    }
}
