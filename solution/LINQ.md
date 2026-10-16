🇬🇧 English | 🇩🇪 [Deutsch](LINQ.de.md)

# LINQ: immediate or deferred

The answer to [exercise 5.3](../exercises/en/05-linq.md#53-the-overview). Every
number here was measured rather than looked up, on .NET 10, with this counter
as the source. `LinqExecutionOverviewTests` in the test project measures them
again on every CI run:

```csharp
static IEnumerable<int> Source(List<string> log, int n)
{
    for (int i = 1; i <= n; i++)
    {
        log.Add($"pull {i}");
        yield return i;
    }
}
```

The source hands out five numbers and takes notes while it does. Afterwards the
log says how many of them were fetched.

## Immediate: computes when the line runs

| Method | pulled out of 5 |
|---|---|
| `ToList()` | 5 |
| `ToArray()` | 5 |
| `ToDictionary(...)` | 5 |
| `ToHashSet()` | 5 |
| `Count()` | 5 |
| `Sum()`, `Max()`, `Average()` | 5 |
| `Last()` | 5 |
| `Aggregate(...)` | 5 |
| `First()` | 1 |
| `Any()` | 1 |
| `Any(i => i == 3)` | 3 |
| `All(i => i < 2)` | 2 |
| `Contains(2)` | 2 |
| `ElementAt(1)` | 2 |

Immediate does not mean exhaustive. The lower half of the table runs just as
immediately but stops as soon as the answer is settled: `Any()` needs one
element, `All` gives up at the first counterexample.

One exception: `Single(i => i == 1)` pulls all five even though the matching
element was the first one. It has to prove there is no second
match, and that takes until the end.

## Deferred: only builds the query

| Method | after building | after the first `MoveNext()` |
|---|---|---|
| `Where(...)` | 0 | 1 |
| `Select(...)` | 0 | 1 |
| `OfType<T>()`, `Cast<T>()` | 0 | 1 |
| `Distinct()` | 0 | 1 |
| `Take(2)` | 0 | 1 |
| `Skip(1)` | 0 | **2** |
| `Except(...)` | 0 | 1 |
| `Concat(...)` | 0 | 1 |
| `SelectMany(...)` | 0 | 1 |
| `OrderBy(...)`, `OrderByDescending(...)` | 0 | **5** |
| `GroupBy(...)` | 0 | **5** |
| `Reverse()` | 0 | **5** |

The first column is 0 everywhere, and that is the whole point: building a query
does not touch the source.

`Skip` is the one row whose answer is not 1. It still has to fetch the element
it skips before it can hand out the next one.

The second column splits the group in two. The upper rows **stream**: one
element in, one element out. The lower rows **buffer**: sorting cannot name the
smallest element before it has seen all of them, and grouping and reversing are
in the same position. Deferred and streaming are two different promises, and
only the first one holds for all twelve rows.

## Two places where people guess wrong

**`Count()` runs the lambda.** Even when the source has known its own size all
along:

```csharp
List<int> list = [1, 2, 3];
int calls = 0;
IEnumerable<int> query = list.Select(i => { calls++; return i; });

int count = query.Count();   // 3
// calls == 3
```

`list.Count` would be 3 without a single call. The `Select` turned the source
from a list into a query, and a query knows its length only once it has run.
Put anything in that lambda that costs more than an assignment and you pay for
it on every `Count()`.

**Enumerating the same query twice means computing it twice.** An `IEnumerable`
is not a result sitting somewhere, it is an instruction that runs from the start
every time. Two `foreach` loops over the same variable are two runs.

## `ToList()`: needed here

In `DeferredExecutionTests.Kittens_MaterialisedBeforeTheForeach_AdoptsAllOfThem`:

```csharp
foreach (Animal kitten in report.Kittens().ToList())
{
    shelter.Adopt(kitten.ChipNumber);
}
```

Without `ToList()` this loop throws an `InvalidOperationException`, and it comes
out of the `BasketDictionary` three levels below the `foreach`. `Kittens()`
reads the shelter while `Adopt` changes it.

The position is the entire fix. `ToList()` sits **on the query**, not in the
loop body: it finishes reading the shelter and closes the enumerator before the
first adoption runs. Inside the body it would save nothing, because the loop's
own enumerator would still be open.

Measured, the shelter stood at two animals out of three when it failed. Not
untouched and not finished, but half changed. That is why this bug is so
unpleasant in real code.

## `ToList()`: pointless here

This is how it read in `ShelterReportTests` until exercise 5.3 exposed it:

```csharp
Assert.Equal(new[] { "Mimi", "Hoppel" }, report.HeaviestThree().Select(a => a.Name).ToList());
```

`Assert.Equal` compares two `IEnumerable` values itself, element by element. The
`ToList()` builds a list whose only purpose is to be read once and dropped. It
was there out of habit rather than for a reason, and it is gone now:

```csharp
Assert.Equal(new[] { "Mimi", "Hoppel" }, report.HeaviestThree().Select(a => a.Name));
```

A `ToList()` in such a place hides whether the query would have run at all, and
in exercise 5 that question is the difference between understanding and
guessing.

## And one that looks needed and is not

In `Veterinarian.VaccinateAllMissing`:

```csharp
IEnumerable<string> missing = vaccinatable.RequiredVaccinations
    .Except(vaccinatable.ReceivedVaccinations)
    .ToList();

foreach (string vaccination in missing)
{
    vaccinatable.Vaccinate(vaccination);   // changes ReceivedVaccinations
}
```

This looks like the case above: the loop changes a collection the query uses.
Reproduced, it does not throw even without the `ToList()`. `Except` reads its
**second** argument completely into a set before it yields the first element,
and after that no enumerator on `ReceivedVaccinations` is open any more.

The `ToList()` stays anyway, and the comment in the code says why: that
behaviour is a property of how `Except` is implemented today, not a promise LINQ
makes. A loop that changes a source should not depend on it.

The difference between this section and the one before it is why this file is
longer than a table. A pointless `ToList()` costs a list; a missing one costs a
half-finished operation.
