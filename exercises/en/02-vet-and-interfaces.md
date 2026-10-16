🇬🇧 English | 🇩🇪 [Deutsch](../de/02-tierarzt-und-interfaces.md)

[← The animal family](01-animal-family.md) · Next: [Equality and ordering](03-equality-and-ordering.md)

# Exercise 2: Vet and interfaces

**Goal:** turn a method full of type checks into a method without a single
one, and do it without any test changing. Then answer the question of when a
type check is acceptable after all.

## 2.1 Refactoring the vet

Create this class in `AnimalShelter.Core.Clinic`, exactly as it stands:

```csharp
public class Veterinarian
{
    public decimal CalculateTreatmentCost(Animal animal)
    {
        if (animal is Cat)
        {
            return 45.00m;
        }
        else if (animal is Dog dog)
        {
            return dog.WeightInKg > 20 ? 70.00m : 55.00m;
        }
        else if (animal is Rabbit)
        {
            return 30.00m;
        }

        throw new NotSupportedException($"Unknown animal: {animal.GetType().Name}");
    }
}
```

**First step: tests for the current behavior.** Cat, dog at 15 kg, dog at
**exactly 20 kg**, dog at 25 kg, rabbit.

The 20 kg is the case that later proves you broke nothing. The condition
reads `> 20`, so at exactly twenty kilograms the lower price applies. Write
`>=` by accident while refactoring and this one test will tell you.

**Second step: refactor.** Afterwards `CalculateTreatmentCost` contains no
type check at all: the price comes from the animal itself, as a new
property on `Animal`, and the vet does nothing but ask for it. What you
call it is up to you. **The tests from step 1 stay unchanged and green.** If you
have to touch a test, it was not a refactoring but a change.

**Third step: a new kind of animal.** Add `Budgie`: `"Tweet"`, 10 g of food
per day regardless of weight, treatment 25 €.

`Veterinarian` is **not** touched for this. That is the acid test: with the
type check still in place you would have to go in there now, and the old
`throw new NotSupportedException` would have told you nothing beforehand. It
would only have surfaced at run time, with the first budgie.

While refactoring, consider whether the new property on `Animal` should be
`virtual` or `abstract`. Both work; only one forces the next person who adds
a kind of animal to think about the price.

## 2.2 The interface `IVaccinatable`

```csharp
public interface IVaccinatable
{
    IReadOnlyList<string> RequiredVaccinations { get; }
    IReadOnlyCollection<string> ReceivedVaccinations { get; }
    void Vaccinate(string vaccination);
    bool IsFullyVaccinated { get; }
}
```

The two return types differ, and that is deliberate. The required vaccinations
have an order, which is the one in the table below. The ones already given have
none, because the same vaccination given twice counts once.
`IReadOnlyCollection` says exactly that: countable, but without an index anyone
could come to rely on.

| Animal | Required vaccinations |
|---|---|
| `Cat` | `"CatFlu"`, `"Panleukopenia"` |
| `Dog` | `"Distemper"`, `"Parvovirus"`, `"Rabies"` |
| `Rabbit` | `"RHD"`, `"Myxomatosis"` |
| `Budgie` | not vaccinatable |

Rules:

- A vaccination that is not one of the required vaccinations throws an
  `ArgumentException`.
- Giving the same vaccination twice is allowed, but it counts only once.
- **The vaccination logic exists in one place in the code.** Three kinds of
  animal with the same logic does not mean the same logic three times. One
  class that the animals use internally, say `VaccinationRecord`, is enough
  for it.

`Cat`, `Dog` and `Rabbit` implement the interface. `Budgie` stays out of it
on purpose, and that is the precondition for 2.3.

## 2.3 Vaccinate everyone

One more method on `Veterinarian`:

```csharp
public int VaccinateAllMissing(IEnumerable<Animal> animals)
```

Gives every vaccinatable animal all its missing required vaccinations and
returns how many vaccinations were administered.

Tests: a mixed list including a budgie, a second call right afterwards
(which has to return 0), and an animal that already has one of its two
vaccinations.

## The two questions

**Why is `is IVaccinatable` defensible here while `is Cat` in 2.1 is not?**

Write the answer down before you read on. It fits in one sentence.

The difference: `is Cat` asked for the species in order to look up a number
the animal has known all along; the information sat in the wrong place.
`is IVaccinatable` asks whether the animal **can** do something at all, and
only the type system can answer that. There is no sensible value a budgie
could return for `Vaccinate`.

**When do you choose an abstract class, when an interface?**

This answer belongs in writing too. In this project both stand side by side:
`Animal` is abstract, `IVaccinatable` is an interface. Why not the other way
round?

## What you learned here

- Distinguishing a refactoring that unchanged tests have secured from a
  change
- That a type check usually means the information sits in the wrong place
- `abstract` as a way to force the next person into a decision instead of
  letting them fail at run time
- An interface as a statement about a capability, not about a species
- Pulling shared logic into a class of its own instead of inheriting it three
  times
