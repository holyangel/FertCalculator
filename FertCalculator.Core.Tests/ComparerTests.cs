using FertCalculator.Core;
using Xunit;

namespace FertCalculator.Core.Tests;

public class ComparerTests
{
    private readonly FertilizerNameComparer comparer = new();

    [Fact]
    public void Compare_NumericSegments_SortNumericallyNotLexically()
    {
        Assert.True(comparer.Compare("Jacks 5-15-26", "Jacks 10-30-20") < 0);
    }

    [Fact]
    public void Compare_IsCaseInsensitive()
    {
        Assert.Equal(0, comparer.Compare("Urea", "urea"));
    }

    [Fact]
    public void Compare_HandlesNulls()
    {
        Assert.Equal(0, comparer.Compare(null, null));
        Assert.True(comparer.Compare(null, "x") < 0);
        Assert.True(comparer.Compare("x", null) > 0);
    }

    [Fact]
    public void Sort_MixedNames_OrdersNaturally()
    {
        var names = new[] { "Jacks 10-30-20", "Boron", "Jacks 5-15-26", "urea" };
        var sorted = names.OrderBy(n => n, comparer).ToArray();
        Assert.Equal(new[] { "Boron", "Jacks 5-15-26", "Jacks 10-30-20", "urea" }, sorted);
    }
}
