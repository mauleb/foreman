using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Binding;

public class BoundSymbol : BoundNodeBase {
    public override BoundNodeKind Kind => BoundNodeKind.Symbol;
    public required BoundSpan<string> Namespace { get; init; }
    public required BoundSpan<string> Key { get; init; }
    public VariableIdentifier Pointer => new(Namespace.Data, Key.Data);
    public DocumentSpan Span => Namespace.Span + Key.Span;
    public override string Serialize(VariableValueBag variables)
        => variables.GetVariable(Namespace.Data,Key.Data);
}