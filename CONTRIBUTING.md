🇬🇧 English | 🇩🇪 [Deutsch](CONTRIBUTING.de.md)

# Contributing to C# Deep Dive Exercises

This is a personal, solo-maintained project.

- Issues and pull requests are welcome.
- Keep pull requests small and focused: easier to review, easier to revert.
- No contributor licence agreement is required.
- Response times vary; this is not maintained on a fixed schedule.

## Adding or changing an exercise

The conventions below are not visible in any single file. They show up only
when several files are compared, so they are written down here for a new
contributor or a fresh AI session with no memory of earlier work.

**Two solutions, and they are not interchangeable**

`AnimalShelter.slnx` in the root is the scaffold. It holds two projects that
build, one test that checks nothing, and no exercise content at all. It must
stay that way: the scaffold is the starting position, and exercise 3.1 asks the
reader to predict what `a == b` does *before* anything is implemented. A
scaffold that already declares `IEquatable<Animal>` has given that answer away.

`solution/AnimalShelter.slnx` is the sample solution and the one the CI tests.
It is complete: every exercise, every extra, every prediction answered.

**Every number is measured**

If an exercise text states a figure, a test in `solution/` produces it. This is
not a formality. The timing exercise compares a list lookup against a
dictionary lookup at 100,000 entries, and the measured ratio was 2903 on one
machine and 563 on another. Any absolute figure in that exercise would have
been wrong on at least one of the two.

What follows from that:

- Never write a millisecond count into an assertion. Assert a ratio, and
  assert that the ratio grows with the number of entries.
- Warm up before measuring, or the first call pays for the JIT compiler.
- Do not state an element count, a call count or an exception type from
  memory. Reproduce it, then write down what happened.

**Order is not a promise**

`Shelter` hands out its animals in the order its baskets happen to hold them,
which follows the hash code of the chip number, which `string.GetHashCode()`
seeds per process. A test that compares a sequence without sorting first passes
with one seed and fails with the next. The CI therefore runs the suite twice,
and a test that depends on order is a bug even when it is green.

Where ordering *is* part of the contract, as with `NamesSorted()` and
`HeaviestThree()`, checking it is correct. The query establishes the order
itself.

**Bilingual**

- Each exercise is two files with the same content, in `exercises/en/` and
  `exercises/de/`, not one bilingual file. The file names are translated, the
  numeric prefix is not: `00-overview.md` pairs with `00-ueberblick.md`.
- Prose outside the exercises follows `NAME.md` plus `NAME.de.md`. `CLAUDE.md`
  is the exception; it addresses a tool rather than a reader.
- Code and code comments are always English, in both language versions. The
  one exception is the example of a prediction comment inside the German
  exercise text: there the learner is writing into their own file, in their
  own language.
- `scripts/check-bilingual-sync.sh` enforces the pairing. Run it before
  pushing.
- Changing one language without the other leaves the repository in a state no
  reviewer can judge. Change both in the same commit.

**Chapter shape**

Line 1 is the language switcher, line 3 the breadcrumb, then the heading, then
`**Ziel:**` or `**Goal:**`. Every exercise chapter ends with "What you learned
here"; the overview and the learning material page do not, because nothing was
practised there.
Prose wraps at about 80 characters.

Unlike the sibling course repositories, there are no 🟢/🟡/🔴 difficulty
markers. Everything in an exercise is required, the extras are optional as a
whole, and a marker per section would suggest a choice that does not exist
here.

**Writing**

Avoid the patterns that make a text read as machine-written: the dash as an
all-purpose connector, hollow intensifiers ("genuinely", "truly"), mechanical
transitions ("Furthermore", "It is worth noting that"), a sentence that repeats
the previous one as confirmation, and three-item lists where only two items
carry content. This applies while writing, not as a cleanup pass afterwards.

**Before pushing**

- `dotnet build AnimalShelter.slnx` and `dotnet test AnimalShelter.slnx`, which
  is what a reader runs first.
- `dotnet test solution/AnimalShelter.slnx` twice in a row. Zero warnings, zero
  failures, both times.
- `bash scripts/check-bilingual-sync.sh`.
- Read the exercise text you changed from top to bottom in both languages. A
  cross-reference to "exercise 4" goes stale silently when something is
  renumbered.

**Commit messages** follow
[Conventional Commits](https://www.conventionalcommits.org/), lower case and in
the imperative (`feat: add exercise 4 on baskets and collisions`,
`fix: ...`, `docs: ...`, `ci: ...`).
