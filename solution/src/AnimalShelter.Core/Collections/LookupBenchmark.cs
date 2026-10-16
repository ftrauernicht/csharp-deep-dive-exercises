using System.Diagnostics;

namespace AnimalShelter.Core.Collections;

/// <summary>
/// Exercise Z4: how long a lookup takes in three containers as they grow.
///
/// This measures on the machine it runs on and reports what it found. It
/// deliberately states no absolute numbers anywhere -- those would be true on
/// one machine and wrong on the next. What does hold everywhere is the shape:
/// the two dictionaries answer in roughly constant time however many entries
/// they hold, while the list has to walk them, so its cost grows with the
/// count. That shape is what the test asserts.
/// </summary>
public static class LookupBenchmark
{
    public sealed record Result(
        int EntryCount,
        double BasketDictionaryMs,
        double FrameworkDictionaryMs,
        double LinearListMs)
    {
        /// <summary>How many times slower the linear search was.</summary>
        public double ListVersusBasket => LinearListMs / BasketDictionaryMs;

        public override string ToString() =>
            $"{EntryCount,8}  basket {BasketDictionaryMs,9:F3} ms   " +
            $"dictionary {FrameworkDictionaryMs,9:F3} ms   " +
            $"list {LinearListMs,11:F3} ms   (list is {ListVersusBasket,7:F1}x slower)";
    }

    public static Result Measure(int entryCount, int lookups = 1000)
    {
        var basket = new BasketDictionary<string, int>();
        var framework = new Dictionary<string, int>();
        var list = new List<KeyValuePair<string, int>>();

        for (int i = 0; i < entryCount; i++)
        {
            string key = $"key-{i}";
            basket.Add(key, i);
            framework.Add(key, i);
            list.Add(new KeyValuePair<string, int>(key, i));
        }

        // Look up keys spread across the whole range, so the linear search is
        // neither always lucky nor always worst case.
        string[] probes = new string[lookups];

        for (int i = 0; i < lookups; i++)
        {
            probes[i] = $"key-{(long)i * entryCount / lookups}";
        }

        return new Result(
            entryCount,
            Time(() => { foreach (string p in probes) { basket.TryGetValue(p, out _); } }),
            Time(() => { foreach (string p in probes) { framework.TryGetValue(p, out _); } }),
            Time(() =>
            {
                foreach (string p in probes)
                {
                    foreach (KeyValuePair<string, int> entry in list)
                    {
                        if (entry.Key == p)
                        {
                            break;
                        }
                    }
                }
            }));
    }

    public static IEnumerable<Result> MeasureAll(params int[] entryCounts) =>
        entryCounts.Select(count => Measure(count)).ToList();

    private static double Time(Action action)
    {
        action();   // once to warm up, so the first call does not pay for JIT
        var watch = Stopwatch.StartNew();
        action();
        watch.Stop();
        return watch.Elapsed.TotalMilliseconds;
    }
}
