using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Collections;
using AnimalShelter.Core.Management;
using AnimalShelter.Core.Queries;

namespace AnimalShelter.Tests.Queries;

/// <summary>
/// Exercise 5.1: every query in ShelterReport, once against a populated
/// shelter and once against an empty one. The empty case is where the
/// difference between Single and First, Max and DefaultIfEmpty shows up, so
/// it gets its own test for each method rather than a footnote.
/// </summary>
public class ShelterReportTests
{
    private const string MimiChip = "276098100000001";
    private const string BrunoChip = "276098100000002";
    private const string RexChip = "276098100000003";
    private const string HoppelChip = "276098100000004";
    private const string TweetyChip = "276098100000005";

    /// <summary>
    /// Mimi (indoor cat, 24 months, 4 kg), bruno (outdoor kitten, 6 months,
    /// 1.5 kg), Rex (dog, 36 months, 30 kg), Hoppel (rabbit, 18 months, 2 kg)
    /// and Tweety (budgie, 12 months, 0.05 kg). The lower-case name is on
    /// purpose: it is what makes the case-insensitive sort visible.
    /// </summary>
    private static Shelter FilledShelter()
    {
        Shelter shelter = new("Test", 10);
        shelter.Admit(new Cat("Mimi", MimiChip, 24, 4.0));
        shelter.Admit(new Cat("bruno", BrunoChip, 6, 1.5, isOutdoor: true));
        shelter.Admit(new Dog("Rex", RexChip, 36, 30.0, "Mixed"));
        shelter.Admit(new Rabbit("Hoppel", HoppelChip, 18, 2.0));
        shelter.Admit(new Budgie("Tweety", TweetyChip, 12, 0.05));
        return shelter;
    }

    private static ShelterReport FilledReport() => new(FilledShelter());

    private static ShelterReport EmptyReport() => new(new Shelter("Test", 5));

    // --- Cats() ---

