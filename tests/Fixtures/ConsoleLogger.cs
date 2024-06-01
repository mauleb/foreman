using Foreman.Core;

namespace Fixtures;

public class ConsoleLogger : IDebugLogger
{
    public void Write(string message)
    {
        Console.WriteLine(message);
    }
}
