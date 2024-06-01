using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;

public enum SyntaxKind {
    PartialSymbol,
    CompoundSymbol,
    Captured,
    Invalid,
    NamespacedSymbol,
    InterpolatedSymbol,
    Attribute,
    Sequence,
    ClosingTag,
    Comment,
    Trivia,
    Element,
    Document,
    Single,
    Content
}

public abstract record SyntaxBase {
    public abstract SyntaxKind Kind { get; }
    public abstract IEnumerable<SyntaxBase?> Children { get; }
    public virtual DocumentSpan Span => Children.GetDocumentSpan();
}