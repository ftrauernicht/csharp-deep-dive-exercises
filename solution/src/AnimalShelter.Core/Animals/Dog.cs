namespace AnimalShelter.Core.Animals;

public class Dog : Animal, IVaccinatable
{
    private static readonly string[] RequiredList = ["Distemper", "Parvovirus", "Rabies"];

    private readonly VaccinationRecord vaccinations = new(RequiredList);

    public Dog(string name, string chipNumber, int ageInMonths, double weightInKg,
        string breed = "")
        : base(name, chipNumber, ageInMonths, weightInKg)
    {
        Breed = breed;
    }

    public string Breed { get; set; }

    public override string MakeSound() => "Woof";

    /// <summary>
    /// Heavier dogs cost more. Note where the boundary sits: at exactly 20 kg
    /// this is still the cheaper price, because the old code asked
    /// `WeightInKg > 20` and the tests from 2.1 have to stay green.
    /// </summary>
    public override decimal TreatmentCostInEuro => WeightInKg > 20 ? 70.00m : 55.00m;

    public IReadOnlyList<string> RequiredVaccinations => vaccinations.Required;

    public IReadOnlyCollection<string> ReceivedVaccinations => vaccinations.Received;

    public void Vaccinate(string vaccination) => vaccinations.Give(vaccination);

    public bool IsFullyVaccinated => vaccinations.IsComplete;
}
