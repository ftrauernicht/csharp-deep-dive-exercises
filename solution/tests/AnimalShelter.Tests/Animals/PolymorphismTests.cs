using AnimalShelter.Core.Animals;

namespace AnimalShelter.Tests.Animals;

/// <summary>
/// One call, four answers. Nothing here asks what species it is holding.
/// </summary>
public class PolymorphismTests
{
    public static TheoryData<Animal, string> SoundsPerSpecies() => new()
    {
        { new Cat("Mimi", "276098100000001", 36, 4.0), "Meow" },
        { new Dog("Rex", "276098100000002", 48, 15.0), "Woof" },
        { new Rabbit("Hoppel", "276098100000003", 18, 2.0), "..." },
        { new Budgie("Hansi", "276098100000004", 24, 0.04), "Tweet" },
    };

    [Theory]
    [MemberData(nameof(SoundsPerSpecies))]
    public void MakeSound_ForEachSpecies_ReturnsItsOwnSound(Animal animal, string expected)
    {
        Assert.Equal(expected, animal.MakeSound());
    }

    [Fact]
    public void MakeSound_OverAListOfAnimals_CollectsEverySoundInOrder()
    {
        List<Animal> animals =
        [
            new Cat("Mimi", "276098100000011", 36, 4.0),
            new Dog("Rex", "276098100000012", 48, 15.0),
            new Rabbit("Hoppel", "276098100000013", 18, 2.0),
        ];

        List<string> sounds = [];

        foreach (Animal animal in animals)
        {
            sounds.Add(animal.MakeSound());
        }

        Assert.Equal(["Meow", "Woof", "..."], sounds);
    }

    [Fact]
    public void TotalDailyFoodInGrams_MixedGroup_AddsUpEveryIndividualRation()
    {
        List<Animal> animals =
        [
            new Cat("Mimi", "276098100000021", 36, 4.0),      // 4.0 * 40      = 160
            new Cat("Flecki", "276098100000022", 4, 3.0),     // 3.0 * 40 * 2  = 240
            new Dog("Rex", "276098100000023", 48, 15.0),      // 15.0 * 30     = 450
            new Rabbit("Hoppel", "276098100000024", 18, 2.0), // 2.0 * 50      = 100
            new Budgie("Hansi", "276098100000025", 24, 0.04), // flat          =  10
        ];

        Assert.Equal(960.0, FeedingPlan.TotalDailyFoodInGrams(animals));
    }

    [Fact]
    public void TotalDailyFoodInGrams_EmptyGroup_IsZero()
    {
        Assert.Equal(0.0, FeedingPlan.TotalDailyFoodInGrams([]));
    }
}
