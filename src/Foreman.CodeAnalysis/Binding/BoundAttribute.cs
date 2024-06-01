using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Binding;

public class BoundAttribute : BoundNodeBase {
    public override BoundNodeKind Kind => BoundNodeKind.Attribute;
    public required DocumentSpan Span { get; init; }
    public required BoundSpan<string> Key { get; init; }
    public required BoundValueSequence Value { get; init; }
    public override string Serialize(VariableValueBag variables) 
        => string.Format("{0}=\"{1}\"", Key.Data, Value.Serialize(variables));

}
