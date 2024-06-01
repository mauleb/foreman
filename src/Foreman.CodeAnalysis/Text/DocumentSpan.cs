using Foreman.CodeAnalysis.Parsing.XmlDocument;

namespace Foreman.CodeAnalysis.Text;

public record DocumentSpan {
    public DocumentSpan(string documentId, int startLine, int startPosition, int endLine, int endPosition) {
        DocumentId = documentId;
        StartLine = startLine;
        StartPosition = startPosition;
        EndLine = endLine;
        EndPosition = endPosition;

        if (startLine > EndLine) {
            throw new ArgumentOutOfRangeException(nameof(endLine),"startLine must be less than or equal to endLine");
        }

        if (startLine == endLine && startPosition > endPosition) {
            throw new ArgumentOutOfRangeException(nameof(endPosition), "endPosition must be greater than or equal to startPosition or endLine must be greater startLine");
        }
    }

    public string DocumentId { get; }
    public int StartLine { get; }
    public int StartPosition { get; }
    public int EndLine { get; }
    public int EndPosition { get; }

    public static DocumentSpan None(string? documentId = null) => new(
        documentId: documentId ?? Guid.NewGuid().ToString(),
        startLine: 0,
        startPosition: 0,
        endLine: 0,
        endPosition: 0
    );

    public static DocumentSpan operator +(DocumentSpan a, DocumentSpan b) {
        if (a.DocumentId != b.DocumentId) {
            throw new InvalidDataException("Unable to merge document spans across distinct documents");
        }

        Func<int> GetStartPosition = () => {
            if (a.StartLine < b.StartLine) return a.StartPosition;
            if (a.StartLine > b.StartLine) return b.StartPosition;
            return Math.Min(a.StartPosition,b.StartPosition);
        };

        Func<int> GetEndposition = () => {
            if (a.EndLine < b.EndLine) return a.EndPosition;
            if (a.EndLine > b.EndLine) return b.EndPosition;
            return Math.Max(a.EndPosition,b.EndPosition);
        };

        return new(
            documentId: a.DocumentId,
            startLine: Math.Min(a.StartLine, b.StartLine),
            startPosition: GetStartPosition(),
            endLine: Math.Max(a.EndLine, b.EndLine),
            endPosition: GetEndposition()
        );
    }
}

public static class DocumentSpanExtensions {
    public static DocumentSpan AsDocumentSpan(this StringSpan span, string documentId) => new(
        documentId: documentId,
        startLine: span.Line,
        startPosition: span.Start,
        endLine: span.Line,
        endPosition: span.End
    );

    private static DocumentSpan GetDocumentSpan(this IEnumerable<DocumentSpan> spans) {
        DocumentSpan[] spanList = spans.ToArray();
        if (spanList.Length == 0) {
            return DocumentSpan.None();
        }

        return spanList.Aggregate((a,b) => a + b);
    }

    public static DocumentSpan GetDocumentSpan(this IEnumerable<SyntaxBase?> nodes) {
        SyntaxBase[] nodesList = nodes
            .Where(n => n != null)
            .Cast<SyntaxBase>()
            .ToArray();
        if (nodesList.Length == 0) {
            return DocumentSpan.None();
        }

        string documentId = nodesList[0].Span.DocumentId;
        return nodesList.Select(node => node.Span).GetDocumentSpan();
    }

    public static DocumentSpan GetDocumentSpan(this IEnumerable<SyntaxToken> tokens) {
        SyntaxToken[] tokenList = tokens.ToArray();
        if (tokenList.Length == 0) {
            return DocumentSpan.None("");
        }

        string documentId = tokenList[0].DocumentId;
        return tokenList
            .AsEnumerable()
            .Select(token => token.Span.AsDocumentSpan(documentId))
            .GetDocumentSpan();
    }
}