using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Text;

public class MultiLineStringTests {
    public class GetAllLines {
        [Fact]
        public void Should_RetainContents() {
            string original = "hello: " + Guid.NewGuid();
            MultiLineString text = new(original);

            string result = text.GetAllLines();

            Assert.Equal(original, result);
        }

        [Fact]
        public void Should_ExcludePreviouslyRemoved() {
            string original = "1\n2\n3";
            MultiLineString text = new("1\n2\n3");

            string before = text.GetAllLines();
            Assert.Equal(original, before);

            text.RemoveLine(1);
            string after = text.GetAllLines();
            Assert.Equal("1\n3", after);
        }
    }

    public class GetLine {
        [Theory]
        [InlineData("hello", 0, "hello")]
        [InlineData("hello\nworld", 0, "hello\n")]
        [InlineData("hello\nworld", 1, "world")]
        public void Should_ReturnCorrectContents(string text, int line, string expected) {
            MultiLineString mls = new(text);

            string result = mls.GetLine(line);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("hello", -1)]
        [InlineData("hello", 1)]
        [InlineData("a\nb\rc\r\nd", -1)]
        [InlineData("a\nb\rc\r\nd", 4)]
        [InlineData("great\n", -1)]
        [InlineData("great\n", 2)]
        public void Should_FailOutOfBounds(string text, int line) {
            MultiLineString mls = new(text);

            Assert.Throws<ArgumentOutOfRangeException>(() => {
                mls.GetLine(line);
            });
        }

        [Fact]
        public void Should_ExcludePreviouslyRemoved() {
            MultiLineString text = new("1\n2\n3");

            Assert.Equal("1\n", text.GetLine(0));
            Assert.Equal("2\n", text.GetLine(1));

            text.RemoveLine(1);
            Assert.Equal("1\n", text.GetLine(0));
            Assert.Equal("3", text.GetLine(1));
        }
    }

    public class GetLineLength {
        [Theory]
        [InlineData("hello", 0, 5)]
        [InlineData("hello\nworld", 0, 6)]
        [InlineData("hello\nworld", 1, 5)]
        public void Should_ReturnCorrectContents(string text, int line, int expected) {
            MultiLineString mls = new(text);

            int result = mls.GetLineLength(line);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("hello", -1)]
        [InlineData("hello", 1)]
        [InlineData("a\nb\rc\r\nd", -1)]
        [InlineData("a\nb\rc\r\nd", 4)]
        [InlineData("great\n", -1)]
        [InlineData("great\n", 2)]
        public void Should_FailOutOfBounds(string text, int line) {
            MultiLineString mls = new(text);

            Assert.Throws<ArgumentOutOfRangeException>(() => {
                mls.GetLineLength(line);
            });
        }

        [Fact]
        public void Should_ExcludePreviouslyRemoved() {
            MultiLineString text = new("111\n22\n3");

            Assert.Equal(4, text.GetLineLength(0));
            Assert.Equal(3, text.GetLineLength(1));

            text.RemoveLine(1);
            Assert.Equal(4, text.GetLineLength(0));
            Assert.Equal(1, text.GetLineLength(1));
        }
    }

    public class CharAt {
        [Theory]
        [InlineData("hello", 2, 'l')]
        [InlineData("hello", 4, 'o')]
        [InlineData("great\nnews", 3, 'a')]
        [InlineData("great\nnews", 5, '\n')]
        [InlineData("great\nnews", 8, 'w')]
        [InlineData("wow\ncool", 3, '\n')]
        [InlineData("wow\rcool", 3, '\n')]
        [InlineData("wow\r\ncool", 3, '\n')]
        [InlineData("wow\r\ncool", 4, 'c')]
        public void Should_HandleAbsolutePositions(string text, int position, char expected) {
            MultiLineString mls = new(text);

            char result = mls.CharAt(position);
            
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("hello", 0, 2, 'l')]
        [InlineData("hello", 0, 4, 'o')]
        [InlineData("great\nnews", 0, 3, 'a')]
        [InlineData("great\nnews", 0, 5, '\n')]
        [InlineData("great\nnews", 1, 2, 'w')]
        [InlineData("wow\ncool", 0, 3, '\n')]
        [InlineData("wow\rcool", 0, 3, '\n')]
        [InlineData("wow\r\ncool", 0, 3, '\n')]
        [InlineData("wow\r\ncool", 1, 0, 'c')]
        public void Should_HandleLinePositions(string text, int line, int position, char expected) {
            MultiLineString mls = new(text);

            char result = mls.CharAt(line, position);
            
            Assert.Equal(expected, result);
        }
    }

