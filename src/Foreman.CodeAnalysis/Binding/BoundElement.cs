using System.Collections.Immutable;
using System.Text;

namespace Foreman.CodeAnalysis.Binding;

public class BoundElement : BoundNodeBase {
    public override BoundNodeKind Kind => BoundNodeKind.Element;
    public required BoundSpan<string> OpenTag { get; init; }
    public required ImmutableArray<BoundAttribute> Attributes { get; init; }
    public BoundValueSequence? ValueContent { get; init; }
    public ImmutableArray<BoundElement>? NestedElements { get; init; }
    public override string Serialize(VariableValueBag variables) {
        StringBuilder builder = new();
        builder.Append('<');
        builder.Append(OpenTag.Data);

        foreach (var nextAttr in Attributes) {
            builder.Append(' ');
            builder.Append(nextAttr.Serialize(variables));
        }

        if (ValueContent != null) {
            builder.Append('>');
            builder.Append(ValueContent.Serialize(variables));
            builder.Append("</");
            builder.Append(OpenTag.Data);
            builder.Append('>');
        } else if (NestedElements != null) {
            builder.Append('>');
            foreach (var child in NestedElements) {
                builder.Append(child.Serialize(variables));
            }
            builder.Append("</");
            builder.Append(OpenTag.Data);
            builder.Append('>');
        } else {
            builder.Append("/>");
        }

        return builder.ToString();
    }
}
