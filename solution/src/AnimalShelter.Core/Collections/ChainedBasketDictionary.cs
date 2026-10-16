using System.Collections;

namespace AnimalShelter.Core.Collections;

/// <summary>
/// Exercise Z3: the same dictionary, but the baskets are a singly linked list
/// written by hand instead of a List&lt;T&gt;.
///
/// It sits beside BasketDictionary rather than replacing it, so that the two
/// can be read side by side. The behaviour is identical and the tests are the
/// same; what changes is that Add now walks a chain of Entry objects and
/// Remove has to relink the one in front of the entry it drops. That relinking
/// is the part a List hides from you.
/// </summary>
public class ChainedBasketDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private const double MaxLoadFactor = 0.75;

    private readonly IEqualityComparer<TKey> comparer;

    private Entry?[] baskets;

    private int version;

    public ChainedBasketDictionary(int initialCapacity = 16)
        : this(initialCapacity, EqualityComparer<TKey>.Default)
    {
    }

    public ChainedBasketDictionary(int initialCapacity, IEqualityComparer<TKey> comparer)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }

        this.comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        baskets = new Entry?[initialCapacity];
    }

    public int Count { get; private set; }

    internal int BasketCount => baskets.Length;

    public void Add(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (Find(key) is not null)
        {
            throw new ArgumentException($"The key '{key}' is already present.", nameof(key));
        }

        int index = BasketIndex(key, baskets.Length);
        baskets[index] = new Entry(key, value) { Next = baskets[index] };
        Count++;
        version++;
        GrowIfNeeded();
    }

    public bool ContainsKey(TKey key) => Find(key) is not null;

    public bool TryGetValue(TKey key, out TValue value)
    {
        Entry? entry = Find(key);

        if (entry is null)
        {
            value = default!;
            return false;
        }

        value = entry.Value;
        return true;
    }

    /// <summary>
    /// Dropping an entry from a chain means the one before it has to point
    /// past it. The first entry in a basket has no predecessor, which is why
    /// there are two cases here and only one in the List version.
    /// </summary>
    public bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        int index = BasketIndex(key, baskets.Length);
        Entry? previous = null;
        Entry? current = baskets[index];

        while (current is not null)
        {
            if (comparer.Equals(current.Key, key))
            {
                if (previous is null)
                {
                    baskets[index] = current.Next;
                }
                else
                {
                    previous.Next = current.Next;
                }

                Count--;
                version++;
                return true;
            }

            previous = current;
            current = current.Next;
        }

        return false;
    }

    public TValue this[TKey key]
    {
        get
        {
            Entry? entry = Find(key);

            if (entry is null)
            {
                throw new KeyNotFoundException($"The key '{key}' was not found.");
            }

            return entry.Value;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(key);
            Entry? entry = Find(key);

            if (entry is not null)
            {
                entry.Value = value;
                return;
            }

            int index = BasketIndex(key, baskets.Length);
            baskets[index] = new Entry(key, value) { Next = baskets[index] };
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

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        int startVersion = version;

        foreach (Entry? head in baskets)
        {
            Entry? current = head;

            while (current is not null)
            {
                if (version != startVersion)
                {
                    throw new InvalidOperationException(
                        "The dictionary was modified while it was being enumerated.");
                }

                yield return new KeyValuePair<TKey, TValue>(current.Key, current.Value);
                current = current.Next;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private Entry? Find(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        Entry? current = baskets[BasketIndex(key, baskets.Length)];

        while (current is not null)
        {
            if (comparer.Equals(current.Key, key))
            {
                return current;
            }

            current = current.Next;
        }

        return null;
    }

    private int BasketIndex(TKey key, int basketCount) =>
        (comparer.GetHashCode(key) & 0x7FFFFFFF) % basketCount;

    private void GrowIfNeeded()
    {
        if ((double)Count / baskets.Length <= MaxLoadFactor)
        {
            return;
        }

        var bigger = new Entry?[baskets.Length * 2];

        foreach (Entry? head in baskets)
        {
            Entry? current = head;

            while (current is not null)
            {
                Entry? next = current.Next;
                int index = BasketIndex(current.Key, bigger.Length);
                current.Next = bigger[index];
                bigger[index] = current;
                current = next;
            }
        }

        baskets = bigger;
    }

    private sealed class Entry
    {
        public Entry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; }

        public TValue Value { get; set; }

        public Entry? Next { get; set; }
    }
}
