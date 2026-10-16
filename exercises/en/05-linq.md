🇬🇧 English | 🇩🇪 [Deutsch](../de/05-linq.md)

[← Your own dictionary](04-your-own-dictionary.md) · Next: [Extras](06-extras.md)

# Exercise 5: LINQ

**Goal:** write nine queries, then run seven experiments, after which you
never again guess when a LINQ query actually computes anything.

## 5.1 Queries

In `AnimalShelter.Core.Queries` a class `ShelterReport` that receives a
`Shelter` in its constructor. All methods in **method syntax**.

| Method | Returns | Contents |
|---|---|---|
| `Cats()` | `IEnumerable<Cat>` | all cats |
| `Kittens()` | `IEnumerable<Cat>` | cats under 12 months |
| `NamesSorted()` | `IEnumerable<string>` | sort the animals by the ordering from 3.4 first, then take the names |
| `TotalDailyFoodInKg()` | `double` | the sum |
| `CountBySpecies()` | `Dictionary<string, int>` | something like `{"Cat": 5, "Dog": 2}`; there is no `Species` property, the type name is the key |
| `NotFullyVaccinated()` | `IEnumerable<Animal>` | vaccinable animals with a missing vaccination |
| `Oldest()` | `Animal?` | `null` for an empty shelter |
| `HeaviestThree()` | `IEnumerable<Animal>` | the three heaviest, and fewer than three when the shelter holds fewer |
| `HasOutdoorCats()` | `bool` | `true` as soon as one cat is an outdoor cat |

Tests for every method, **each of them also with an empty shelter**: one test
per method, checking the neutral result, so an empty sequence, `0.0`, an empty
dictionary, `false`, `null`. Plus one test that runs a LINQ query directly on
a `BasketDictionary`.

`ShelterReport` holds the shelter itself, not a copy of it. That is not a side
detail: experiment 4 in 5.2 hangs on it.

Look at the return types while you write. Some methods hand back a result,
others hand back a query, and the rest of this exercise lives between those
two things.

A trap waits at `Oldest()`. Since exercise 3, `Animal` is an
`IComparable<Animal>`, so `shelter.Max()` compiles without complaint. It does
not return the oldest animal but the last one in the ordering from 3.4, so by
name. Check what your line actually compares.

### A trap when testing

The order in which `Shelter` hands out its animals is **not fixed**. It
follows the basket distribution of your `BasketDictionary`, which follows the
hash code of the chip number, and in .NET `string.GetHashCode()` returns
different values on every process start.

A test like

```csharp
Assert.Equal(["Mimi", "Rex"], report.Cats().Select(c => c.Name));
```

therefore passes today and fails tomorrow. Sort before comparing, or check the
contents without the order.

For `NamesSorted()` the order is part of the promise and may be checked: the
ordering from 3.4 falls back to the chip number when the names are equal, and
that one is unique.

For `HeaviestThree()` that holds **only as long as no two animals weigh the
same**. `OrderByDescending` is stable, so at equal weight it keeps the order
of the source, and the source is exactly the undetermined basket order from
above. With four animals of the same weight it is then not only the order that
changes but the selection: a different three from run to run. Pick different
weights in your test data, or check something that does not hang on the
source.

## 5.2 Prediction: deferred execution

A test class of its own, `DeferredExecutionTests`, in
`AnimalShelter.Tests.Queries`. Seven experiments, one test for each, and above
every assert your prediction as a comment. At the end all tests are green and
check the **actual** behavior.

### 1. Changing the source after the query

```csharp
List<Animal> animals = new() { cat1, dog1 };
IEnumerable<Animal> cats = animals.Where(a => a is Cat);
animals.Add(cat2);
// cats.Count() == ?
```

The same thing again with `.Where(...).ToList()` right after the query.

### 2. How often the lambda runs

```csharp
int calls = 0;
IEnumerable<string> names = animals.Select(a => { calls++; return a.Name; });
// calls == ?
int count = names.Count();
// calls == ?
string first = names.First();
// calls == ?
foreach (string n in names) { }
// calls == ?
```

Four predictions, four asserts. The counter is cumulative; it is not reset
between the steps.

