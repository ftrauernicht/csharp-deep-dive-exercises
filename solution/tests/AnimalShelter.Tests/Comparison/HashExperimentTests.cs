using System.Diagnostics;

namespace AnimalShelter.Tests.Comparison;

/// <summary>
/// Exercise 3.3, the three hash experiments.
///
/// The task says to break Animal, watch, and put it back. A solution that is
/// checked by CI cannot leave Animal broken, so the broken states live here as
/// small stand-ins instead: each one is wrong in exactly one way, and the
/// tests pin down what that costs. Reading them side by side is the point.
/// </summary>
public class HashExperimentTests
{
    private const string Chip = "276098100000001";

    /// <summary>Experiment A: Equals by value, GetHashCode left alone.</summary>
    ///
    // CS0659 is switched off for this one class on purpose. The compiler is
    // right -- overriding Equals without GetHashCode is a bug -- and that bug
    // is what the experiment is about. Ignoring the warning is how people end
    // up here in the first place.
#pragma warning disable CS0659
    private sealed class NoHashOverride
    {
        public NoHashOverride(string chip) => Chip = chip;

        public string Chip { get; }

        public override bool Equals(object? obj) => obj is NoHashOverride o && o.Chip == Chip;

        // GetHashCode deliberately not overridden -- this is the bug.
    }
#pragma warning restore CS0659

    /// <summary>Experiment B: a hash code that is always the same.</summary>
    private sealed class ConstantHash
    {
        public ConstantHash(string chip) => Chip = chip;

        public string Chip { get; }

        public override bool Equals(object? obj) => obj is ConstantHash o && o.Chip == Chip;

        public override int GetHashCode() => 1;
    }

    /// <summary>Experiment C: a key whose hash can change after it is stored.</summary>
    private sealed class MutableKey
    {
        public string Chip { get; set; } = string.Empty;

        public override bool Equals(object? obj) => obj is MutableKey o && o.Chip == Chip;

        public override int GetHashCode() => Chip.GetHashCode();
    }

    [Fact]
    public void A_WithoutGetHashCode_HashSetKeepsBothAndLookupFails()
    {
        NoHashOverride first = new(Chip);
        NoHashOverride second = new(Chip);

        // Prediction: the two are Equal, so a correct type would collapse them.
        Assert.True(first.Equals(second));

        // Prediction: the set holds 2. Object.GetHashCode is per instance, so
        // the two land in different buckets and never meet to be compared.
        HashSet<NoHashOverride> set = [first, second];
        Assert.Equal(2, set.Count);

        // Prediction: false. ContainsKey hashes the probe, looks in that one
        // bucket, and the stored entry is not in it.
        Dictionary<NoHashOverride, string> map = new() { [first] = "Mimi" };
        bool foundWithAnEqualObject = map.ContainsKey(second);
        Assert.False(foundWithAnEqualObject);

        // And with the very same object it does work, which is what makes the
        // bug so slippery.
        bool foundWithTheSameObject = map.ContainsKey(first);
        Assert.True(foundWithTheSameObject);

        // Taking the LINQ road changes nothing, which is worth measuring
        // rather than guessing: Enumerable.Contains asks the collection itself
        // first, and Dictionary's KeyCollection implements ICollection<T>,
        // whose Contains hashes the probe exactly the way ContainsKey does.
        bool foundThroughKeysContains = map.Keys.Contains(second);
        Assert.False(foundThroughKeysContains);

        // Only a lookup that really walks the keys and asks Equals finds it.
        // The entry is present and unreachable at the same time, and which of
        // the two it is depends on the road you take to it.
        bool foundByWalkingTheKeys = map.Keys.Any(key => key.Equals(second));
        Assert.True(foundByWalkingTheKeys);
    }

    [Fact]
    public void B_ConstantHash_StillCorrectButEverythingInOneBucket()
    {
        ConstantHash first = new("276098100000001");
        ConstantHash second = new("276098100000002");

        // Prediction: both work correctly. A constant hash is legal -- equal
        // objects do have equal hash codes. It is only slow.
        HashSet<ConstantHash> set = [first, second];
        Assert.Equal(2, set.Count);
        Assert.Contains(new ConstantHash("276098100000001"), set);
    }

    [Fact]
    public void B_ConstantHash_IsDramaticallySlower()
    {
        const int count = 20_000;

        double constantMs = Time(() =>
        {
            HashSet<ConstantHash> set = [];
            for (int i = 0; i < count; i++)
            {
                set.Add(new ConstantHash($"{i:D15}"));
            }
        });

        double properMs = Time(() =>
        {
            HashSet<string> set = [];
            for (int i = 0; i < count; i++)
            {
                set.Add($"{i:D15}");
            }
        });

        // No millisecond figure is asserted: those differ per machine. What
        // holds everywhere is the shape. One bucket turns every insert into a
        // walk of everything already in it, so the cost grows with the square
        // of the count while a real hash stays flat. Measured here at 20 000
        // entries the gap was several hundredfold; the factor of 10 below is
        // the floor, not the expectation.
        Assert.True(constantMs > properMs * 10,
            $"constant hash {constantMs:F1} ms vs proper hash {properMs:F1} ms");
    }

    [Fact]
    public void C_MutatingAKeyAfterInsertingItLosesIt()
    {
        MutableKey key = new() { Chip = "276098100000001" };
        HashSet<MutableKey> set = [key];

        bool beforeTheChange = set.Contains(key);
        Assert.True(beforeTheChange);

        key.Chip = "276098100000002";

        // Prediction: false, with the very same object. The entry still sits
        // in the bucket for the old hash, and Contains now looks in the bucket
        // for the new one. Nothing is corrupt, nothing throws -- the item is
        // simply unreachable, which is why a key should be immutable.
        bool afterTheChange = set.Contains(key);
        Assert.False(afterTheChange);

        // It is still in there, though:
        Assert.Single(set);
    }

    private static double Time(Action action)
    {
        action();
        Stopwatch watch = Stopwatch.StartNew();
        action();
        watch.Stop();
        return watch.Elapsed.TotalMilliseconds;
    }
}
