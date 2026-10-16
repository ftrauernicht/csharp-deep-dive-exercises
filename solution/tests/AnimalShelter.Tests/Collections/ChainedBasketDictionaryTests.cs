using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Collections;

namespace AnimalShelter.Tests.Collections;

/// <summary>
/// Exercise Z3: the same properties BasketDictionaryTests checks on the
/// List-backed version, checked again on the hand-written chain.
///
/// The interesting part is at the bottom. A List hides the removal from you:
/// RemoveAt knows nothing about neighbours. A chain does not, because the
/// entry in front of the one being dropped has to point past it -- and the
/// first entry in a basket has no entry in front of it. Those are two code
/// paths, so they get two sets of tests.
/// </summary>
public class ChainedBasketDictionaryTests
{
    /// <summary>
    /// A key that hands out whatever hash code the test asks for, so that
    /// collisions can be arranged on purpose instead of hoped for. Every key
    /// built with the same hash lands in the same basket, which is the only
    /// way to get a chain longer than one entry on purpose.
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

    // --- the same behaviour as the List version ---

    [Fact]
    public void Add_ThenRead_ReturnsTheValue()
    {
        ChainedBasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);

        Assert.Equal(1, dictionary["one"]);
        Assert.Equal(1, dictionary.Count);
        Assert.True(dictionary.ContainsKey("one"));
    }

    [Fact]
    public void Add_SameKeyTwice_Throws()
    {
        ChainedBasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);

        Assert.Throws<ArgumentException>(() => dictionary.Add("one", 2));
    }

    [Fact]
    public void Indexer_UnknownKey_Throws()
    {
        ChainedBasketDictionary<string, int> dictionary = new();

        Assert.Throws<KeyNotFoundException>(() => dictionary["missing"]);
    }

    [Fact]
    public void Indexer_Set_AddsOrOverwrites()
    {
        ChainedBasketDictionary<string, int> dictionary = new();

        dictionary["one"] = 1;
        Assert.Equal(1, dictionary.Count);

        dictionary["one"] = 2;
        Assert.Equal(2, dictionary["one"]);
        Assert.Equal(1, dictionary.Count);
    }

    [Fact]
    public void TryGetValue_ReportsPresenceWithoutThrowing()
    {
        ChainedBasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);

        Assert.True(dictionary.TryGetValue("one", out int found));
        Assert.Equal(1, found);
        Assert.False(dictionary.TryGetValue("two", out int missing));
        Assert.Equal(0, missing);
    }

    [Fact]
    public void Remove_ReportsWhetherSomethingWasRemoved()
    {
        ChainedBasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);

        Assert.True(dictionary.Remove("one"));
        Assert.False(dictionary.Remove("one"));
        Assert.Equal(0, dictionary.Count);
        Assert.False(dictionary.ContainsKey("one"));
    }

    [Fact]
    public void KeysValuesAndEnumeration_SeeEverything()
    {
        ChainedBasketDictionary<string, int> dictionary = new();
        dictionary.Add("one", 1);
        dictionary.Add("two", 2);
        dictionary.Add("three", 3);

        Assert.Equal(["one", "three", "two"], dictionary.Keys.OrderBy(k => k));
        Assert.Equal([1, 2, 3], dictionary.Values.OrderBy(v => v));
        Assert.Equal(3, dictionary.Count());
    }

    [Theory]
    [InlineData(-7)]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void NegativeAndExtremeHashCodes_Work(int hash)
    {
        // int.MinValue is the one that matters. Math.Abs(int.MinValue) throws,
        // because +2147483648 does not fit in an int -- so the basket index
        // has to mask the sign bit instead of taking an absolute value. The
        // chained version computes the index exactly the same way, so the
        // same case has to hold here.
        ChainedBasketDictionary<FixedHashKey, string> dictionary = new();
        FixedHashKey key = new(hash, "key");

        dictionary.Add(key, "value");

        Assert.Equal("value", dictionary[key]);
        Assert.True(dictionary.ContainsKey(new FixedHashKey(hash, "key")));
    }

    [Fact]
    public void ThreeKeysWithTheSameHash_AreFoundAndRemovedIndividually()
    {
        ChainedBasketDictionary<FixedHashKey, string> dictionary = new();
        FixedHashKey a = new(42, "a");
        FixedHashKey b = new(42, "b");
        FixedHashKey c = new(42, "c");

        dictionary.Add(a, "a");
        dictionary.Add(b, "b");
        dictionary.Add(c, "c");

        Assert.Equal(3, dictionary.Count);
        Assert.Equal("a", dictionary[a]);
        Assert.Equal("b", dictionary[b]);
        Assert.Equal("c", dictionary[c]);

        Assert.True(dictionary.Remove(b));
        Assert.Equal(2, dictionary.Count);
        Assert.False(dictionary.ContainsKey(b));
        Assert.Equal("a", dictionary[a]);
        Assert.Equal("c", dictionary[c]);
    }

    [Fact]
    public void AnimalAsKey_IsFoundThroughADifferentObjectWithTheSameChip()
    {
        ChainedBasketDictionary<Animal, string> dictionary = new();
        Animal stored = new Cat("Mimi", "276098100000001", 36, 4.0);
        Animal probe = new Dog("Rex", "276098100000001", 24, 18.0);

        dictionary.Add(stored, "kennel 1");

        Assert.True(dictionary.ContainsKey(probe));
        Assert.Equal("kennel 1", dictionary[probe]);
    }

    // --- growth ---

    [Fact]
    public void Growing_KeepsEverythingFindable()
    {
        ChainedBasketDictionary<string, int> dictionary = new();

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
        ChainedBasketDictionary<string, int> dictionary = new(initialCapacity: 16);

        Assert.Equal(16, dictionary.BasketCount);

        // 0.75 of 16 is 12, so the thirteenth entry tips it over.
        for (int i = 0; i < 13; i++)
        {
            dictionary.Add($"key-{i}", i);
        }

        Assert.Equal(32, dictionary.BasketCount);
        Assert.Equal(13, dictionary.Count);
    }

    [Fact]
    public void Growing_WithAChainInOneBasket_KeepsTheWholeChain()
    {
        // Rehashing unlinks every entry and relinks it into the bigger array.
        // Colliding keys stay together, so one basket has to come out of the
        // move with its chain intact rather than with its tail dropped.
        ChainedBasketDictionary<FixedHashKey, string> dictionary = new(initialCapacity: 16);
        List<FixedHashKey> collide = [new(42, "a"), new(42, "b"), new(42, "c")];

        foreach (FixedHashKey key in collide)
        {
            dictionary.Add(key, key.ToString());
        }

        for (int i = 0; i < 13; i++)
        {
            dictionary.Add(new FixedHashKey(1000 + i, $"spread-{i}"), $"spread-{i}");
        }

        Assert.Equal(32, dictionary.BasketCount);
        Assert.Equal(16, dictionary.Count);

        foreach (FixedHashKey key in collide)
        {
            AssertFindable(dictionary, key);
        }
    }

    // --- the point of Z3: dropping an entry out of a chain ---
    //
    // Removing from a chain has two cases. The head of a basket is reached
    // through the basket slot, so dropping it means writing the slot. Every
    // later entry is reached through its predecessor's Next, so dropping it
    // means writing that Next. Get either one wrong and the entries behind
    // the gap are still in memory but no longer reachable -- which is why
    // each of these tests checks the two survivors and not just the count.
    //
    // Add prepends, so the chain runs in reverse insertion order: adding a,
    // b, c leaves the head at c. The tests do not take that on trust; they
    // read the order back out of the enumeration, which walks each chain from
    // its head.

    [Fact]
    public void Chain_RunsInReverseInsertionOrder()
    {
        ChainedBasketDictionary<FixedHashKey, string> dictionary = BuildChainOfThree();

        Assert.Equal(["c", "b", "a"], ChainOrder(dictionary).Select(key => key.ToString()));
    }

    [Fact]
    public void Remove_HeadOfAChain_LeavesTheOtherTwoFindable()
    {
        // No predecessor: the basket slot itself has to be repointed.
        ChainedBasketDictionary<FixedHashKey, string> dictionary = BuildChainOfThree();
        List<FixedHashKey> chain = ChainOrder(dictionary);

        Assert.True(dictionary.Remove(chain[0]));

        Assert.Equal(2, dictionary.Count);
        Assert.False(dictionary.ContainsKey(chain[0]));
        AssertFindable(dictionary, chain[1]);
        AssertFindable(dictionary, chain[2]);
        Assert.Equal([chain[1], chain[2]], ChainOrder(dictionary));
    }

    [Fact]
    public void Remove_MiddleOfAChain_LeavesTheOtherTwoFindable()
    {
        // The predecessor has to skip over the dropped entry and keep its
        // tail. Losing the tail here is the classic chain bug.
        ChainedBasketDictionary<FixedHashKey, string> dictionary = BuildChainOfThree();
        List<FixedHashKey> chain = ChainOrder(dictionary);

        Assert.True(dictionary.Remove(chain[1]));

        Assert.Equal(2, dictionary.Count);
        Assert.False(dictionary.ContainsKey(chain[1]));
        AssertFindable(dictionary, chain[0]);
        AssertFindable(dictionary, chain[2]);
        Assert.Equal([chain[0], chain[2]], ChainOrder(dictionary));
    }

    [Fact]
    public void Remove_TailOfAChain_LeavesTheOtherTwoFindable()
    {
        // The predecessor's Next becomes null. Walking the chain afterwards
        // has to stop there instead of running into the dropped entry.
        ChainedBasketDictionary<FixedHashKey, string> dictionary = BuildChainOfThree();
        List<FixedHashKey> chain = ChainOrder(dictionary);

        Assert.True(dictionary.Remove(chain[2]));

        Assert.Equal(2, dictionary.Count);
        Assert.False(dictionary.ContainsKey(chain[2]));
        AssertFindable(dictionary, chain[0]);
        AssertFindable(dictionary, chain[1]);
        Assert.Equal([chain[0], chain[1]], ChainOrder(dictionary));
    }

    [Fact]
    public void Remove_EveryEntryOfAChain_EmptiesTheBasket()
    {
        ChainedBasketDictionary<FixedHashKey, string> dictionary = BuildChainOfThree();
        List<FixedHashKey> chain = ChainOrder(dictionary);

        // Middle first, then the old head, then what is left: every removal
        // after the first works on a chain the previous one relinked.
        Assert.True(dictionary.Remove(chain[1]));
        Assert.True(dictionary.Remove(chain[0]));
        Assert.True(dictionary.Remove(chain[2]));

        Assert.Equal(0, dictionary.Count);
        Assert.Empty(ChainOrder(dictionary));
        Assert.False(dictionary.Remove(chain[0]));
    }

    [Fact]
    public void Add_AfterRemovingTheHeadOfAChain_RebuildsTheChain()
    {
        ChainedBasketDictionary<FixedHashKey, string> dictionary = BuildChainOfThree();
        List<FixedHashKey> chain = ChainOrder(dictionary);
        Assert.True(dictionary.Remove(chain[0]));

        FixedHashKey fresh = new(42, "d");
        dictionary.Add(fresh, "d");

        Assert.Equal(3, dictionary.Count);
        AssertFindable(dictionary, fresh);
        AssertFindable(dictionary, chain[1]);
        AssertFindable(dictionary, chain[2]);
    }

    [Fact]
    public void Remove_UnknownKeyFromAPopulatedChain_ReturnsFalseAndKeepsEverything()
    {
        ChainedBasketDictionary<FixedHashKey, string> dictionary = BuildChainOfThree();

        // Same basket, no match: Remove walks to the end of the chain and
        // must leave it exactly as it was.
        Assert.False(dictionary.Remove(new FixedHashKey(42, "nobody")));

        Assert.Equal(3, dictionary.Count);

        foreach (FixedHashKey key in ChainOrder(dictionary))
        {
            AssertFindable(dictionary, key);
        }
    }

    /// <summary>
    /// Three keys with one hash code, so all three share a basket. The value
    /// stored under a key is its own name, which lets a test check both that
    /// an entry is still reachable and that it still carries the right value.
    /// </summary>
    private static ChainedBasketDictionary<FixedHashKey, string> BuildChainOfThree()
    {
        ChainedBasketDictionary<FixedHashKey, string> dictionary = new();
        dictionary.Add(new FixedHashKey(42, "a"), "a");
        dictionary.Add(new FixedHashKey(42, "b"), "b");
        dictionary.Add(new FixedHashKey(42, "c"), "c");
        return dictionary;
    }

    /// <summary>
    /// The keys of one basket in chain order. The enumeration walks the
    /// baskets in turn and each chain from its head, so with every key in the
    /// same basket this is the chain itself.
    /// </summary>
    private static List<FixedHashKey> ChainOrder(
        ChainedBasketDictionary<FixedHashKey, string> dictionary) =>
        dictionary.Select(entry => entry.Key).ToList();

    private static void AssertFindable(
        ChainedBasketDictionary<FixedHashKey, string> dictionary, FixedHashKey key)
    {
        Assert.True(dictionary.ContainsKey(key));
        Assert.Equal(key.ToString(), dictionary[key]);
    }
}
