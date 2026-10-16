🇬🇧 English | 🇩🇪 [Deutsch](../de/03-gleichheit-und-sortierung.md)

[← Vet and interfaces](02-vet-and-interfaces.md) · Next: [Your own dictionary](04-your-own-dictionary.md)

# Exercise 3: Equality, hash code and ordering

**Goal:** decide when two animals are the same animal, then see what
`HashSet` and `Dictionary` do with that decision when you only do half
the work.

## 3.1 Prediction: the starting state

Before anything is implemented:

```csharp
Cat a = new Cat("Mimi", "276098100000001", 36, 4.0);
Cat b = new Cat("Mimi", "276098100000001", 36, 4.0);

bool sameByOperator = a == b;
bool sameByEquals   = a.Equals(b);

HashSet<Animal> set = new() { a, b };
int count = set.Count;
```

Write the test **now**, with your three predictions as a comment, and run
it. Adjust it once 3.2 is in place, and leave the old comments standing.

## 3.2 `IEquatable<Animal>`

**Two animals are equal when their `ChipNumber` is equal.** The name does
not matter.

Implement in `Animal`:

- `IEquatable<Animal>` with `Equals(Animal? other)`
- `override Equals(object? obj)`
- `override GetHashCode()`
- the `==` and `!=` operators, which handle `null` on both sides as well

**One decision is yours:** are a `Cat` and a `Dog` with the same chip
number equal? Both answers can be defended. Write down the one you pick,
justify it in one sentence in the code, and implement it.

Tests:

- reflexive, symmetric, transitive
- `a.Equals(null)` is `false`
- `null == null`, `a == null`, `null == a`
- equal objects have equal hash codes
- same chip number, different name → equal
- same name, different chip number → not equal

## 3.3 Prediction: three hash experiments

One test per experiment. Afterwards put the code back into its correct
state.

**A: comment out `GetHashCode()`.**
How many elements does a `HashSet<Animal>` hold after two animals with the
same chip number have been inserted? And does `ContainsKey` on a
`Dictionary<Animal, string>` find an animal when the search runs with a
**different** object that carries the same chip number?

Three lines that look like they do the same thing. Predict what each one
returns before you run it:

```csharp
dictionary.ContainsKey(probe)
dictionary.Keys.Contains(probe)
dictionary.Keys.Any(key => key.Equals(probe))
```

Two of them return the same thing, and it is not what reading them suggests.
The third returns something else. Only one of the three lines compares with
`Equals`; the other two compute a hash code first and then look into a single
basket. Which is which becomes
clear once you check what type `Keys` actually has.

Before anything runs at all, the compiler tells you, twice:

```
warning CS0659: 'Animal' overrides Object.Equals(object o) but does not override Object.GetHashCode()
warning CS0661: 'Animal' defines operator == or operator != but does not override Object.GetHashCode()
```

The second one comes from the operators you wrote in 3.2. Leave both
standing for this experiment and remember their wording.

And now the part that gets uncomfortable: **on the next build they are
gone.** MSBuild considers the project up to date, never calls the compiler,
and therefore does not replay the warnings either. Scroll past them once and
you will not see them again as long as the file stays untouched. You
can bring them back with `dotnet build --no-incremental`. That is the most
honest answer to the question of how this bug gets into real code: .NET
reports it, once.

**B: `GetHashCode()` always returns `1`.**
Do `HashSet` and `Dictionary` still work correctly? Then insert 20 000
animals into a `HashSet<Animal>` and measure the time with `Stopwatch`,
once with `return 1;` and once with the correct implementation.

The measurement is a test like any other. To see the numbers you need
`ITestOutputHelper` in the constructor of the test class and
`dotnet test --logger "console;verbosity=detailed"`; the test runner
swallows `Console.WriteLine`. And the 20 000 animals need 20 000 valid chip
numbers, so fifteen digits each.

Measure before you guess. A millisecond figure as the expectation holds on
your machine and nowhere else; assert the ratio instead.

For this experiment the compiler says nothing at all. A hash code that
always returns 1 fulfills the contract; it is only useless. Warnings catch
bugs, not bad decisions.

**C: `ChipNumber` temporarily gets a public setter.**
An animal is inserted into a `HashSet`, then its chip number is changed.
What does `set.Contains(animal)` return, with that same object?

