namespace AnimalShelter.Core.Animals;

/// <summary>
/// Holds the vaccination logic once, so that Cat, Dog and Rabbit do not each
/// carry their own copy of it. Exercise 2.2 asks for exactly this: the rules
/// live in one place, the species only supply their list.
/// </summary>
public sealed class VaccinationRecord
{
    private readonly HashSet<string> received = new(StringComparer.Ordinal);

    public VaccinationRecord(IReadOnlyList<string> required)
    {
        Required = required;
    }

    public IReadOnlyList<string> Required { get; }

    public IReadOnlyCollection<string> Received => received;

    /// <summary>
    /// Giving the same vaccination twice is allowed and counts once -- that is
    /// a HashSet, not an oversight. A vaccination the animal does not need is
    /// a mistake and says so.
    /// </summary>
    public void Give(string vaccination)
    {
        if (!Required.Contains(vaccination))
        {
            throw new ArgumentException(
                $"'{vaccination}' is not one of the required vaccinations.",
                nameof(vaccination));
        }

        received.Add(vaccination);
    }

    public bool IsComplete => Required.All(received.Contains);

    public IEnumerable<string> Missing => Required.Where(v => !received.Contains(v));
}
