using AnimalShelter.Core.Animals;

namespace AnimalShelter.Tests.Animals;

/// <summary>
/// The daily ration per species. Animals are held in Animal variables
/// throughout: CalculateDailyFoodInGrams is declared there, and which version
/// runs is decided by the object, not by the variable.
/// </summary>
public class FoodTests
{
    [Fact]
    public void CalculateDailyFoodInGrams_AdultCat_IsFortyGramsPerKilogram()
    {
        Animal cat = new Cat("Mimi", "276098100000001", 36, 4.0);

        Assert.Equal(160.0, cat.CalculateDailyFoodInGrams());
    }

    [Fact]
    public void CalculateDailyFoodInGrams_Kitten_IsDoubleTheAdultRation()
    {
        Animal kitten = new Cat("Flecki", "276098100000002", 4, 3.0);

        Assert.Equal(240.0, kitten.CalculateDailyFoodInGrams());
    }

    [Fact]
    public void CalculateDailyFoodInGrams_CatAtExactlyTwelveMonths_IsTheAdultRation()
    {
        // The boundary: the rule asks `AgeInMonths < 12`, so twelve months
        // already counts as grown up and the doubling stops here.
        Animal cat = new Cat("Luna", "276098100000003", 12, 4.0);

        Assert.Equal(160.0, cat.CalculateDailyFoodInGrams());
    }

    [Fact]
    public void CalculateDailyFoodInGrams_Dog_IsThirtyGramsPerKilogram()
    {
        // A dog adds nothing of its own here and keeps the base rule.
        Animal dog = new Dog("Rex", "276098100000004", 48, 15.0);

        Assert.Equal(450.0, dog.CalculateDailyFoodInGrams());
    }

    [Fact]
    public void CalculateDailyFoodInGrams_Rabbit_IsFiftyGramsPerKilogram()
    {
        Animal rabbit = new Rabbit("Hoppel", "276098100000005", 18, 2.0);

        Assert.Equal(100.0, rabbit.CalculateDailyFoodInGrams());
    }

    [Theory]
    [InlineData(0.04)]
    [InlineData(0.09)]
    public void CalculateDailyFoodInGrams_Budgie_IsTenGramsRegardlessOfWeight(double weightInKg)
    {
        Animal budgie = new Budgie("Hansi", "276098100000006", 24, weightInKg);

        Assert.Equal(10.0, budgie.CalculateDailyFoodInGrams());
    }
}
