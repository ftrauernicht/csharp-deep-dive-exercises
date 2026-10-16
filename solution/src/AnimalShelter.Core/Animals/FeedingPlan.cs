namespace AnimalShelter.Core.Animals;

public static class FeedingPlan
{
    /// <summary>
    /// Adds up what the whole group eats in a day.
    ///
    /// There is no `if`, no `is` and no `switch` on the species here, and that
    /// is the whole exercise. Each animal already knows its own appetite; this
    /// method only has to ask. Add a species tomorrow and this line still
    /// holds.
    /// </summary>
    public static double TotalDailyFoodInGrams(IEnumerable<Animal> animals) =>
        animals.Sum(animal => animal.CalculateDailyFoodInGrams());
}
