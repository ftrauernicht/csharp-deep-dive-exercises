using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Management;

namespace AnimalShelter.Tests.Management;

/// <summary>
/// Exercise 4.4. The shelter stores its animals in the BasketDictionary from
/// exercise 4, which hands them back in basket order rather than in the order
/// they arrived. Every assertion below is therefore written to be independent
/// of the order -- an assertion on a position would pass or fail depending on
/// hash codes, not on the shelter.
/// </summary>
public class ShelterTests
{
    private const string MimiChip = "276098100000001";
    private const string RexChip = "276098100000002";
    private const string FlokiChip = "276098100000003";
    private const string UnknownChip = "276098100000999";

    private static Cat Mimi() => new("Mimi", MimiChip, 36, 4.0);

    private static Dog Rex() => new("Rex", RexChip, 24, 18.0);

    private static Rabbit Floki() => new("Floki", FlokiChip, 12, 2.0);

    [Fact]
    public void Admit_NewAnimal_IsTakenInAndRaisesAnimalCount()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        Cat mimi = Mimi();

        shelter.Admit(mimi);

        Assert.Equal(1, shelter.AnimalCount);
        Assert.Same(mimi, shelter[MimiChip]);
    }

    [Fact]
    public void Admit_SecondAnimal_RaisesAnimalCountAgain()
    {
        var shelter = new Shelter("Tierheim Nord", 10);

        shelter.Admit(Mimi());
        shelter.Admit(Rex());

        Assert.Equal(2, shelter.AnimalCount);
    }

    [Fact]
    public void Admit_ChipNumberAlreadyPresent_ThrowsInvalidOperationException()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());

        // Same chip number, different species and name: the shelter identifies
        // an animal by its chip alone, so this is the same animal twice.
        var again = new Dog("Rex", MimiChip, 24, 18.0);

        Assert.Throws<InvalidOperationException>(() => shelter.Admit(again));
        Assert.Equal(1, shelter.AnimalCount);
    }

    [Fact]
    public void Admit_ShelterIsFull_ThrowsInvalidOperationException()
    {
        // Capacity 2, so the first two fit exactly and the third is one too many.
        var shelter = new Shelter("Tierheim Nord", 2);
        shelter.Admit(Mimi());
        shelter.Admit(Rex());

        Assert.Throws<InvalidOperationException>(() => shelter.Admit(Floki()));
        Assert.Equal(2, shelter.AnimalCount);
    }

    [Fact]
    public void Adopt_KnownChipNumber_ReturnsAnimalAndLowersAnimalCount()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        Cat mimi = Mimi();
        shelter.Admit(mimi);
        shelter.Admit(Rex());

        Animal adopted = shelter.Adopt(MimiChip);

        Assert.Same(mimi, adopted);
        Assert.Equal(1, shelter.AnimalCount);
        Assert.Throws<KeyNotFoundException>(() => { _ = shelter[MimiChip]; });
    }

    [Fact]
    public void Adopt_UnknownChipNumber_ThrowsKeyNotFoundException()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());

        Assert.Throws<KeyNotFoundException>(() => shelter.Adopt(UnknownChip));
        Assert.Equal(1, shelter.AnimalCount);
    }

    [Fact]
    public void Indexer_KnownChipNumber_ReturnsAnimal()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        Dog rex = Rex();
        shelter.Admit(Mimi());
        shelter.Admit(rex);

        Animal found = shelter[RexChip];

        Assert.Same(rex, found);
        Assert.Equal("Rex", found.Name);
    }

    [Fact]
    public void Indexer_UnknownChipNumber_ThrowsKeyNotFoundException()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());

        Assert.Throws<KeyNotFoundException>(() => { _ = shelter[UnknownChip]; });
    }

    [Fact]
    public void Foreach_OverShelter_SeesEveryAnimal()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());
        shelter.Admit(Rex());
        shelter.Admit(Floki());

        List<string> seen = [];

        foreach (Animal animal in shelter)
        {
            seen.Add(animal.ChipNumber);
        }

        // Sorted, because the shelter enumerates in basket order.
        seen.Sort();
        Assert.Equal([MimiChip, RexChip, FlokiChip], seen);
    }

    /// <summary>
    /// Because Shelter implements IEnumerable&lt;Animal&gt;, every extension
    /// method over a sequence is available on the shelter itself without the
    /// class offering anything for it.
    /// </summary>
    [Fact]
    public void Shelter_IsEnumerable_SoLinqWorksOnItDirectly()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());
        shelter.Admit(Rex());
        shelter.Admit(Floki());

        // Through a local, so that this stays a statement about the sequence
        // the shelter yields and not a collection-size assertion that an
        // analyzer would rewrite into Assert.Equal on AnimalCount.
        int counted = shelter.Count();
        List<Cat> cats = shelter.OfType<Cat>().ToList();

        Assert.Equal(3, counted);
        Assert.Equal(shelter.AnimalCount, counted);
        Assert.Equal([MimiChip], cats.Select(cat => cat.ChipNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NameIsEmpty_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => new Shelter(name, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Constructor_CapacityIsNotPositive_ThrowsArgumentException(int capacity)
    {
        Assert.Throws<ArgumentException>(() => new Shelter("Tierheim Nord", capacity));
    }

    [Fact]
    public void Constructor_ValidArguments_KeepsNameAndCapacity()
    {
        var shelter = new Shelter("Tierheim Nord", 25);

        Assert.Equal("Tierheim Nord", shelter.Name);
        Assert.Equal(25, shelter.Capacity);
        Assert.Equal(0, shelter.AnimalCount);
    }

    /// <summary>
    /// An adoption has to leave no trace of the animal behind. If Adopt only
    /// removed it from the dictionary and forgot the kennel, the duplicate
    /// check would still pass but the shelter would run out of kennels; if it
    /// forgot the dictionary entry, this second Admit would be rejected as a
    /// duplicate.
    /// </summary>
    [Fact]
    public void Admit_AfterTheSameAnimalWasAdopted_WorksAgain()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());
        shelter.Adopt(MimiChip);

        Cat returned = Mimi();
        shelter.Admit(returned);

        Assert.Equal(1, shelter.AnimalCount);
        Assert.Same(returned, shelter[MimiChip]);
    }
}
