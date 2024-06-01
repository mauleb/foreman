using System.Collections.Immutable;
using System.Text;

namespace Foreman.CodeAnalysis.Binding;

public class BoundValueSequence : BoundNodeBase
{
    public override BoundNodeKind Kind => BoundNodeKind.ValueSequence;

    public required ImmutableArray<BoundNodeBase> Nodes { get; init; }

    public override string Serialize(VariableValueBag variables) => Nodes
        .Select(node => node.Serialize(variables))
        .Aggregate(new StringBuilder(), (builder, next) => builder.Append(next))
        .ToString();
}
