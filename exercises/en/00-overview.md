🇬🇧 English | 🇩🇪 [Deutsch](../de/00-ueberblick.md)

# Overview

A shelter, five exercises and seven extra exercises. At the end you have a
class library in which a dictionary is built by hand, equality means what you
defined it to mean, and you have seen the moment at which LINQ actually
computes.

## Who this is for

You know classes, inheritance and interfaces, you have used generics, and you
have seen a test framework at least once. What you are missing is the layer
underneath, and these twelve exercises are that layer.

If you are still learning the basics, do not start here.

## How this works

**The text holds the specification, the test passes judgment.** There is no
sample output to compare against and no step-by-step guide. Every exercise
describes what should hold; whether it holds is answered by the test you
write.

This is not an end in itself. Writing a test forces you to read the exercise
to the end first, and that is where most solutions fail.

### Prediction exercises

Some sections are headed **Prediction**. An extra rule applies there, and it
is the core of the whole package:

> Write your expectation into the test as a comment **before** you run it.

```csharp
// Prediction: 2 -- both objects end up in the set, because ...
Assert.Equal(?, set.Count);
```

Only then run it. If the prediction was off, you have learned something; if
it was right, you now know it for certain instead of roughly. Write the test
after you know the answer and you have produced a line of code and nothing
else.

The comments stay, even the wrong ones. They are the record.

Some sections want a reason or a decision rather than a number. It goes in
the same place, as a comment above the test that pins it down. The only
section that asks for a file of its own is the summary in 5.3, and it says
so there.

### The analyzer will get in your way

The test project brings xUnit's analyzers with it, and they do not know the
prediction rule. As soon as the predicted number is 0 or 1, you get

```
warning xUnit2013: Do not use Assert.Equal() to check for collection size.
                   Use Assert.Single instead.
```

and for `Assert.True(...Contains(...))` xUnit2012 or xUnit2017 accordingly.
The suggestions are right for ordinary test code and wrong here:
`Assert.Single` deletes exactly the number the test is about, and
`Assert.Contains` compares with xUnit's comparer instead of the one you are
investigating.

The way out is a local variable:

```csharp
int count = set.Count;
// Prediction: 1 -- both objects are equal, so one of them is left
Assert.Equal(1, count);
```

The analyzer then sees no collection access in the assert, and your test code
still says what it checks. The same move helps everywhere you want to compare
two routes that are both called `Contains`.

## Project structure

```
AnimalShelter.slnx
├── src/AnimalShelter.Core/        class library
└── tests/AnimalShelter.Tests/     test project
```

| Namespace | Contents |
|---|---|
| `AnimalShelter.Core.Animals` | animal classes, interfaces, feeding plan |
| `AnimalShelter.Core.Clinic` | vet |
| `AnimalShelter.Core.Comparison` | comparer classes |
| `AnimalShelter.Core.Collections` | your own dictionary |
| `AnimalShelter.Core.Management` | class `Shelter` |
| `AnimalShelter.Core.Queries` | LINQ queries |
| `AnimalShelter.Core.Adoption` | adoption requests (extra exercise) |

The test project mirrors this structure: tests for
`AnimalShelter.Core.Animals` live in `AnimalShelter.Tests.Animals`, and so on.

The scaffold sits in the root directory: two projects that build, and nothing
else. No class is given to you. Which file belongs in which folder is in the
table above, and the folders are already there. That is where you work.

## Getting started

```
dotnet build
dotnet test
```

The first time, exactly one test runs and it checks nothing. It is there so you
can see that the chain from the project file to the console works before you
write your first test. After that it can go.

## The sample solution

`solution/` holds a complete version: implementation **and** tests, including
the filled-in predictions.

**Do not look there before you have attempted an exercise.** For an
implementation, a glance costs you the practice. For a prediction it costs you
the whole exercise, because knowing the answer is exactly what makes it
worthless.

The sample solution is built and tested in CI. Every number that sits in a
prediction test there is therefore a measured guarantee and not a claim: if
.NET changes its behavior, it shows up there.

## Order

The exercises build on each other and are worked through one after the other.
Exercise 3 changes `Animal`, exercise 4 uses `Animal` as a key, exercise 5
builds on all of it.

1. [The animal family](01-animal-family.md): inheritance, polymorphism, abstract methods
2. [Vet and interfaces](02-vet-and-interfaces.md): getting rid of type checks
3. [Equality, hash code and ordering](03-equality-and-ordering.md): `IEquatable`, `GetHashCode`, `IComparable`
4. [Your own dictionary](04-your-own-dictionary.md): baskets, collisions, growth
5. [LINQ](05-linq.md): queries and deferred execution
6. [Extras](06-extras.md): seven of them, Z1 to Z7

To catch up on the background: [Learning material](LEARNING-MATERIAL.md).