    public class RemoveLine {
        [Theory]
        [InlineData("hello", -1)]
        [InlineData("hello", 1)]
        [InlineData("a\nb\rc\r\nd", -1)]
        [InlineData("a\nb\rc\r\nd", 4)]
        [InlineData("great\n", -1)]
        [InlineData("great\n", 2)]
        public void Should_FailOutOfBounds(string text, int line) {
            MultiLineString mls = new(text);

            Assert.Throws<ArgumentOutOfRangeException>(() => {
                mls.RemoveLine(line);
            });
        }

        [Theory]
        [InlineData("hello", 0, 0)]
        public void Should_ReturnCorrectLineCount(string text, int line, int expected) {
            MultiLineString mls = new(text);

            Assert.Equal(expected + 1, mls.LineCount);
            mls.RemoveLine(line);
            Assert.Equal(expected, mls.LineCount);
        }
    }

    public class RemoveSubstring {
        [Theory]
        [InlineData("hello", 0, 1, 0, 3, "ho", 1)]
        [InlineData("hello", 0, 1, 0, 4, "h", 1)]
        [InlineData("hello", 0, 0, 0, 3, "o", 1)]
        [InlineData("hello", 0, 0, 0, 4, "", 0)]
        [InlineData("hello\nworld", 0, 0, 0, 4, "\nworld", 2)]
        [InlineData("hello\nworld", 0, 0, 1, 1, "rld", 1)]
        [InlineData("hello\nworld\ncool", 0, 0, 1, 1, "rld\ncool", 2)]
        [InlineData("hello\nworld\ncool", 0, 0, 2, 1, "ol", 1)]
        [InlineData("hello\nworld\ncool", 0, 1, 2, 1, "hol", 1)]
        [InlineData("hello\nworld\ncool\nnice", 0, 1, 2, 1, "hol\nnice", 2)]
        public void Should_ProperlyExclude(string original, int startLine, int startPosition, int endLine, int endPosition, string expectedValue, int expectedLines) {
            MultiLineString mls = new(original);
            DocumentSpan span = new(
                documentId: mls.DocumentId,
                startLine: startLine,
                startPosition: startPosition,
                endLine: endLine,
                endPosition: endPosition
            );

            mls.RemoveSubstring(span);

            Assert.Equal(expectedValue, mls.GetAllLines());
            Assert.Equal(expectedLines, mls.LineCount);
        }

        [Theory]
        [InlineData("hello", -1, 0, 0, 0)]
        [InlineData("hello", 0, 0, 1, 0)]
        [InlineData("hello\nsweet\nworld", -1, 0, 0, 0)]
        [InlineData("hello\nsweet\nworld", 0, 0, 3, 0)]
        [InlineData("hello", 0, -1, 0, 0)]
        [InlineData("hello", 0, 0, 0, 5)]
        [InlineData("hello\nworld", 0, 6, 1, 0)]
        [InlineData("hello\nworld", 0, 0, 1, -1)]
        public void Should_ThrowOutOfBounds(string text, int startLine, int startPosition, int endLine, int endPosition) {
            MultiLineString mls = new(text);
            DocumentSpan span = new(
                documentId: mls.DocumentId,
                startLine: startLine,
                startPosition: startPosition,
                endLine: endLine,
                endPosition: endPosition
            );

            Assert.Throws<ArgumentOutOfRangeException>(() => {
                mls.RemoveSubstring(span);
            });

            Assert.Equal(text, mls.GetAllLines());
        }
    }
}