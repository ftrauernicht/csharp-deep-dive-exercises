🇬🇧 English | 🇩🇪 [Deutsch](README.de.md)

# C# Deep Dive Exercises

Twelve exercises around an animal shelter, for people who can already write C#.
The language is not the subject here. The layer underneath it is: why `Equals`
without `GetHashCode` is a bug, what a `Dictionary` does while it looks for a
key, and when a LINQ query actually computes anything.

What you end up with is a class library containing a dictionary you built
yourself, an equality that means what you decided it means, and a set of tests
that record what you found out about .NET.

## Who this is for

You can write classes, you have used inheritance, interfaces and generics, and
you have seen a test framework at least once. If you are starting from
scratch, start with
[csharp-oop-course](https://github.com/ftrauernicht/csharp-oop-course)
instead; this package picks up where that course stops.

## The rule everything rests on

Some sections are headed **Prediction**:

> Write what you expect into the test as a comment **before** you run it.

The comments stay in the file, the wrong ones included. They are the record.
Why that order decides whether an exercise is worth anything is spelled out in
the [overview](exercises/en/00-overview.md).

## What is here

| Place | Contents |
|---|---|
| [`exercises/en/`](exercises/en/00-overview.md) | the exercises in English |
| [`exercises/de/`](exercises/de/00-ueberblick.md) | the same exercises in German |
| `AnimalShelter.slnx` in the root | the scaffold: two projects that build, and nothing else. This is where you work |
| [`solution/`](solution/) | the complete sample solution, implementation and tests |

Five main exercises build on each other. Seven extras each hang off one of
them and are independent of one another.

## What you need

| Tool | For what |
|---|---|
| [.NET SDK 10](https://dotnet.microsoft.com/download) | builds and tests the code |
| an editor of your choice | Visual Studio, Rider and VS Code all do what is needed here |

That is the whole list. No database, no web server, no packages beyond xUnit,
and the test project fetches that itself.

## Getting it onto your computer

**Without Git:** on <https://github.com/ftrauernicht/csharp-deep-dive-exercises>
use the green **Code** button, then **Download ZIP**, and extract the archive
anywhere.

**With Git:**

```
git clone https://github.com/ftrauernicht/csharp-deep-dive-exercises.git
```

## Getting started

```
dotnet build
dotnet test
```

The first time, exactly one test runs and it checks nothing. It is there so you
can see that the chain from the project file to the console works before you
write your first test. After that it can go.

Start reading here: [Overview](exercises/en/00-overview.md).

## The sample solution

`solution/` holds a complete version with 240 tests, the filled-in predictions
among them. The CI builds and tests it, twice in a row, because
`string.GetHashCode()` returns different values per process and a test that
relies on an enumeration order would otherwise only show up on the next run.

**Do not look there before you have attempted an exercise.** For an
implementation, a glance costs you the practice. For a prediction it costs you
the whole exercise, because knowing the answer is exactly what makes it
worthless.

## Two languages

Every exercise exists in German and in English, as two files with the same
content. Code and code comments are always English. That is not sloppiness but
the habit that holds at work: source code is written in English regardless of
which language the team speaks.

## Where this came from

The exercises started as a practice package for an apprentice, once the basics
were in place and the question came up what actually happens inside `Dictionary`
and LINQ. They are public because the answer to that question is not specific
to any apprentice.

Sibling repositories with the same layout:
[javascript-course](https://github.com/ftrauernicht/javascript-course),
[csharp-oop-course](https://github.com/ftrauernicht/csharp-oop-course),
[java-games-course](https://github.com/ftrauernicht/java-games-course).

## Licence

[MIT](LICENSE). Use it, change it, pass it on.
