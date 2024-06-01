using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Text;

public class SlidingStringWindowTests {
    public class Get {
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
        public void Should_ProxyContainedMultiLineString(string text, int position, char expected) {
            MultiLineString mls = new(text);
            SlidingStringWindow ssw = new(mls);

            char result = ssw.Get(position);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_EOFPastEndOfContents() {
            MultiLineString mls = new("hello");
            SlidingStringWindow ssw = new(mls);

            Assert.Equal('\0', ssw.Peek(5));
            Assert.Equal('\0', ssw.Peek(50));
            Assert.Equal('\0', ssw.Peek(500));
        }
    }

    public class Build {
        [Theory]
        [InlineData("hello", 0, 2, "he")]
        [InlineData("hello", 1, 2, "el")]
        [InlineData("hello\nworld", 3, 3, "lo\n")]
        public void Should_CorrectlyBuildSpan(string text, int skip, int take, string expectedValue) {
            MultiLineString mls = new(text);
            SlidingStringWindow ssw = new(mls);
            
            StringSpan result = ssw.Build(skip, skip + take - 1);
            Assert.Equal(expectedValue, mls.GetSubstring(result));
        }

        [Fact]
        public void Should_DisallowCrossingLineBoundaries() {
            MultiLineString mls = new("hello\nworld");
            SlidingStringWindow ssw = new(mls);

            Assert.Throws<InvalidCastException>(() => {
                ssw.Build(0, mls.GetAllLines().Length - 1);
            });
        }
    }
}