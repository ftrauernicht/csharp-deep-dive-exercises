using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Clinic;

namespace AnimalShelter.Tests.Clinic;

/// <summary>
/// The price list of exercise 2.1. The veterinarian takes an Animal and nothing
/// more specific, so every case below is passed as one.
/// </summary>
public class VeterinarianTests
{
    private readonly Veterinarian veterinarian = new();

    public static TheoryData<Animal, decimal> PricePerPatient() => new()
    {
        { new Cat("Mimi", "276098100000001", 36, 4.0), 45.00m },
        { new Dog("Rex", "276098100000002", 48, 15.0), 55.00m },
        // The boundary is `> 20`, so exactly 20 kg still pays the lower price.
        { new Dog("Bruno", "276098100000003", 60, 20.0), 55.00m },
        { new Dog("Hasso", "276098100000004", 72, 25.0), 70.00m },
        { new Rabbit("Hoppel", "276098100000005", 18, 2.0), 30.00m },
        { new Budgie("Hansi", "276098100000006", 24, 0.04), 25.00m },
    };

    [Theory]
    [MemberData(nameof(PricePerPatient))]
    public void CalculateTreatmentCost_ForEachPatient_ReturnsItsPrice(Animal animal, decimal expected)
    {
        Assert.Equal(expected, veterinarian.CalculateTreatmentCost(animal));
    }

    [Fact]
    public void CalculateTreatmentCost_DogJustAboveTwentyKilograms_ReturnsTheHigherPrice()
    {
        // The other side of the same boundary, spelled out on its own.
        Animal dog = new Dog("Bello", "276098100000007", 60, 20.1);

        Assert.Equal(70.00m, veterinarian.CalculateTreatmentCost(dog));
    }
}
