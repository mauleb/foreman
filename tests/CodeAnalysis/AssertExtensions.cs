namespace CodeAnalysis;

public static class CustomAssert {
    public static void Any(params bool[] bools) {
        bool result = bools.Aggregate((a,b) => a || b);
        Assert.True(result);
    }

    public static void All(params bool[] bools) {
        bool result = bools.Aggregate((a,b) => a && b);
        Assert.True(result);
    }
}