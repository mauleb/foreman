using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Binding;

public enum BoundNodeKind {
    Symbol,
    Span,
    ValueSequence,
    Attribute,
    Element,
    Document,
    ForemanJob

}

public abstract class BoundNodeBase {
    public abstract BoundNodeKind Kind { get; }
    public abstract string Serialize(VariableValueBag variables);
}