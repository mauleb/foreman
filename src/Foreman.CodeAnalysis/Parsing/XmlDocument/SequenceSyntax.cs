using System.Collections.Immutable;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public record SequenceSyntax : SyntaxBase {
    public override SyntaxKind Kind => SyntaxKind.Sequence;
    public override IEnumerable<SyntaxBase?> Children => Nodes;
    public required ImmutableArray<SyntaxBase> Nodes { get; init; }
}
