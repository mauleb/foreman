using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Binding;

public class BoundSpan<TData> : BoundNodeBase {
    public BoundSpan(DocumentSpan span, TData data) {
        Data = data;
        Span = span;
    }

    public override BoundNodeKind Kind => BoundNodeKind.Span;
    public TData Data { get; }
    public DocumentSpan Span { get; }
    public override string Serialize(VariableValueBag variables) => Data + "";
}
