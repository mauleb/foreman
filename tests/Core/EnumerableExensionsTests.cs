using Foreman.Core;

namespace Core;

public class EnumerableExensionsTests
{
    [Theory]
    [InlineData(new int[] {0,1,2,3}, 2)]
    [InlineData(new int[] {3,3,3}, 0)]
    [InlineData(new int[] {0,0,0,0,0,0,0,0}, -1)]
    [InlineData(new int[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2,2,2,2,2}, 23)]
    public void IndexWhere_ShouldFindIndex(int[] list, int expected) {
        int result = list.IndexWhere(i => i > 1);
        Assert.Equal(expected, result);
    }
}