using System.Text;

namespace Foreman.CodeAnalysis.Text;

public class MultiLineString {
    public string DocumentId { get; private set; }
    private readonly List<StringBuilder> _lines;
    private string? _fullContents = null;
    private string FullContents {
        get {
            if (_fullContents == null) {
                _fullContents = _lines
                    .Aggregate(new StringBuilder(), (a,b) => a.Append(b))
                    .ToString();
            }
            return _fullContents!;
        }
    }

    public int LineCount => _lines.Count;

    public MultiLineString(string contents) {
        DocumentId = Guid.NewGuid().ToString();
        _lines = [];

        int pos = 0;

        for (int len = 1; pos + len <= contents.Length; len += 1) {
            int idx = pos + len - 1;
            if (contents[idx] == '\n' || contents[idx] == '\r') {
                StringBuilder line = new(contents.Substring(pos, len));
                if (line[len - 1] == '\r') {
                    // literally replace \r with \n
                    line[len - 1] = '\n';
                    
                    // functionally replace \r\n with \n
                    if (idx + 1 < contents.Length && contents[idx + 1] == '\n') {
                        pos += 1;
                    }
                }

                _lines.Add(line);
                pos += len;
                len = 0; // for loop will bump this
            }
        }

        if (pos < contents.Length) {
            StringBuilder line = new(contents.Substring(pos));
            _lines.Add(line);
        }
    }

    public string GetAllLines() => FullContents;

    public string GetLine(int line) {
        if (line < 0 || line >= LineCount) {
            throw new ArgumentOutOfRangeException(nameof(line));
        }

        return _lines[line].ToString();
    }

    public int GetAbsolutePosition(int line, int position) {
        int lineStart = _lines
            .Take(line)
            .Select(line => line.Length)
            .Sum();
        return lineStart + position;
    }

    public string GetSubstring(StringSpan span) {
        return FullContents.Substring(span.AbsolutePosition, span.Length);
    }

    public string GetSubstring(DocumentSpan span) {
        if (span.DocumentId != DocumentId) { return ""; }
        
        int from = GetAbsolutePosition(span.StartLine, span.StartPosition);
        int to = GetAbsolutePosition(span.EndLine, span.EndPosition);
        return FullContents.Substring(from, to - from + 1);
    }

    public int GetLineLength(int line) {
        if (line < 0 || line >= LineCount) {
            throw new ArgumentOutOfRangeException(nameof(line));
        }

        return _lines[line].Length;
    }

    public char CharAt(int absolutePosition) {
        return FullContents[absolutePosition];
    }

    public char CharAt(int line, int position) {
        return _lines[line][position];
    }

    public void RemoveLine(int line) {
        if (line < 0 || line >= LineCount) {
            throw new ArgumentOutOfRangeException(nameof(line));
        }

        _lines.RemoveAt(line);
        _fullContents = null;
    }

    public void RemoveSubstring(DocumentSpan span) {
        if (span.StartLine > span.EndLine) {
            throw new ArgumentException("Invalid span", nameof(span));
        }

        if (span.StartLine == span.EndLine && span.StartPosition > span.EndPosition) {
            throw new ArgumentException("Invalid span", nameof(span));
        }

        if (span.StartLine < 0 || span.EndLine >= LineCount) {
            throw new ArgumentOutOfRangeException(nameof(span));
        }

        if (span.StartPosition < 0 || span.StartPosition >= _lines[span.StartLine].Length) {
            throw new ArgumentOutOfRangeException(nameof(span));
        }

        if (span.EndPosition < 0 || span.EndPosition >= _lines[span.EndLine].Length) {
            throw new ArgumentOutOfRangeException(nameof(span));
        }
        
        if (span.StartLine == span.EndLine) {
            _lines[span.StartLine].Remove(
                span.StartPosition,
                span.EndPosition - span.StartPosition + 1
            );

            if (_lines[span.StartLine].Length == 0) {
                RemoveLine(span.StartLine);
            }

            _fullContents = null;
            return;
        }

        _lines[span.StartLine].Remove(
            span.StartPosition,
            _lines[span.StartLine].Length - span.StartPosition
        );

        for (int line = span.StartLine + 1; line < span.EndLine; line += 1) {
            _lines[line].Clear();
        }

        _lines[span.EndLine].Remove(0, span.EndPosition + 1);

        if (_lines[span.StartLine].Length > 0) {
            _lines[span.StartLine].Append(_lines[span.EndLine]);
            _lines[span.EndLine].Clear();
        }

        for (int i = span.StartLine, j = span.EndLine; i <= j;) {
            if (_lines[i].Length == 0) {
                RemoveLine(i);
                j -= 1;
            } else {
                i += 1;
            }
        }

        _fullContents = null;
    }
}