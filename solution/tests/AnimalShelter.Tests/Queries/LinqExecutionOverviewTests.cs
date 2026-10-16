namespace AnimalShelter.Tests.Queries;

/// <summary>
/// Every figure in solution/LINQ.md, pinned down.
///
/// The file answers exercise 5.3 with a table of how many elements each LINQ
/// method actually pulls out of its source. Those numbers were measured rather
/// than looked up, and a measurement that nothing re-runs is a claim again two
/// releases later. So the same counting source runs here, and the table and
/// these assertions have to say the same thing.
/// </summary>
public class LinqExecutionOverviewTests
{
    /// <summary>
    /// Hands out five numbers and writes down every one it is asked for, so
    /// the log says how far somebody pulled the query.
    /// </summary>
    private static IEnumerable<int> Source(List<string> log)
    {
        for (int i = 1; i <= 5; i++)
        {
            log.Add($"pull {i}");
            yield return i;
        }
    }

    [Theory]
    // Pull everything, because the answer cannot be known earlier.
    [InlineData("ToList", 5)]
    [InlineData("ToArray", 5)]
    [InlineData("ToDictionary", 5)]
    [InlineData("ToHashSet", 5)]
    [InlineData("Count", 5)]
    [InlineData("Sum", 5)]
    [InlineData("Max", 5)]
    [InlineData("Average", 5)]
    [InlineData("Last", 5)]
    [InlineData("Aggregate", 5)]
    // Stop as soon as the answer is settled. Immediate does not mean exhaustive.
    [InlineData("First", 1)]
    [InlineData("Any", 1)]
    [InlineData("AnyWithPredicate", 3)]
    [InlineData("AllWithPredicate", 2)]
    [InlineData("Contains", 2)]
    [InlineData("ElementAt", 2)]
    // The odd one out: the match is the first element, and it still reads to
    // the end, because it has to prove there is no second one.
    [InlineData("SingleWithPredicate", 5)]
    public void Immediate_PullsWhatTheTableSays(string method, int expectedPulls)
    {
        List<string> log = [];

        RunImmediate(method, Source(log));

        Assert.Equal(expectedPulls, log.Count);
    }

    [Theory]
    // Streaming: one element in, one element out.
    [InlineData("Where", 1)]
    [InlineData("Select", 1)]
    [InlineData("OfType", 1)]
    [InlineData("Cast", 1)]
    [InlineData("Distinct", 1)]
    [InlineData("Take", 1)]
    // Skip has to pull what it throws away, so the first element it hands out
    // costs two. The only row in this half whose answer is not 1.
    [InlineData("Skip", 2)]
    [InlineData("Except", 1)]
    [InlineData("Concat", 1)]
    [InlineData("SelectMany", 1)]
    // Buffering: deferred, but the first element costs the whole source.
    [InlineData("OrderBy", 5)]
    [InlineData("OrderByDescending", 5)]
    [InlineData("GroupBy", 5)]
    [InlineData("Reverse", 5)]
    public void Deferred_BuildsWithoutPulling_ThenPullsWhatTheTableSays(
        string method, int pullsForTheFirstElement)
    {
        List<string> log = [];

        IEnumerable<object> query = BuildDeferred(method, Source(log));

        // Building a query does not touch the source. This is the first column
        // of the table, and it is 0 for every row.
        Assert.Empty(log);

        using IEnumerator<object> enumerator = query.GetEnumerator();
        enumerator.MoveNext();

        Assert.Equal(pullsForTheFirstElement, log.Count);
    }

    /// <summary>
    /// The second column is what separates the two halves of the table: both
    /// are deferred, only one of them is streaming.
    /// </summary>
    [Fact]
    public void Deferred_IsNotTheSameAsStreaming()
    {
        List<string> streamingLog = [];
        List<string> bufferingLog = [];

        using IEnumerator<object> streaming =
            BuildDeferred("Where", Source(streamingLog)).GetEnumerator();
        using IEnumerator<object> buffering =
            BuildDeferred("OrderBy", Source(bufferingLog)).GetEnumerator();

        streaming.MoveNext();
        buffering.MoveNext();

        int streamingPulls = streamingLog.Count;
        int bufferingPulls = bufferingLog.Count;

        Assert.Equal(1, streamingPulls);
        Assert.Equal(5, bufferingPulls);
    }

    private static void RunImmediate(string method, IEnumerable<int> source)
    {
        switch (method)
        {
            case "ToList": _ = source.ToList(); break;
            case "ToArray": _ = source.ToArray(); break;
            case "ToDictionary": _ = source.ToDictionary(i => i); break;
            case "ToHashSet": _ = source.ToHashSet(); break;
            case "Count": _ = source.Count(); break;
            case "Sum": _ = source.Sum(); break;
            case "Max": _ = source.Max(); break;
            case "Average": _ = source.Average(); break;
            case "Last": _ = source.Last(); break;
            case "Aggregate": _ = source.Aggregate((a, b) => a + b); break;
            case "First": _ = source.First(); break;
            case "Any": _ = source.Any(); break;
            case "AnyWithPredicate": _ = source.Any(i => i == 3); break;
            case "AllWithPredicate": _ = source.All(i => i < 2); break;
            case "Contains": _ = source.Contains(2); break;
            case "ElementAt": _ = source.ElementAt(1); break;
            case "SingleWithPredicate": _ = source.Single(i => i == 1); break;
            default: throw new ArgumentException($"Unknown method '{method}'.", nameof(method));
        }
    }

    private static IEnumerable<object> BuildDeferred(string method, IEnumerable<int> source) =>
        method switch
        {
            "Where" => source.Where(i => true).Cast<object>(),
            "Select" => source.Select(i => (object)i),
            "OfType" => source.OfType<int>().Cast<object>(),
            "Cast" => source.Cast<object>(),
            "Distinct" => source.Distinct().Cast<object>(),
            "Take" => source.Take(2).Cast<object>(),
            "Skip" => source.Skip(1).Cast<object>(),
            "Except" => source.Except([3]).Cast<object>(),
            "Concat" => source.Concat([9]).Cast<object>(),
            "SelectMany" => source.SelectMany(i => new[] { i }).Cast<object>(),
            "OrderBy" => source.OrderBy(i => i).Cast<object>(),
            "OrderByDescending" => source.OrderByDescending(i => i).Cast<object>(),
            "GroupBy" => source.GroupBy(i => i % 2).Cast<object>(),
            "Reverse" => source.Reverse().Cast<object>(),
            _ => throw new ArgumentException($"Unknown method '{method}'.", nameof(method)),
        };
}
