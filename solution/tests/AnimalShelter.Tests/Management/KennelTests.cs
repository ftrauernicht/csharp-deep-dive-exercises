using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Management;

namespace AnimalShelter.Tests.Management;

/// <summary>
/// Exercise Z6. Kennels are numbered from 1 and handed out in order of
/// arrival, and the shelter carries two indexers that are told apart by the
/// type inside the brackets alone.
/// </summary>
public class KennelTests
{
    private const string MimiChip = "276098100000001";
    private const string RexChip = "276098100000002";
    private const string FlokiChip = "276098100000003";
    private const string UnknownChip = "276098100000999";

    private static Cat Mimi() => new("Mimi", MimiChip, 36, 4.0);

    private static Dog Rex() => new("Rex", RexChip, 24, 18.0);

    private static Rabbit Floki() => new("Floki", FlokiChip, 12, 2.0);

    [Fact]
    public void Admit_FirstAndSecondAnimal_GetKennelOneAndTwo()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        Cat mimi = Mimi();
        Dog rex = Rex();

        shelter.Admit(mimi);
        shelter.Admit(rex);

        Assert.Same(mimi, shelter[1]);
        Assert.Same(rex, shelter[2]);
    }

    /// <summary>
    /// The point of the exercise: both calls below are written as
    /// shelter[...], and nothing but the type in the brackets decides which
    /// indexer runs. The chip number is a string of digits, so the difference
    /// is not even visible in the characters -- only in the quotes.
    /// </summary>
    [Fact]
    public void Indexers_IntAndString_AreToldApartByTheTypeInTheBrackets()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());

        Animal byKennel = shelter[1];
        Animal byChipNumber = shelter["276098100000001"];

        Assert.Same(byChipNumber, byKennel);

        // And the other way round: kennel 1 is not the chip number 1, and the
        // chip number is not a kennel.
        Assert.Equal(1, shelter.KennelOf(byKennel.ChipNumber));
    }

    [Fact]
    public void Indexer_EmptyKennel_ThrowsKeyNotFoundException()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());

        Assert.Throws<KeyNotFoundException>(() => { _ = shelter[99]; });
    }

    /// <summary>
    /// Kennel numbers are reused, not counted up forever: the shelter always
    /// takes the lowest free one, so the kennel an adopted animal leaves
    /// behind is the next one handed out.
    /// </summary>
    [Fact]
    public void Admit_AfterAnAdoption_ReusesTheFreedKennel()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());
        shelter.Admit(Rex());

        shelter.Adopt(MimiChip);

        Rabbit floki = Floki();
        shelter.Admit(floki);

        Assert.Equal(1, shelter.KennelOf(FlokiChip));
        Assert.Same(floki, shelter[1]);

        // Rex never moved while all this happened.
        Assert.Equal(2, shelter.KennelOf(RexChip));
    }

    [Fact]
    public void Adopt_FreesTheKennel_SoTheIndexerNoLongerFindsIt()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());

        shelter.Adopt(MimiChip);

        Assert.Throws<KeyNotFoundException>(() => { _ = shelter[1]; });
    }

    [Fact]
    public void KennelOf_KnownChipNumber_ReturnsTheKennel()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());
        shelter.Admit(Rex());
        shelter.Admit(Floki());

        Assert.Equal(1, shelter.KennelOf(MimiChip));
        Assert.Equal(2, shelter.KennelOf(RexChip));
        Assert.Equal(3, shelter.KennelOf(FlokiChip));
    }

    [Fact]
    public void KennelOf_UnknownChipNumber_ThrowsKeyNotFoundException()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());

        Assert.Throws<KeyNotFoundException>(() => shelter.KennelOf(UnknownChip));
    }

    [Fact]
    public void KennelOf_AnAdoptedAnimal_ThrowsKeyNotFoundException()
    {
        var shelter = new Shelter("Tierheim Nord", 10);
        shelter.Admit(Mimi());
        shelter.Adopt(MimiChip);

        Assert.Throws<KeyNotFoundException>(() => shelter.KennelOf(MimiChip));
    }
}
