using System.Collections.Immutable;

using Foreman.CodeAnalysis.Parsing.XmlDocument;
using Foreman.CodeAnalysis.Semantics;
using Foreman.CodeAnalysis.Text;

namespace Foreman.CodeAnalysis.Binding;

public class BindingContext {
    private readonly MultiLineString _contents;
    private readonly List<Diagnostic> _diagnostics;
    private readonly VariableReferenceBag _variableReferences;
    public ImmutableArray<Diagnostic> Diagnostics => _diagnostics.ToImmutableArray();
    public VariableReferenceBag VariableReferences => _variableReferences.Clone();

    public BindingContext(MultiLineString contents) {
        _contents = contents;
        _diagnostics = [];
        _variableReferences = new();
    }

    public BoundSpan<string> BindString(DocumentSpan span) {
        string rawValue = _contents.GetSubstring(span);
        return new(span, rawValue);
    }

    internal void AddVariableReference(VariableIdentifier pointer, DocumentSpan span) {
        _variableReferences.SetVariableReference(pointer, span);
    }

    internal void ReportNonMatchingElementTags(string expected, DocumentSpan closingTag) {
        string message = string.Format("Closing tag does not align. Expected: {0}", expected);
        Diagnostic diagnostic = new() {
            Message = message,
            Span = closingTag
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportUnexpectedSyntax(SyntaxKind unexpected, DocumentSpan span) {
        string message = string.Format("Encountered unexpected syntax of kind: ", unexpected);
        Diagnostic diagnostic = new() {
            Message = message,
            Span = span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportBindingError(BoundNodeKind expected, DocumentSpan span) {
        string message = string.Format("Failed to bind instance of {0}", expected);
        Diagnostic diagnostic = new() {
            Message = message,
            Span = span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportMalformedSymbol(DocumentSpan span)
    {
        string message = string.Format("Unbindable symbol.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportDuplicateAttribute(DocumentSpan span)
    {
        string message = string.Format("Duplicate attribute");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = span
        };

        _diagnostics.Add(diagnostic);
    }

    internal void ReportUnexpectedSymbol(DocumentSpan span)
    {
        string message = string.Format("Unexpected symbol. Only first party foreman blocks are allowed to have embedded symbols.");
        Diagnostic diagnostic = new() {
            Message = message,
            Span = span
        };

        _diagnostics.Add(diagnostic);
    }

}
