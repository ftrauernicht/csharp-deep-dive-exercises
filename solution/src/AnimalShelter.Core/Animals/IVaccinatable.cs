namespace AnimalShelter.Core.Animals;

/// <summary>
/// An animal that can be vaccinated. A budgie deliberately does not implement
/// this, which is what makes `is IVaccinatable` in Veterinarian meaningful:
/// it asks about a capability, not about a species.
/// </summary>
public interface IVaccinatable
{
    IReadOnlyList<string> RequiredVaccinations { get; }

    IReadOnlyCollection<string> ReceivedVaccinations { get; }

    void Vaccinate(string vaccination);

    bool IsFullyVaccinated { get; }
}
