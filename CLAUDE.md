# csharp-deep-dive-exercises

A personal project. This folder is the entire project; there is no separate
documentation folder. See `README.md` for what it does and `CONTRIBUTING.md`
for the conventions.

**Two solutions live here on purpose.** `AnimalShelter.slnx` in the root is the
scaffold the learner works in and stays empty. `solution/AnimalShelter.slnx` is
the sample solution and is the one the CI builds and tests. A change to one of
them is almost never correct without a look at the other.

**Every number in an exercise text is measured, not estimated.** Timings,
element counts, how often a lambda runs: if the text states a figure, there is
a test in `solution/` that produces it. Do not add a figure that no test backs
up, and do not write a millisecond count into an assertion.

**Never commit secrets** such as credentials, API keys, tokens or personal
data. This repository is public.

The cross-project rules live in the global guide at
`%USERPROFILE%\.claude\CLAUDE.md`.
