namespace AnimalShelter.Core.Animals;

/// <summary>
/// Added in exercise 2.1 to prove the point: Veterinarian is not touched for
/// it. A budgie cannot be vaccinated, so it does not implement IVaccinatable
/// -- which is what lets VaccinateAllMissing skip it without naming a species.
/// </summary>
public class Budgie : Animal
{
    public Budgie(string name, string chipNumber, int ageInMonths, double weightInKg)
        : base(name, chipNumber, ageInMonths, weightInKg)
    {
    }

    public override string MakeSound() => "Tweet";

    /// <summary>Ten grams a day, whatever the bird weighs.</summary>
    public override double CalculateDailyFoodInGrams() => 10;

    public override decimal TreatmentCostInEuro => 25.00m;
}