The second one is where opinions split: the source is a `List<Animal>` whose
count has been known all along.

### 3. When the exception happens

A query with `Select` whose lambda throws an exception for one particular
animal.

`Assert.Throws` wraps **exactly the line in which the exception actually
occurs**, not the one in which the query is built. Show both: that building
does nothing, and where it blows up.

With a `foreach` that place is not a line in the body but the loop header
that fetches the next element. The whole loop therefore goes inside the
lambda.

### 4. Adopting inside the loop

```csharp
foreach (Animal kitten in report.Kittens())
{
    shelter.Adopt(kitten.ChipNumber);
}
```

What happens? Predict it, then run it.

Afterwards check two things that go beyond the error message. **Where** does
the exception come from, which class throws it? And what state is the shelter
in after that?

The message itself does not give the class away.
`ex.TargetSite?.DeclaringType` does, and what stands there is a surprise of
its own.

Fix it with `ToList()` and justify the place you chose. One place saves
nothing at all, namely the loop body. More than one place carries. Say what
the carrying ones have in common.

### 5. The order elements are pulled in

```csharp
private static IEnumerable<Animal> WithLog(IEnumerable<Animal> source, List<string> log)
{
    foreach (Animal animal in source)
    {
        log.Add($"yield {animal.Name}");
        yield return animal;
    }
}
```

The method hands out its elements with `yield return`. This is not a loop that
runs through and returns a list at the end: every `yield return` stops in the
middle of the method and gives control back to the caller, and for the next
element it carries on at exactly that point. What stands in the log therefore
shows how far someone pulled the query.

How many entries are in the log after

- `WithLog(animals, log).Where(...).First()`
- `WithLog(animals, log).OrderBy(a => a.Name).First()`
- `WithLog(animals, log).Take(2).ToList()`

Use at least four animals for this, and do **not** put the one you are looking
for first. With it in front, all three lines pull one element and the
comparison shows nothing.

The second line is the one that explains the most.

And then the question that lays the whole mechanism open. With
`WithLog(...).Select(...).Where(...)`, does **every** element go through
`Select` first and then each one through `Where`, or does each element travel
the whole chain on its own? Check it by having `Select` and `Where` write to
the log as well, then compare the order of the entries.

### 6. A captured variable

```csharp
int minAge = 12;
IEnumerable<Animal> old = animals.Where(a => a.AgeInMonths >= minAge);
minAge = 100;
// old.Count() == ?
```

### 7. `Distinct`

```csharp
Cat original = new Cat("Mimi",  "276098100000001", 36, 4.0);
Cat renamed  = new Cat("Minka", "276098100000001", 36, 4.0);
List<Animal> list = new() { original, renamed };
// list.Distinct().Count() == ?
```

And with `GetHashCode()` commented out? The answer hangs on exercise 3, and
this is where it shows what all that work there was good for.

The same problem as in 3.3: you cannot leave `Animal` broken, because in the
end all tests should be green. So measure first with `GetHashCode()` commented
out, restore `Animal`, and then decide how the measurement survives. A small
stand-in class in the test project that overrides `Equals` and not
`GetHashCode` keeps the result on record for good; it then needs a
`#pragma warning disable CS0659` and a comment on why the warning is exactly
right here.

## 5.3 The overview

Put a file `LINQ.md` next to your solution file, so in the root of your
working copy. Which language you write it in is up to you; it is for you.
Contents:

- a table: at least six LINQ methods with **immediate** execution and six with
  **deferred** execution
- one example each from your own code where `ToList()` is necessary and where
  it is superfluous

The second point is where it gets hard. A superfluous `ToList()` is not a
question of style: it materializes a list nobody needs, and in doing so it
hides the fact that the query would not have run at all.

## What you learned here

- The difference between a query and a result, readable from the return type
- That a query reads its source only when enumerated, and what that means when
  the source changes in between
- How often a lambda runs, measured instead of assumed
- That deferred does not mean streaming
- Why changing a collection while enumerating it blows up three levels deeper
  than where you are looking
- Where `ToList()` belongs, and where it only looks like it does
- That a captured variable is the variable and not its value
- That a promise about order carries only as far as the comparison that
  produces it
