using AnimalShelter.Core.Animals;

namespace AnimalShelter.Core.Adoption;

/// <summary>
/// Exercise Z7. Requests are kept in a SortedSet, ordered by the day they
/// came in.
///
/// The trap is in CompareTo. A SortedSet decides that two items are the same
/// when CompareTo returns 0, so comparing the date alone makes the set throw
/// away every second request that arrives on a day it already knows -- no
/// exception, no warning, the Add simply returns false. The chip number is
/// the tie-break that stops that.
/// </summary>
public class AdoptionRequest : IComparable<AdoptionRequest>
{
    public AdoptionRequest(string applicant, Animal animal, DateOnly receivedOn)
    {
        if (string.IsNullOrWhiteSpace(applicant))
        {
            throw new ArgumentException("Applicant must not be empty.", nameof(applicant));
        }

        Applicant = applicant;
        Animal = animal ?? throw new ArgumentNullException(nameof(animal));
        ReceivedOn = receivedOn;
    }

    public string Applicant { get; }

    public Animal Animal { get; }

    public DateOnly ReceivedOn { get; }

    public int CompareTo(AdoptionRequest? other)
    {
        if (other is null)
        {
            return 1;
        }

        int byDate = ReceivedOn.CompareTo(other.ReceivedOn);

        if (byDate != 0)
        {
            return byDate;
        }

        int byAnimal = string.CompareOrdinal(Animal.ChipNumber, other.Animal.ChipNumber);
        return byAnimal != 0
            ? byAnimal
            : string.Compare(Applicant, other.Applicant, StringComparison.Ordinal);
    }

    public override string ToString() => $"{ReceivedOn:yyyy-MM-dd} {Applicant} -> {Animal.Name}";
}
