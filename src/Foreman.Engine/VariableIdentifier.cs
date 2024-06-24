namespace Foreman.Engine;

public record VariableIdentifier {
    public required string Namespace { get; init; }
    public required string Key { get; init; }

    public static VariableIdentifier Parse(string value) {
        string[] parts = value.Split("/");
        string @namespace = parts.SkipLast(1)
            .Aggregate((a,b) => a + "/" + b);
        return new() {
            Namespace = @namespace,
            Key = parts[parts.Length - 1]
        };
    }

    public override string ToString() {
        return string.Format("{0}/{1}",Namespace,Key);
    }
}