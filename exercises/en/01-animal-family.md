🇬🇧 English | 🇩🇪 [Deutsch](../de/01-tierfamilie.md)

[← Overview](00-overview.md) · Next: [Vet and interfaces](02-vet-and-interfaces.md)

# Exercise 1: The animal family

**Goal:** a base class that three kinds of animal inherit from, and a method
that computes with all three without naming a single one of them.

## 1.1 The abstract base class `Animal`

In `AnimalShelter.Core.Animals`.

| Member | Kind |
|---|---|
| `Name` | `string`, get and set |
| `ChipNumber` | `string`, get only, set in the constructor |
| `AgeInMonths` | `int`, get and set |
| `WeightInKg` | `double`, get and set |
| `MakeSound()` | `abstract`, returns `string` |
| `CalculateDailyFoodInGrams()` | `virtual`, returns `double`, default: 30 g per kilogram |
| `Describe()` | `virtual`, returns `string`, default: `"{Name}, {AgeInMonths} months"` |
| `ToString()` | `override`, returns `Describe()` |

The constructor is `protected` and takes the four values in this order:

```csharp
protected Animal(string name, string chipNumber, int ageInMonths, double weightInKg)
```

Keep that order. Every later exercise creates animals, and the sample
solution does it with exactly this signature.

`ChipNumber` is the only member without a setter, and that is no oversight:
an animal gets heavier and older, but it does not get a new chip number.
What happens when you see it differently is in exercise 3.

The constructor throws an `ArgumentException` when

- `Name` is empty or consists of spaces only,
- `ChipNumber` does not consist of exactly 15 digits,
- `AgeInMonths` is negative,
- `WeightInKg` is not greater than 0.

Two boundaries that are easy to read past: an age of **0** months is valid,
because a newborn animal is not an error. And a weight of 0 is already
invalid, not only a negative one.

## 1.2 The concrete animals

| Class | `MakeSound()` | Food per day | In addition |
|---|---|---|---|
| `Cat` | `"Meow"` | 40 g/kg, double that below 12 months, so 80 | `IsOutdoor` (`bool`) |
| `Dog` | `"Woof"` | the base class default | `Breed` (`string`) |
| `Rabbit` | `"..."` | 50 g/kg | — |

The rabbit's sound is meant literally. A rabbit is quiet.

The "In addition" column means a property with a constructor parameter: `Cat`
gets `isOutdoor`, `Dog` gets `breed`. Both come after the four values of the
base class.

`Cat` overrides `Describe()` and appends `" (outdoor)"` or `" (indoor)"`. Use
`base.Describe()` for that and do not assemble the string a second time.
Otherwise two places know the same format, and next time someone changes only
one of them.

Note that `Dog` overrides nothing about the food. `MakeSound()` it has to
supply, because the method is `abstract` and the compiler refuses otherwise;
for the food it has a choice and takes the default. That is exactly what a
default on a `virtual` method is for.

## 1.3 The feeding plan

A static class `FeedingPlan`, also in `AnimalShelter.Core.Animals`, with a
static method:

```csharp
public static double TotalDailyFoodInGrams(IEnumerable<Animal> animals)
```

**The method contains no `if`, no `is` and no `switch` on the animal type.**

That is the exercise, and not a style rule. Every animal already
knows its food requirement; a method that asks for it stays correct when a
fifth kind of animal arrives tomorrow. A method that interrogates the type
has to be changed then, and changed by someone who knows it exists.

## 1.4 Tests

- `MakeSound()` for every kind of animal, checked through a concrete class:
  `Animal` is abstract, `new Animal(...)` does not compile, and the
  validation rules therefore run through a species of your choice as well
- food requirement: adult cat, kitten, cat at **exactly 12 months**, dog,
  rabbit
- every single validation rule
- `Describe()` for an outdoor cat and an indoor cat
- `TotalDailyFoodInGrams` with a mixed list
- a test that walks a `List<Animal>` holding a cat, a dog and a rabbit and
  collects all the sounds

The cat at exactly twelve months is not there by chance. The rule says
"double that below 12 months", so at exactly twelve it no longer applies, and
that boundary is the place where a `<` and a `<=` differ.

**All animals are stored in variables of type `Animal`**, not of the concrete
type. The concrete type may appear only where a species-specific property
such as `IsOutdoor` is checked. The reason becomes visible in 1.5.

## 1.5 Prediction: `new` instead of `override`

Give `Rabbit` this method for the time being:

```csharp
public new string Describe() => $"Rabbit {Name}";
```

Write tests for:

```csharp
Rabbit a = new Rabbit(...);
Animal b = new Rabbit(...);
```

What do `a.Describe()`, `b.Describe()` and `b.ToString()` return?

Enter your three predictions as comments before you run the test. The third
one in particular pays off: `ToString()` sits in `Animal` and calls
`Describe()` from there.

Then replace `new` with `override` and run the same tests again. Adjust the
asserts and leave the old predictions standing as comments.

The method then stays, as an `override`. That is the end state of this
exercise, and the sample solution looks the same: from here on `Rabbit`
describes itself as `"Rabbit Hoppel"`.

### Why the second result looks different

With `new`, the type written **in the source** decides. `Rabbit a` calls the
method from `Rabbit`, `Animal b` the one from `Animal`, and both calls are
settled at compile time. The same object answers differently depending on
which pair of glasses you look at it through.

With `override`, the object decides. `Animal` carries a slot for
`Describe()` that a subclass can overwrite, and the call looks it up there
at run time.

`ToString()` is the case that shows it. The method sits in `Animal` and does
not even know `Rabbit` exists. With `new` it therefore cannot possibly find
the method from `Rabbit`; with `override` it finds exactly that one. This is
why `new` is almost never what you want: it looks like overriding and works
only where the static type happens to match.

## What you learned here

- An abstract base class that brings along a default not every subclass has
  to inherit
- `abstract` against `virtual`: one forces an answer, the other offers one
- `base.Describe()` instead of the same string in two places
- A method over a mixed list that knows not a single type
- What `new` on a method does, and why the static type decides the outcome
