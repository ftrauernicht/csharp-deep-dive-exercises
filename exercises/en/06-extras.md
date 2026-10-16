🇬🇧 English | 🇩🇪 [Deutsch](../de/06-zusatzaufgaben.md)

[← LINQ](05-linq.md) · [Learning material](LEARNING-MATERIAL.md)

# Extras

Seven of them, independent of each other. Each one requires exercise 3 or
exercise 4 and digs into exactly one thing.

## Z1: `IEqualityComparer<Animal>`

*Requires exercises 3 and 4.*

Write a `ByNameEqualityComparer` that treats two animals as equal when their
names match, ignoring case.

Use it with `new HashSet<Animal>(comparer)` and with `Distinct(comparer)`.

Give `BasketDictionary` a constructor that accepts an
`IEqualityComparer<TKey>`, and show in a test that the same key is found or
not found depending on the comparer.

The gain is bigger than it looks: `Animal.Equals` fixes **one** meaning of
equality, for good and for everyone. A comparer from outside allows a second
one without touching the class. Think about when you want which of the two.

## Z2: Changing a collection while enumerating it

*Requires exercise 4.*

`BasketDictionary` throws an `InvalidOperationException` when it is modified
during a `foreach`.

The usual way is a counter that is incremented on every structural change and
checked again while enumerating.

The real question is **what counts as a structural change**. Adding a key,
obviously. Removing one too. But overwriting a value that is already there?
The `Dictionary` from .NET allows it during enumeration. Decide, give your
reason, and write a test for both cases.

A hint from painful experience: if you walk your baskets with a `foreach` over
the inner `List<T>`, your counter no longer decides, the one in `List<T>`
does, and `List<T>` counts an indexer assignment as a change. Your dictionary
then behaves differently from what your own code claims.

## Z3: Linked list

*Requires exercise 4.*

Replace the `List<KeyValuePair<...>>` in the baskets with a singly linked list
of your own: a class `Entry` with `Key`, `Value` and `Next`.

All tests from exercise 4 have to stay green, unchanged.

The lesson sits in removal. A `List<T>` handles `RemoveAt` for you; in a
linked list the node **before** has to point at the node **after**, and the
first node of a basket has no predecessor. That is two cases, and testing
only one of them hides the other.

So put three keys with the same hash code into one basket on purpose and
remove the first one once, the middle one once, the last one once.

## Z4: Timing

*Requires exercise 4.*

Compare `BasketDictionary`, `Dictionary` and a `List<KeyValuePair<...>>` with
linear search at 100, 10,000 and 100,000 entries.

**Do not write a millisecond number into an assert.** It holds on your
machine, at this time of day, at this battery level. What holds everywhere is
the ratio, and something sturdier still: that it **grows** with the count.

Measure, print a table, then formulate an assert that holds on any machine.
Warm up once before measuring, otherwise you are measuring the JIT compiler.

At 100 entries you will barely see a difference. That is not a measurement
error. Where it stops being true is part of what you measure here.

## Z5: Records

*Requires exercise 3.*

A `record ChipNumber(string Value)` takes the place of the `string` in
`Animal`.

Test **which methods the compiler generates for a `record` on its own**, and
put the result next to what you wrote by hand in exercise 3: value equality,
hash code, `ToString`, `Deconstruct`, `==` and `!=`, plus `with`.

Then the uncomfortable question: if the compiler can do all of that, why did
you do exercise 3 at all? The answer has two parts, and the second one matters
more. A `record` builds equality over **all** fields, and in exercise 3 that
was exactly what should not hold.

> The sample solution leaves `Animal` with the `string` and puts the record
> beside it. The rebuild would change every signature from exercises 1 to 5.
> Do it on a branch if you want to see it whole.

## Z6: A second indexer

*Requires exercise 4.*

`Shelter` also gets `this[int kennelNumber]`. Every animal receives the next
free kennel when it is taken in; after an adoption the kennel becomes free
again.

Two indexers on one class are told apart by the type in the brackets:
`shelter["2760981..."]` is a chip number, `shelter[1]` is a kennel.

Decide what "the next free kennel" means when kennel 1 becomes free and kennel
2 is occupied. Does the counter keep counting, or is the gap filled? Both are
defensible, and the test records which one you decided on.

## Z7: Adoption requests

*Requires exercise 3.*

Requests are kept in a `SortedSet<AdoptionRequest>`, ordered by the date they
came in.

### Prediction

What happens with **two requests that carry the same date** when `CompareTo`
compares nothing but the date?

Predict it, then build it. And when you see the result: there is no error
message. `Add` has a return value that almost nobody looks at.

Correct the behavior. The correction is the same idea as in exercise 3.4: a
comparison that returns 0 only when two things are one and the same.

## What you learned here

- Determining equality from outside instead of writing it into the class
- What a structural change is and what is only a change of value
- Rewiring pointers, and why the first node is a special case
- Formulating a measurement that still holds on someone else's machine
- What a `record` gives you, and when that gift does not fit
- Two indexers, told apart by type
- A data structure that silently swallows entries when the comparison is too
  coarse
