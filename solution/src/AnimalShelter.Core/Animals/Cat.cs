namespace AnimalShelter.Core.Animals;

public class Cat : Animal, IVaccinatable
{
    private static readonly string[] RequiredList = ["CatFlu", "Panleukopenia"];

    private readonly VaccinationRecord vaccinations = new(RequiredList);

    public Cat(string name, string chipNumber, int ageInMonths, double weightInKg,
        bool isOutdoor = false)
        : base(name, chipNumber, ageInMonths, weightInKg)
    {
        IsOutdoor = isOutdoor;
    }

    public bool IsOutdoor { get; set; }

    public override string MakeSound() => "Meow";

    /// <summary>40 g per kilogram, twice that below twelve months.</summary>
    public override double CalculateDailyFoodInGrams()
    {
        double food = WeightInKg * 40;
        return AgeInMonths < 12 ? food * 2 : food;
    }

    public override string Describe() =>
        base.Describe() + (IsOutdoor ? " (outdoor)" : " (indoor)");

    public override decimal TreatmentCostInEuro => 45.00m;

    public IReadOnlyList<string> RequiredVaccinations => vaccinations.Required;

    public IReadOnlyCollection<string> ReceivedVaccinations => vaccinations.Received;

    public void Vaccinate(string vaccination) => vaccinations.Give(vaccination);

    public bool IsFullyVaccinated => vaccinations.IsComplete;
}
