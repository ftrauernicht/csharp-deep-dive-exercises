🇬🇧 English | 🇩🇪 [Deutsch](../de/LERNMATERIAL.md)

[← Overview](00-overview.md)

# Learning material

This list is short, and it is short on purpose. There are a hundred videos on
every topic in these exercises; what is missing is never the material, it is
the time. So there is one source per topic here, not five.

**Read the source after the exercise, not before.** If you already know the
answer you cannot get the prediction wrong any more, and with that the
practice effect is gone. The order is: try, fail, read up, understand why it failed.

## Microsoft Learn

The reference. Free, current, and in case of a contradiction it wins against
any video and any blog post.

| Page | What it says | Exercise |
|---|---|---|
| [Polymorphism](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/polymorphism) | `virtual`, `override`, `abstract`, and how the runtime decides which method runs | 1 |
| [Knowing when to use override and new keywords](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/knowing-when-to-use-override-and-new-keywords) | the difference that decides the prediction in exercise 1 | 1 |
| [How to define value equality for a type](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type) | the complete list of what belongs to `Equals`, operators included | 3 |
| [Object.GetHashCode — Remarks](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-object-gethashcode) | the contract between `Equals` and `GetHashCode`, and why a key has to be immutable | 3, 4 |
| [IComparable&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.icomparable-1) | what `CompareTo` is allowed to return, and the promise that collides with equality in 3.4 | 3 |
| [Indexers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/indexers/) | `this[...]`, several indexers on one class, told apart by type | 4, Z6 |
| [Deferred execution and lazy evaluation](https://learn.microsoft.com/en-us/dotnet/standard/linq/deferred-execution-lazy-evaluation) | which LINQ method computes immediately and which does not | 5 |

The **Remarks** section under `GetHashCode` is a list of rules you otherwise
learn the hard way. Of the seven pages it pays off most; read it again after
exercise 3.

## Videos

Five of them, one per topic, roughly an hour together. Chosen for the shortest
path to the one insight the exercise needs, not for completeness.

| Video | Length | Exercise |
|---|---|---|
| [I Cut My Code in Half After Adding Just One Virtual Method](https://www.youtube.com/watch?v=jgtPwXHTdHc) · Zoran Horvat | 13 min | 1, 2 |
| [Here Is When HashSet & Dictionary Call GetHashCode and Equals](https://www.youtube.com/watch?v=wO3Rg5dV9GI) · Zoran Horvat | 8 min | 3 |
| [HashMaps & Dictionaries, Explained Simply](https://www.youtube.com/watch?v=y11XNXi9dgs) · Nic Barker | 23 min | 4 |
| [The Ultimate Guide to yield return and IEnumerable](https://www.youtube.com/watch?v=2chM9iMeY7k) · Zoran Horvat | 8 min | 5 |
| [Record vs Class vs Struct - What Most Developers Get Wrong](https://www.youtube.com/watch?v=Zmz3rpPyunQ) · Zoran Horvat | 9 min | Z5 |

The first video is the most important one for exercise 2, because it shows the
same move that is asked for there: a type check disappears because the type
itself knows the answer.

The third one explains hash tables without C#, at the level of the concept.
Once you have built exercise 4 you will recognize your own baskets in it. At
that moment a dictionary stops being a line of syntax and becomes a data
structure.

## Paid courses

[Dometrain](https://dometrain.com/) has a course on every topic in this
package, and they are good. They cost money, so only the three chapters that
sit directly on an exercise are listed here:

- **Hands-On: Data Structures & Algorithms in C#** (Nick Chapsas), chapter 14
  "Hash Tables": hash function, collisions with chaining, resize. That is
  exercise 4 and Z3 as a video.
- **Hands-On: LINQ in C#** (Nick Chapsas), chapter 1 "Deferred vs Immediate
  Execution" and chapter 12 "Building LINQ from Scratch": exercise 5.
- **Mastering C#** (Sergey Teplyakov), chapter 3 "Equality Semantics":
  exercise 3, considerably more demanding than the rest of this list.

None of it is necessary. The package is built so that the Microsoft Learn
pages above are enough.

## What is missing and why

There is nothing here on **xUnit**. The test framework is a tool, not a topic:
`[Fact]`, `Assert.Equal`, `Assert.Throws`. None of these exercises needs
more than that, and whatever turns out to be needed beyond it gets learned
the moment it is needed. If you have no idea at all how a test project runs,
start with the `dotnet test` from the
[overview](00-overview.md#getting-started) and read the rest as soon as a test
claims something you do not understand.

On **Records** the table above holds exactly one video, and it is tagged for
Z5 rather than exercise 3. On **Pattern Matching** and **Streams** there is
nothing here at all. These things solve problems you only see after you have
solved them by hand. Read in that order, they are a relief;
read in the other order, they are syntax to memorize.
