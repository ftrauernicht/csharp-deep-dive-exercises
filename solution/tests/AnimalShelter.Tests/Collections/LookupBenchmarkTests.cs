using AnimalShelter.Core.Collections;
using Xunit.Abstractions;

namespace AnimalShelter.Tests.Collections;

/// <summary>
/// Exercise Z4. The benchmark measures the machine it runs on, so a test may
/// not claim milliseconds: the same code answers in a different number of
/// them on a laptop on battery, on a build agent and under a debugger. What
/// does not change is the shape. A dictionary answers in roughly constant
/// time whatever it holds, a linear search has to walk, so the gap between
/// them widens as the collection grows. That is what is asserted here.
///
/// For the record, measured on the development machine: the list was 2.7x
/// slower at 100 entries, 36x at 10.000 and 2903x at 100.000. Those three
/// numbers are true there and nowhere else, which is exactly why none of them
/// appears in an assertion.
/// </summary>
public class LookupBenchmarkTests
{
    private readonly ITestOutputHelper output;

    public LookupBenchmarkTests(ITestOutputHelper output) => this.output = output;

    [Theory]
    [InlineData(100)]
    [InlineData(1_000)]
    [InlineData(10_000)]
    public void Measure_ForEverySize_ReportsPlausibleValues(int entryCount)
    {
        LookupBenchmark.Result result = LookupBenchmark.Measure(entryCount);
        output.WriteLine(result.ToString());

        Assert.Equal(entryCount, result.EntryCount);
        Assert.True(result.BasketDictionaryMs > 0, $"basket time was {result.BasketDictionaryMs}");
        Assert.True(result.FrameworkDictionaryMs > 0, $"framework time was {result.FrameworkDictionaryMs}");
        Assert.True(result.LinearListMs > 0, $"list time was {result.LinearListMs}");
    }

    [Fact]
    public void Measure_WithAnExplicitLookupCount_StillAnswersForThatSize()
    {
        LookupBenchmark.Result result = LookupBenchmark.Measure(100, lookups: 50);
        output.WriteLine(result.ToString());

        Assert.Equal(100, result.EntryCount);
        Assert.True(result.BasketDictionaryMs > 0, $"basket time was {result.BasketDictionaryMs}");
        Assert.True(result.LinearListMs > 0, $"list time was {result.LinearListMs}");
    }

    [Fact]
    public void ListVersusBasket_IsConsistentWithTheTwoTimes()
    {
        LookupBenchmark.Result result = LookupBenchmark.Measure(1_000);

        Assert.Equal(result.LinearListMs / result.BasketDictionaryMs, result.ListVersusBasket);
    }

    // The two tests below build 100.000 entries in three containers and walk
    // the list for every probe, so they are the slow ones in this suite --
    // seconds, where every other test here is milliseconds. The trait lets a
    // quick run skip them:
    //     dotnet test --filter "Category!=Slow"
    // They stay in the normal run by default, because the growing gap is the
    // whole claim of Z4 and only shows up at this size.

    [Fact]
    [Trait("Category", "Slow")]
    public void LinearList_AtOneHundredThousandEntries_IsFarSlowerThanTheBasketDictionary()
    {
        LookupBenchmark.Result result = LookupBenchmark.Measure(100_000);
        output.WriteLine(result.ToString());

        // The development machine saw 2903x. Twenty is the floor: low enough
        // that a slow, shared or throttled machine still clears it, high
        // enough that it can only pass if the list really is walking the
        // entries while the dictionary is not.
        Assert.True(
            result.LinearListMs >= result.BasketDictionaryMs * 20,
            "expected the list to be at least 20x slower, measured " +
            $"{result.ListVersusBasket:F1}x ({result.LinearListMs:F3} ms against " +
            $"{result.BasketDictionaryMs:F3} ms)");
    }

    [Fact]
    [Trait("Category", "Slow")]
    public void ListVersusBasket_GrowsWithTheEntryCount()
    {
        LookupBenchmark.Result small = LookupBenchmark.Measure(100);
        LookupBenchmark.Result large = LookupBenchmark.Measure(100_000);

        output.WriteLine(small.ToString());
        output.WriteLine(large.ToString());

        // This is the actual statement of the exercise, and it needs no
        // absolute number at all: whatever the machine, a thousandfold more
        // entries cost the dictionary next to nothing and the list a walk
        // through all of them, so the ratio has to be bigger at the top.
        Assert.True(
            large.ListVersusBasket > small.ListVersusBasket,
            $"expected the gap to widen, measured {small.ListVersusBasket:F1}x at " +
            $"{small.EntryCount} entries and {large.ListVersusBasket:F1}x at " +
            $"{large.EntryCount}");
    }

    [Fact]
    public void MeasureAll_ReturnsOneResultPerSizeInOrder()
    {
        List<LookupBenchmark.Result> results = LookupBenchmark.MeasureAll(100, 10_000).ToList();

        foreach (LookupBenchmark.Result result in results)
        {
            output.WriteLine(result.ToString());
        }

        Assert.Equal([100, 10_000], results.Select(r => r.EntryCount));
    }
}
