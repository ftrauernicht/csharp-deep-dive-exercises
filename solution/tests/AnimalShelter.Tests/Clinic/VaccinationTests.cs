using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Clinic;

namespace AnimalShelter.Tests.Clinic;

/// <summary>
/// Exercises 2.2 and 2.3: which shots a species needs, what happens with one it
/// does not need, and what the veterinarian does with a mixed group.
/// </summary>
public class VaccinationTests
{
    private readonly Veterinarian veterinarian = new();

    [Fact]
    public void RequiredVaccinations_Cat_AreCatFluAndPanleukopenia()
    {
        Cat cat = new Cat("Mimi", "276098100000001", 36, 4.0);

        Assert.Equal(["CatFlu", "Panleukopenia"], cat.RequiredVaccinations);
    }

    [Fact]
    public void RequiredVaccinations_Dog_AreDistemperParvovirusAndRabies()
    {
        Dog dog = new Dog("Rex", "276098100000002", 48, 15.0);

        Assert.Equal(["Distemper", "Parvovirus", "Rabies"], dog.RequiredVaccinations);
    }

    [Fact]
    public void RequiredVaccinations_Rabbit_AreRhdAndMyxomatosis()
    {
        Rabbit rabbit = new Rabbit("Hoppel", "276098100000003", 18, 2.0);

        Assert.Equal(["RHD", "Myxomatosis"], rabbit.RequiredVaccinations);
    }

    [Fact]
    public void Vaccinate_VaccinationTheSpeciesDoesNotNeed_ThrowsArgumentException()
    {
        Cat cat = new Cat("Mimi", "276098100000004", 36, 4.0);

        // Rabies is a real vaccination, just not one on the cat's list.
        Assert.Throws<ArgumentException>(() => cat.Vaccinate("Rabies"));
        Assert.Empty(cat.ReceivedVaccinations);
    }

    [Fact]
    public void Vaccinate_SameVaccinationTwice_CountsOnce()
    {
        Cat cat = new Cat("Mimi", "276098100000005", 36, 4.0);

        cat.Vaccinate("CatFlu");
        cat.Vaccinate("CatFlu");

        Assert.Single(cat.ReceivedVaccinations);
        Assert.Contains("CatFlu", cat.ReceivedVaccinations);
    }

    [Fact]
    public void IsFullyVaccinated_WithoutAnyVaccination_IsFalse()
    {
        Dog dog = new Dog("Rex", "276098100000006", 48, 15.0);

        Assert.False(dog.IsFullyVaccinated);
    }

    [Fact]
    public void IsFullyVaccinated_AfterOnlySomeVaccinations_IsStillFalse()
    {
        Dog dog = new Dog("Rex", "276098100000007", 48, 15.0);

        dog.Vaccinate("Distemper");
        dog.Vaccinate("Parvovirus");

        Assert.False(dog.IsFullyVaccinated);
    }

    [Fact]
    public void IsFullyVaccinated_AfterEveryRequiredVaccination_IsTrue()
    {
        Dog dog = new Dog("Rex", "276098100000008", 48, 15.0);

        foreach (string vaccination in dog.RequiredVaccinations)
        {
            dog.Vaccinate(vaccination);
        }

        Assert.True(dog.IsFullyVaccinated);
    }

    [Fact]
    public void VaccinateAllMissing_MixedGroup_GivesEveryMissingShotAndSkipsTheBudgie()
    {
        Cat cat = new Cat("Mimi", "276098100000011", 36, 4.0);
        Dog dog = new Dog("Rex", "276098100000012", 48, 15.0);
        Rabbit rabbit = new Rabbit("Hoppel", "276098100000013", 18, 2.0);
        Budgie budgie = new Budgie("Hansi", "276098100000014", 24, 0.04);

        List<Animal> animals = [cat, dog, rabbit, budgie];

        int given = veterinarian.VaccinateAllMissing(animals);

        // Two for the cat, three for the dog, two for the rabbit, none for the bird.
        Assert.Equal(7, given);
        Assert.True(cat.IsFullyVaccinated);
        Assert.True(dog.IsFullyVaccinated);
        Assert.True(rabbit.IsFullyVaccinated);

        // The budgie is left alone without anybody asking for its species: it
        // simply does not implement the capability.
        Assert.False(budgie is IVaccinatable);
    }

    [Fact]
    public void VaccinateAllMissing_GroupWithOnePartlyVaccinatedAnimal_CountsOnlyWhatIsMissing()
    {
        Cat cat = new Cat("Mimi", "276098100000015", 36, 4.0);
        cat.Vaccinate("CatFlu");

        int given = veterinarian.VaccinateAllMissing([cat]);

        Assert.Equal(1, given);
        Assert.True(cat.IsFullyVaccinated);
    }

    [Fact]
    public void VaccinateAllMissing_CalledASecondTime_ReturnsZero()
    {
        List<Animal> animals =
        [
            new Cat("Mimi", "276098100000021", 36, 4.0),
            new Dog("Rex", "276098100000022", 48, 15.0),
            new Rabbit("Hoppel", "276098100000023", 18, 2.0),
            new Budgie("Hansi", "276098100000024", 24, 0.04),
        ];

        Assert.Equal(7, veterinarian.VaccinateAllMissing(animals));

        // Nothing is missing any more, and giving a shot again would have been
        // allowed -- so a zero here is the record of what the method checks.
        Assert.Equal(0, veterinarian.VaccinateAllMissing(animals));
    }
}
