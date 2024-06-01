using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Parsing.XmlDocument;


public partial class XmlDocumentParser {
    internal readonly static SyntaxTokenKind[] AnyWhitespace = [
        SyntaxTokenKind.Whitespace,
        SyntaxTokenKind.NewLine
    ];
}