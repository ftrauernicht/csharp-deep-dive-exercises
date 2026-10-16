using System.Collections;

namespace AnimalShelter.Core.Collections;

/// <summary>
/// A dictionary built from nothing but arrays and lists, to show what the one
/// in the framework does behind the indexer.
///
/// Keys are spread over baskets. The basket follows from the key's hash code;
/// inside a basket, keys are compared with Equals. That is the whole idea, and
/// everything else here is bookkeeping around it.
/// </summary>
public class BasketDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private const double MaxLoadFactor = 0.75;

    private readonly IEqualityComparer<TKey> comparer;

    private List<KeyValuePair<TKey, TValue>>[] baskets;

    /// <summary>
    /// Counts structural changes so that an enumeration can notice one. The
    /// framework dictionary does the same thing, and exercise Z2 is about why.
    /// </summary>
    private int version;

    public BasketDictionary(int initialCapacity = 16)
        : this(initialCapacity, EqualityComparer<TKey>.Default)
    {
    }

    /// <summary>Exercise Z1: let the caller decide what "the same key" means.</summary>
    public BasketDictionary(IEqualityComparer<TKey> comparer)
        : this(16, comparer)
    {
    }

    public BasketDictionary(int initialCapacity, IEqualityComparer<TKey> comparer)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity),
                "Capacity must be greater than 0.");
        }

        this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        baskets = CreateBaskets(initialCapacity);
    }

    public int Count { get; private set; }

    /// <summary>Visible to the tests only, for exercise 4.3.</summary>
    internal int BasketCount => baskets.Length;

    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        List<KeyValuePair<TKey, TValue>> basket = BasketFor(key);

        if (IndexInBasket(basket, key) >= 0)
        {
            throw new ArgumentException($"The key '{key}' is already present.", nameof(key));
        }

        basket.Add(new KeyValuePair<TKey, TValue>(key, value));
        Count++;
        version++;
        GrowIfNeeded();
    }

    public bool ContainsKey(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return IndexInBasket(BasketFor(key), key) >= 0;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        List<KeyValuePair<TKey, TValue>> basket = BasketFor(key);
        int index = IndexInBasket(basket, key);

        if (index < 0)
        {
            value = default!;
            return false;
        }

        value = basket[index].Value;
        return true;
    }

    public bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        List<KeyValuePair<TKey, TValue>> basket = BasketFor(key);
        int index = IndexInBasket(basket, key);

        if (index < 0)
        {
            return false;
        }

        basket.RemoveAt(index);
        Count--;
        version++;
        return true;
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue value))
            {
                return value;
            }

            throw new KeyNotFoundException($"The key '{key}' was not found.");
        }

        set
        {
            ArgumentNullException.ThrowIfNull(key);

            List<KeyValuePair<TKey, TValue>> basket = BasketFor(key);
            int index = IndexInBasket(basket, key);

            if (index >= 0)
            {
                // Overwriting a value is not a structural change: an
                // enumeration in progress still sees the same keys in the same
                // baskets, so the version stays put.
                basket[index] = new KeyValuePair<TKey, TValue>(key, value);
                return;
            }

            basket.Add(new KeyValuePair<TKey, TValue>(key, value));
            Count++;
            version++;
            GrowIfNeeded();
        }
    }

    public IEnumerable<TKey> Keys
    {
        get
        {
            foreach (KeyValuePair<TKey, TValue> entry in this)
            {
                yield return entry.Key;
            }
        }
    }

    public IEnumerable<TValue> Values
    {
        get
        {
            foreach (KeyValuePair<TKey, TValue> entry in this)
            {
                yield return entry.Value;
            }
        }
    }

    /// <summary>
    /// Walks the baskets by index rather than with foreach, and that is not a
    /// stylistic preference. A foreach over the inner List would take List's
    /// own enumerator, and List counts an indexer assignment as a change --
    /// so overwriting a value during enumeration would throw, even though
    /// nothing moved between baskets. The framework dictionary allows that,
    /// and this one should behave the same. Only the version counter here
    /// decides what counts as a structural change.
    /// </summary>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        int startVersion = version;

        for (int b = 0; b < baskets.Length; b++)
        {
            List<KeyValuePair<TKey, TValue>> basket = baskets[b];

            for (int i = 0; i < basket.Count; i++)
            {
                if (version != startVersion)
                {
                    throw new InvalidOperationException(
                        "The dictionary was modified while it was being enumerated.");
                }

                yield return basket[i];
            }
        }

        if (version != startVersion)
        {
            throw new InvalidOperationException(
                "The dictionary was modified while it was being enumerated.");
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // --- The basket arithmetic ---

    private List<KeyValuePair<TKey, TValue>> BasketFor(TKey key) =>
        baskets[BasketIndex(key, baskets.Length)];

    private int BasketIndex(TKey key, int basketCount)
    {
        int hash = comparer.GetHashCode(key);

        // Masking the sign bit rather than Math.Abs, and that is not a style
        // choice: a hash code may be int.MinValue, and Math.Abs(int.MinValue)
        // throws because the positive counterpart does not exist in an int.
        // Exercise 4.2 asks for exactly that key.
        return (hash & 0x7FFFFFFF) % basketCount;
    }

    private int IndexInBasket(List<KeyValuePair<TKey, TValue>> basket, TKey key)
    {
        for (int i = 0; i < basket.Count; i++)
        {
            if (comparer.Equals(basket[i].Key, key))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Doubles the number of baskets once they are three-quarters full and
    /// redistributes every entry. Without this the baskets grow into long
    /// lists and every lookup walks one of them, which is the difference
    /// between a dictionary and an array you search through.
    /// </summary>
    private void GrowIfNeeded()
    {
        if ((double)Count / baskets.Length <= MaxLoadFactor)
        {
            return;
        }

        List<KeyValuePair<TKey, TValue>>[] bigger = CreateBaskets(baskets.Length * 2);

        foreach (List<KeyValuePair<TKey, TValue>> basket in baskets)
        {
            foreach (KeyValuePair<TKey, TValue> entry in basket)
            {
                bigger[BasketIndex(entry.Key, bigger.Length)].Add(entry);
            }
        }

        baskets = bigger;
    }

    private static List<KeyValuePair<TKey, TValue>>[] CreateBaskets(int count)
    {
        var created = new List<KeyValuePair<TKey, TValue>>[count];

        for (int i = 0; i < count; i++)
        {
            created[i] = [];
        }

        return created;
    }
}
