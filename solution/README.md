🇬🇧 English | 🇩🇪 [Deutsch](README.de.md)

# Sample solution

The complete solution: all five exercises, all seven extras, 240 tests, and in
every prediction test the prediction sits as a comment above the assertion.

**If you are working through the exercises right now, you are in the wrong
place.** On a prediction a glance costs you the whole exercise, not just the
practice. Back to the [overview](../exercises/en/00-overview.md).

```
dotnet test solution/AnimalShelter.slnx
```

| File | Contents |
|---|---|
| `src/AnimalShelter.Core/` | the implementation, sorted by namespace |
| `tests/AnimalShelter.Tests/` | 20 test files, the same structure |
| [`LINQ.md`](LINQ.md) | the answer to exercise 5.3, with measured figures |

The comments in the code are part of the solution rather than decoration. They
say why a decision went the way it did and what it cost, and in two places they
hold the result of a measurement that contradicted the guess.
