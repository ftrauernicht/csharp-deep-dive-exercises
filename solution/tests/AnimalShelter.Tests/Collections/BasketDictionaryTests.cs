using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Collections;
using AnimalShelter.Core.Comparison;

namespace AnimalShelter.Tests.Collections;

/// <summary>Exercises 4.1, 4.2, 4.3 and the extras Z1 and Z2.</summary>
public class BasketDictionaryTests
{
    /// <summary>
    /// A key that hands out whatever hash code the test asks for, so that
    /// collisions can be arranged on purpose instead of hoped for.
    /// </summary>
    private sealed class FixedHashKey
    {
        private readonly int hash;
        private readonly string name;

        public FixedHashKey(int hash, string name)
        {
            this.hash = hash;
            this.name = name;
        }

        public override int GetHashCode() => hash;

        public override bool Equals(object? obj) => obj is FixedHashKey o && o.name == name;

        public override string ToString() => name;
    }

    // --- 4.1 behaviour ---

    [Fact]
    public void Add_ThenRead_ReturnsTheValue()
    {
        BasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);

        Assert.Equal(1, dictionary["one"]);
        Assert.Equal(1, dictionary.Count);
        Assert.True(dictionary.ContainsKey("one"));
    }

    [Fact]
    public void Add_SameKeyTwice_Throws()
    {
        BasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);

        Assert.Throws<ArgumentException>(() => dictionary.Add("one", 2));
    }

    [Fact]
    public void Indexer_UnknownKey_Throws()
    {
        BasketDictionary<string, int> dictionary = new();

        Assert.Throws<KeyNotFoundException>(() => dictionary["missing"]);
    }

    [Fact]
    public void Indexer_Set_AddsOrOverwrites()
    {
        BasketDictionary<string, int> dictionary = new();

        dictionary["one"] = 1;
        Assert.Equal(1, dictionary.Count);

        dictionary["one"] = 2;
        Assert.Equal(2, dictionary["one"]);
        Assert.Equal(1, dictionary.Count);
    }

    [Fact]
    public void TryGetValue_ReportsPresenceWithoutThrowing()
    {
        BasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);

        Assert.True(dictionary.TryGetValue("one", out int found));
        Assert.Equal(1, found);
        Assert.False(dictionary.TryGetValue("two", out int missing));
        Assert.Equal(0, missing);
    }

    [Fact]
    public void Remove_ReportsWhetherSomethingWasRemoved()
    {
        BasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);

        Assert.True(dictionary.Remove("one"));
        Assert.False(dictionary.Remove("one"));
        Assert.Equal(0, dictionary.Count);
        Assert.False(dictionary.ContainsKey("one"));
    }

    [Fact]
    public void KeysValuesAndEnumeration_SeeEverything()
    {
        BasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);
        dictionary.Add("two", 2);
        dictionary.Add("three", 3);

        Assert.Equal(["one", "three", "two"], dictionary.Keys.OrderBy(k => k));
        Assert.Equal([1, 2, 3], dictionary.Values.OrderBy(v => v));
        Assert.Equal(3, dictionary.Count());
    }

    // --- 4.2 special cases ---

    [Theory]
    [InlineData(-7)]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void NegativeAndExtremeHashCodes_Work(int hash)
    {
        // int.MinValue is the one that matters. Math.Abs(int.MinValue) throws,
        // because +2147483648 does not fit in an int -- so the basket index
        // has to mask the sign bit instead of taking an absolute value.
        BasketDictionary<FixedHashKey, string> dictionary = new();
        FixedHashKey key = new(hash, "key");

        dictionary.Add(key, "value");

        Assert.Equal("value", dictionary[key]);
        Assert.True(dictionary.ContainsKey(new FixedHashKey(hash, "key")));
    }

    [Fact]
    public void ThreeKeysWithTheSameHash_AreFoundAndRemovedIndividually()
    {
        BasketDictionary<FixedHashKey, string> dictionary = new();
        FixedHashKey a = new(42, "a");
        FixedHashKey b = new(42, "b");
        FixedHashKey c = new(42, "c");

        dictionary.Add(a, "A");
        dictionary.Add(b, "B");
        dictionary.Add(c, "C");

        Assert.Equal(3, dictionary.Count);
        Assert.Equal("A", dictionary[a]);
        Assert.Equal("B", dictionary[b]);
        Assert.Equal("C", dictionary[c]);

        Assert.True(dictionary.Remove(b));
        Assert.Equal(2, dictionary.Count);
        Assert.False(dictionary.ContainsKey(b));
        Assert.Equal("A", dictionary[a]);
        Assert.Equal("C", dictionary[c]);
    }

    [Fact]
    public void AnimalAsKey_IsFoundThroughADifferentObjectWithTheSameChip()
    {
        BasketDictionary<Animal, string> dictionary = new();
        Animal stored = new Cat("Mimi", "276098100000001", 36, 4.0);
        Animal probe = new Dog("Rex", "276098100000001", 24, 18.0);

        dictionary.Add(stored, "kennel 1");

        // Equality is the chip number, so a different object -- even a
        // different species -- finds the entry.
        Assert.True(dictionary.ContainsKey(probe));
        Assert.Equal("kennel 1", dictionary[probe]);
    }

    // --- 4.3 growth ---

    [Fact]
    public void Growing_KeepsEverythingFindable()
    {
        BasketDictionary<string, int> dictionary = new();

        for (int i = 0; i < 1000; i++)
        {
            dictionary.Add($"key-{i}", i);
        }

        Assert.Equal(1000, dictionary.Count);

        for (int i = 0; i < 1000; i++)
        {
            Assert.Equal(i, dictionary[$"key-{i}"]);
        }
    }

    [Fact]
    public void Growing_DoublesTheBasketCount()
    {
        BasketDictionary<string, int> dictionary = new(initialCapacity: 16);

        Assert.Equal(16, dictionary.BasketCount);

        // 0.75 of 16 is 12, so the thirteenth entry tips it over.
        for (int i = 0; i < 13; i++)
        {
            dictionary.Add($"key-{i}", i);
        }

        Assert.Equal(32, dictionary.BasketCount);
        Assert.Equal(13, dictionary.Count);
    }

    // --- Z1: a comparer of your own ---

    [Fact]
    public void Z1_WithANameComparer_TheKeyIsTheName()
    {
        BasketDictionary<Animal, string> dictionary =
            new(new ByNameEqualityComparer());

        Animal mimi = new Cat("Mimi", "276098100000001", 36, 4.0);
        Animal sameNameOtherChip = new Dog("mimi", "276098100000002", 24, 18.0);

        dictionary.Add(mimi, "first");

        // Animal.Equals would say these two differ. The comparer overrules it.
        Assert.True(dictionary.ContainsKey(sameNameOtherChip));
        Assert.Throws<ArgumentException>(() => dictionary.Add(sameNameOtherChip, "second"));
    }

    [Fact]
    public void Z1_NameComparer_ChangesWhatHashSetAndDistinctMean()
    {
        Animal mimi = new Cat("Mimi", "276098100000001", 36, 4.0);
        Animal minka = new Cat("Minka", "276098100000001", 36, 4.0);

        // By chip number these are the same animal.
        Assert.Single(new HashSet<Animal>([mimi, minka]));

        // By name they are two.
        Assert.Equal(2, new HashSet<Animal>([mimi, minka], new ByNameEqualityComparer()).Count);
        Assert.Equal(2, new[] { mimi, minka }.Distinct(new ByNameEqualityComparer()).Count());
    }

    // --- Z2: changing it while walking it ---

    [Fact]
    public void Z2_AddingDuringEnumeration_Throws()
    {
        BasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);
        dictionary.Add("two", 2);

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (KeyValuePair<string, int> entry in dictionary)
            {
                dictionary.Add($"new-{entry.Key}", 0);
            }
        });
    }

    [Fact]
    public void Z2_OverwritingAValueDuringEnumeration_IsAllowed()
    {
        BasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);
        dictionary.Add("two", 2);

        // Replacing a value moves nothing between baskets, so the enumeration
        // still sees the same keys in the same order. The framework dictionary
        // draws the line in the same place.
        foreach (KeyValuePair<string, int> entry in dictionary)
        {
            dictionary[entry.Key] = entry.Value * 10;
        }

        Assert.Equal([10, 20], dictionary.Values.OrderBy(v => v));
    }
}