    [Fact]
    public void Cats_FilledShelter_ReturnsOnlyTheCats()
    {
        ShelterReport report = FilledReport();

        List<string> chips = report.Cats()
            .Select(cat => cat.ChipNumber)
            .OrderBy(chip => chip, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(new[] { MimiChip, BrunoChip }, chips);
    }

    [Fact]
    public void Cats_EmptyShelter_ReturnsNothing()
    {
        Assert.Empty(EmptyReport().Cats());
    }

    // --- Kittens() ---

    [Fact]
    public void Kittens_FilledShelter_ReturnsOnlyCatsUnderTwelveMonths()
    {
        ShelterReport report = FilledReport();

        Cat kitten = Assert.Single(report.Kittens());

        Assert.Equal(BrunoChip, kitten.ChipNumber);
    }

    [Fact]
    public void Kittens_CatExactlyTwelveMonths_IsNotAKitten()
    {
        Shelter shelter = new("Test", 5);
        shelter.Admit(new Cat("Grenzfall", MimiChip, 12, 3.0));

        ShelterReport report = new(shelter);

        // The query asks for AgeInMonths < 12, so twelve months is already outside.
        Assert.Empty(report.Kittens());
    }

    [Fact]
    public void Kittens_EmptyShelter_ReturnsNothing()
    {
        Assert.Empty(EmptyReport().Kittens());
    }

    // --- NamesSorted() ---

    [Fact]
    public void NamesSorted_FilledShelter_SortsByNameIgnoringCase()
    {
        ShelterReport report = FilledReport();

        List<string> names = report.NamesSorted().ToList();

        // "bruno" leads because Animal.CompareTo compares names with
        // OrdinalIgnoreCase; an ordinal sort would have put it last.
        Assert.Equal(new[] { "bruno", "Hoppel", "Mimi", "Rex", "Tweety" }, names);
    }

    [Fact]
    public void NamesSorted_SameNameDifferentCase_FallsBackToChipNumber()
    {
        Shelter shelter = new("Test", 5);
        shelter.Admit(new Cat("Luna", HoppelChip, 24, 4.0));
        shelter.Admit(new Dog("luna", RexChip, 24, 10.0));

        ShelterReport report = new(shelter);

        // Names tie, so CompareTo moves on to the chip number: 003 before 004.
        Assert.Equal(new[] { "luna", "Luna" }, report.NamesSorted());
    }

    [Fact]
    public void NamesSorted_EmptyShelter_ReturnsNothing()
    {
        Assert.Empty(EmptyReport().NamesSorted());
    }

    // --- TotalDailyFoodInKg() ---

    [Fact]
    public void TotalDailyFoodInKg_FilledShelter_AddsUpEverySpeciesRule()
    {
        ShelterReport report = FilledReport();

        // Mimi 4 kg * 40 = 160 g, bruno 1.5 kg * 40 doubled below twelve months = 120 g,
        // Rex 30 kg * 30 = 900 g, Hoppel 2 kg * 50 = 100 g, Tweety a flat 10 g.
        Assert.Equal(1.29, report.TotalDailyFoodInKg(), 6);
    }

    [Fact]
    public void TotalDailyFoodInKg_EmptyShelter_ReturnsZero()
    {
        Assert.Equal(0.0, EmptyReport().TotalDailyFoodInKg(), 6);
    }

    // --- CountBySpecies() ---

    [Fact]
    public void CountBySpecies_FilledShelter_CountsPerRuntimeType()
    {
        ShelterReport report = FilledReport();

        Dictionary<string, int> counts = report.CountBySpecies();

        int species = counts.Count;
        Assert.Equal(4, species);
        Assert.Equal(2, counts["Cat"]);
        Assert.Equal(1, counts["Dog"]);
        Assert.Equal(1, counts["Rabbit"]);
        Assert.Equal(1, counts["Budgie"]);
    }

    [Fact]
    public void CountBySpecies_EmptyShelter_ReturnsEmptyDictionary()
    {
        Assert.Empty(EmptyReport().CountBySpecies());
    }

    // --- NotFullyVaccinated() ---

    [Fact]
    public void NotFullyVaccinated_FilledShelter_SkipsTheBudgie()
    {
        ShelterReport report = FilledReport();

        List<string> chips = report.NotFullyVaccinated()
            .Select(animal => animal.ChipNumber)
            .OrderBy(chip => chip, StringComparer.Ordinal)
            .ToList();

        // Tweety is missing because a Budgie does not implement IVaccinatable,
        // not because the query names the species.
        Assert.Equal(new[] { MimiChip, BrunoChip, RexChip, HoppelChip }, chips);
    }

    [Fact]
    public void NotFullyVaccinated_EverythingGiven_ReturnsNothing()
    {
        Cat cat = new("Mimi", MimiChip, 24, 4.0);
        cat.Vaccinate("CatFlu");
        cat.Vaccinate("Panleukopenia");

        Shelter shelter = new("Test", 5);
        shelter.Admit(cat);

        ShelterReport report = new(shelter);

        Assert.Empty(report.NotFullyVaccinated());
    }

    [Fact]
    public void NotFullyVaccinated_EmptyShelter_ReturnsNothing()
    {
        Assert.Empty(EmptyReport().NotFullyVaccinated());
    }

    // --- Oldest() ---

    [Fact]
    public void Oldest_FilledShelter_ReturnsTheHighestAge()
    {
        ShelterReport report = FilledReport();

        Animal? oldest = report.Oldest();

        Assert.NotNull(oldest);
        Assert.Equal(RexChip, oldest.ChipNumber);
        Assert.Equal(36, oldest.AgeInMonths);
    }

    [Fact]
    public void Oldest_EmptyShelter_ReturnsNull()
    {
        // FirstOrDefault, not First: an empty shelter has no oldest animal and
        // that is an answer, not an error.
        Assert.Null(EmptyReport().Oldest());
    }

    // --- HeaviestThree() ---

    [Fact]
    public void HeaviestThree_FilledShelter_ReturnsThreeHeaviestDescending()
    {
        ShelterReport report = FilledReport();

        List<string> names = report.HeaviestThree().Select(animal => animal.Name).ToList();

        Assert.Equal(new[] { "Rex", "Mimi", "Hoppel" }, names);
    }

    [Fact]
    public void HeaviestThree_FewerThanThreeAnimals_ReturnsWhatIsThere()
    {
        Shelter shelter = new("Test", 5);
        shelter.Admit(new Cat("Mimi", MimiChip, 24, 4.0));
        shelter.Admit(new Rabbit("Hoppel", HoppelChip, 18, 2.0));

        ShelterReport report = new(shelter);

        // Take(3) on two elements yields two. It does not throw and it does
        // not pad -- that is the difference between Take and an index.
        Assert.Equal(new[] { "Mimi", "Hoppel" }, report.HeaviestThree().Select(a => a.Name));
    }

    [Fact]
    public void HeaviestThree_EmptyShelter_ReturnsNothing()
    {
        Assert.Empty(EmptyReport().HeaviestThree());
    }

    // --- HasOutdoorCats() ---

    [Fact]
    public void HasOutdoorCats_FilledShelter_ReturnsTrue()
    {
        Assert.True(FilledReport().HasOutdoorCats());
    }

    [Fact]
    public void HasOutdoorCats_OnlyIndoorCats_ReturnsFalse()
    {
        Shelter shelter = new("Test", 5);
        shelter.Admit(new Cat("Mimi", MimiChip, 24, 4.0));
        shelter.Admit(new Dog("Rex", RexChip, 36, 30.0));

        ShelterReport report = new(shelter);

        Assert.False(report.HasOutdoorCats());
    }

    [Fact]
    public void HasOutdoorCats_EmptyShelter_ReturnsFalse()
    {
        // Any() over nothing is false, so no special case is needed in the query.
        Assert.False(EmptyReport().HasOutdoorCats());
    }

    // --- LINQ straight onto the hand-built dictionary ---

    [Fact]
    public void BasketDictionary_QueriedWithLinq_FiltersAndProjectsKeyValuePairs()
    {
        BasketDictionary<string, int> perSpecies = new();
        perSpecies.Add("Cat", 2);
        perSpecies.Add("Dog", 1);
        perSpecies.Add("Rabbit", 3);

        // BasketDictionary implements IEnumerable<KeyValuePair<string, int>>, and
        // that single interface is all LINQ needs. OrderBy is in the chain because
        // basket order follows the hash code and is nothing to rely on.
        List<string> crowded = perSpecies
            .Where(pair => pair.Value > 1)
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => pair.Key)
            .ToList();

        Assert.Equal(new[] { "Cat", "Rabbit" }, crowded);

        int total = perSpecies.Sum(pair => pair.Value);
        Assert.Equal(6, total);
    }
}
