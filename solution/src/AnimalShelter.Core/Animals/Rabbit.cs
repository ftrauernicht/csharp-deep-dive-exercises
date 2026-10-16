namespace AnimalShelter.Core.Animals;

public class Rabbit : Animal, IVaccinatable
{
    private static readonly string[] RequiredList = ["RHD", "Myxomatosis"];

    private readonly VaccinationRecord vaccinations = new(RequiredList);

    public Rabbit(string name, string chipNumber, int ageInMonths, double weightInKg)
        : base(name, chipNumber, ageInMonths, weightInKg)
    {
    }

    /// <summary>A rabbit is quiet. The dots are the sound.</summary>
    public override string MakeSound() => "...";

    public override double CalculateDailyFoodInGrams() => WeightInKg * 50;

    /// <summary>
    /// This is where exercise 1.5 ends up: `new` replaced by `override`.
    ///
    /// With `new`, a Rabbit stored in an Animal variable answered with the
    /// base version, because the compiler picked the method from the
    /// variable's declared type. With `override` it answers with this one,
    /// whatever the variable says. ToString() shows it too: it calls
    /// Describe() from inside Animal and therefore never saw the `new` one.
    /// </summary>
    public override string Describe() => $"Rabbit {Name}";

    public override decimal TreatmentCostInEuro => 30.00m;

    public IReadOnlyList<string> RequiredVaccinations => vaccinations.Required;

    public IReadOnlyCollection<string> ReceivedVaccinations => vaccinations.Received;

    public void Vaccinate(string vaccination) => vaccinations.Give(vaccination);

    public bool IsFullyVaccinated => vaccinations.IsComplete;
}
