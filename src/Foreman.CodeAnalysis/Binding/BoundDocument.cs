using System.Collections.Immutable;

namespace Foreman.CodeAnalysis.Binding;

public class BoundDocument : BoundNodeBase {
    public override BoundNodeKind Kind => BoundNodeKind.Document;
    public override string Serialize(VariableValueBag variables) => Root != null
        ? Root.Serialize(variables)
        : string.Empty;
    public required VariableReferenceBag VariableReferences { get; init; }
    public BoundElement? Root { get; init; }
}

public class BoundForemanJob : BoundDocument {
    public override BoundNodeKind Kind => BoundNodeKind.ForemanJob;
}