using Foreman.CodeAnalysis.Text;

namespace CodeAnalysis.Text;

public class StringSpanTests {
    [Fact]
    public void Length_Should_TreatEndInclusively() {
        StringSpan span = new() {
            AbsolutePosition = 0,
            Line = 0,
            Start = 0,
            End = 0
        };

        Assert.Equal(1, span.Length);
    }
}