The result is the reason a key should be immutable.

This experiment trips you up when you put the code back: once the setter is
gone again, **the test no longer compiles**, and until you resolve that, no
test runs at all. Decide what becomes of it. One option that keeps the
lesson alive: prediction and measurement stay as a comment, and from then on
the test checks that the setter does not exist.

> **Note on the sample solution:** there, `Animal` cannot be broken,
> because the CI checks the opposite. The three faulty states therefore sit
> in the test project as small stand-in classes, each wrong in exactly one
> way. For you, the route through `Animal` is the right one: you see the
> fault on the class you have been working with all along.

## 3.4 `IComparable<Animal>`

You implement this in `Animal` as well.

- ordering by `Name`, case-insensitive
- by `ChipNumber` when the names are equal
- `CompareTo(null)` returns a positive value

Tests:

- `List<Animal>.Sort()` with mixed animals
- two animals with the same name
- `a.CompareTo(b)` and `b.CompareTo(a)` have opposite signs
- **`a.CompareTo(b) == 0` exactly when `a.Equals(b)`**

### The contradiction you are about to find

The last requirement does not fit what stands above it, and that is
intended.

Take two animals with **the same chip number and different names**. By 3.2
they are equal. By the ordering rule above, `CompareTo` compares the names
first, and those are different, so something other than 0 comes out. The
requirement cannot be met this way.

Solve it before you read on. The solution is one line, and it sits at the
very start of `CompareTo`.

This is not a flaw in the exercise, it is the exercise. Two comparison
rules that look sensible on their own contradict each other as soon as you
put them side by side, and you notice that while you write the test, not
while you read. That is exactly why the test is in the list.

### What this solution costs

The one line meets the requirement and breaks another one that nobody wrote
down. Take three animals:

| | Name | Chip number |
|---|---|---|
| a | Anna | ...001 |
| b | Zoe | ...001 |
| c | Mia | ...002 |

a and b carry the same chip number, so they are equal and `a.CompareTo(b)`
returns 0. Anna comes before Mia, so a is less than c. Mia comes before Zoe,
so c is less than b. That makes a equal to b and at the same time less than
something that is less than b. This is no longer an order.

Measured, `List<Animal>.Sort()` does not complain about it. The same three
animals come out differently depending on the input order: `[a, b, c]`
becomes Anna, Mia, Zoe, and `[b, a, c]` becomes Mia, Anna, Zoe. The second
result is not even sorted by name.

At scale it stays that way. Take a few thousand animals where every chip
number appears twice, shuffle them repeatedly and sort them again every time.
Almost every new input order yields a new result. How many come out exactly
depends on the names and the shuffle; that it is more than one depends only on
the comparison. The bug never reports itself, it only takes effect.

The pair that triggers the contradiction is two animals with **the same chip
number and different names**. A chip number is unique, so that pair cannot
exist in the first place. You have three options, and all three are
defensible:

- ask `Equals` first and write the assumption next to it, that chip numbers
  are unique
- throw the names out of `CompareTo` and sort by chip number alone: a clean
  order, but an ordering no human wants to read
- stay with the name and drop the requirement from the test list; then only
  equal animals returning 0 holds, not the other direction

The sample solution takes the first and puts the assumption in a comment.
Which one you take matters less than writing down what you gave up for it.
Write a test for it too. It keeps the decision on record and forces the next
person to make it again rather than discover it.

Optional: the operators `<`, `>`, `<=`, `>=`.

## 3.5 `IComparer<Animal>`

In `AnimalShelter.Core.Comparison`:

- `ByAgeComparer`: youngest first
- `ByDailyFoodComparer`: highest food requirement first

Test both with `list.Sort(new ByAgeComparer())`.

While you do, think about why these classes exist at all when `Animal`
is already `IComparable`. A class has **one** natural order; every further
one needs a comparer of its own that comes from outside.

## What you learned here

- Equality as a decision, not as a given
- Why `Equals` without `GetHashCode` goes unnoticed until a `HashSet`
  comes into play
- That a constant hash code is correct and still useless
- Why a key has to be immutable
- That `CompareTo` and `Equals` have to agree, and what happens when two
  rules get in each other's way
- That a solution can have a price that never shows up as an error, only as
  a result that depends on the order the input arrived in
- The difference between the one natural order and any number of further
  ones
