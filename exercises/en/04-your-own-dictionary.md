🇬🇧 English | 🇩🇪 [Deutsch](../de/04-eigenes-dictionary.md)

[← Equality and ordering](03-equality-and-ordering.md) · Next: [LINQ](05-linq.md)

# Exercise 4: Your own dictionary

**Goal:** build the class you have been relying on for years. Afterwards
you know what happens between `dictionary[key]` and the answer.

## 4.1 `BasketDictionary<TKey, TValue>`

In `AnimalShelter.Core.Collections`. **Internally only arrays and `List<T>`
are allowed**: no `Dictionary`, no `HashSet`, and no inheriting from those
classes.

The entries are distributed over baskets. The basket follows from the hash
code of the key modulo the number of baskets. Inside a basket, comparison
runs through `Equals`.

```csharp
using System.Collections;   // for the non-generic IEnumerator, see below

public class BasketDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private List<KeyValuePair<TKey, TValue>>[] baskets;

    public BasketDictionary(int initialCapacity = 16) { ... }

    public int Count { get; private set; }

    public void Add(TKey key, TValue value) { ... }
    public bool ContainsKey(TKey key) { ... }
    public bool TryGetValue(TKey key, out TValue value) { ... }
    public bool Remove(TKey key) { ... }

    public TValue this[TKey key]
    {
        get { ... }
        set { ... }
    }

    public IEnumerable<TKey> Keys { get { ... } }
    public IEnumerable<TValue> Values { get { ... } }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { ... }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Three places deserve a sentence, because they go beyond what you have needed
so far. The first two are in the scaffold, the third is in the list below it:

- `where TKey : notnull` asks the caller for a key type that allows no `null`,
  at compile time. That is a warning and not a guarantee: through an `object`
  detour, or from code without nullable analysis, `null` still arrives. Which
  is why the list below also carries the `ArgumentNullException`.
- The enumeration appears twice. `IEnumerator IEnumerable.GetEnumerator()` is
  the old, untyped version from the time before generics. It is implemented
  **explicitly**, so without `public` and with the interface in front of the
  method name, so that it does not show up in the public surface and nobody
  accidentally gets back `object` instead of `KeyValuePair`. That is what you
  need `using System.Collections;` for.
- `EqualityComparer<TKey>.Default` instead of `key.Equals(other)`: it copes
  with `null` on both sides, finds `IEquatable<T>` without boxing, and in Z1
  you swap out this one object to decide equality from outside.

One spot will annoy you. `TryGetValue` has to write something into `value` on
the failure path, and with nullable analysis on, `value = default;` is a
`warning CS8601: Possible null reference assignment`. The usual remedy is
`value = default!;`, where the exclamation mark tells the compiler you know
better. Here you do know better: if you get `false` back, you do not read
`value`.

Behavior as in .NET's `Dictionary<TKey, TValue>`:

- `Add` with an existing key → `ArgumentException`
- indexer get with an unknown key → `KeyNotFoundException`
- indexer set adds or overwrites; on an overwrite `Count` stays the same,
  because no entry was added
- `Remove` returns `true` or `false`
- a `null` key → `ArgumentNullException`, an `initialCapacity` of 0 or less →
  `ArgumentOutOfRangeException`
- `Keys` and `Values` are a **view**, not a copy: whatever is inserted after
  you fetch the property shows up when you enumerate. Both hand back the
  entries in the same order, position for position
- key comparison through `EqualityComparer<TKey>.Default`

What is **not** asked for here: noticing that the dictionary has changed
during an enumeration. That is extra exercise Z2, and there the real content
is the question of what counts as a change in the first place.

**Work test-driven: the test first, then the implementation.** It pays off
in this exercise, because each of the methods is small on its own and they
only amount to something together.

## 4.2 Prediction: the special cases

Put a helper class `FixedHashKey` in the test project whose `GetHashCode()`
returns a value handed in through the constructor. That lets you order
collisions instead of waiting for them.

For that it needs two things: the hash code from the constructor and an
identity of its own that `Equals` decides on. Without the second one only
reference equality is left, and then the third point below cannot be checked
at all: three different keys with the same hash code are exactly the case in
which your dictionary has to ask `Equals` inside a basket.

Check:

- a key with hash code **-7**
- a key with hash code **`int.MinValue`**
- three different keys with the **same** hash code: find every one of them
  separately, remove every one of them separately
- an `Animal` as the key, looked up with a different object that has the
  same chip number

The second case is the interesting one. Write down your prediction of what
happens with `int.MinValue` before you let it run.

And if you were about to write `Math.Abs(hash) % baskets.Length`: that is
the obvious route, and it throws an `OverflowException`.
`Math.Abs(int.MinValue)` has no result that fits into an `int`; the positive
counterpart does not exist. So how do you get a non-negative index instead?

A negative remainder is a problem of the same kind: `-7 % 16` is `-7` in
C#, and an array does not like that. There is more than one way to turn that
into a valid index, and they do not all land in the same basket. That is
fine as long as the index is in range and the same key always hits the same
basket.

## 4.3 Growing

When the ratio of entries to baskets exceeds the value **0.75**, the number
of baskets is doubled and all entries are redistributed. The check happens
after an insert, and after every insert: through `Add` as well as through the
indexer.

Do not write that ratio as `Count / baskets.Length`. Both are `int`, so C#
divides as integers, and 13 entries in 16 baskets come out as 0 instead of
0.8125. Your dictionary would never grow, and the only place it would show
is the timing.

Tests:

- insert 1 000 entries, all of them are found again
- the number of baskets grows

For the second test you need a view from outside. Make an `internal`
property `BasketCount` visible through

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnimalShelter.Tests")]
```

The attribute has to sit before any namespace declaration. The usual place
is a file `AssemblyInfo.cs` of its own in the `AnimalShelter.Core` project;
the head of `BasketDictionary.cs` does the job too.

That is the honest route: the property stays out of the public contract,
and the tests reach it anyway.

Think about why redistributing is **necessary** and why enlarging the array
is not enough. The basket index hangs on the number of baskets; after the
doubling, almost every key sits somewhere else.

## 4.4 The `Shelter` class

In `AnimalShelter.Core.Management`. The animals are stored internally in
**your** `BasketDictionary<string, Animal>`, and the key is the chip
number.

```csharp
public class Shelter : IEnumerable<Animal>
{
    public Shelter(string name, int capacity) { ... }

    public string Name { get; }
    public int Capacity { get; }
    public int AnimalCount { get; }      // counts what the dictionary holds

    public void Admit(Animal animal) { ... }
    public Animal Adopt(string chipNumber) { ... }
    public Animal this[string chipNumber] { get { ... } }

    public IEnumerator<Animal> GetEnumerator() { ... }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

- `Admit` with a chip number that is already present →
  `InvalidOperationException`
- `Admit` on a full shelter → `InvalidOperationException`
- `Adopt` removes the animal and returns it; unknown chip number →
  `KeyNotFoundException`
- `shelter["276098100000001"]` returns the animal; an unknown chip number →
  `KeyNotFoundException`, because the return type is `Animal` and not
  `Animal?`

From here on the class from 4.1 carries real load.

## What you learned here

- How a dictionary finds a key, and why that works in constant time
- Collisions as the normal case, not as the exception
- Why `Math.Abs` is the wrong choice for a hash code
- That the remainder of a negative number stays negative in C#
- That two `int` divided give an `int` again, even with a `double` on the
  left
- Why growing is a redistribution
- `InternalsVisibleTo`, to test what should not be public
- A data structure you built yourself, underneath a class that uses it
