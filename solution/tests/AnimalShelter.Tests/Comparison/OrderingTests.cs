using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Comparison;

namespace AnimalShelter.Tests.Comparison;

/// <summary>Exercise 3.4 and 3.5.</summary>
public class OrderingTests
{
    private static Cat Cat(string name, string chip, int age = 36, double kg = 4.0) =>
        new(name, chip, age, kg);

    [Fact]
    public void Sort_OrdersByNameIgnoringCase()
    {
        List<Animal> animals =
        [
            Cat("zoe", "276098100000003"),
            new Dog("Anton", "276098100000001", 24, 18.0),
            Cat("Mimi", "276098100000002"),
        ];

        animals.Sort();

        Assert.Equal(["Anton", "Mimi", "zoe"], animals.Select(a => a.Name));
    }

    [Fact]
    public void CompareTo_SameName_FallsBackToChipNumber()
    {
        Animal first = Cat("Mimi", "276098100000001");
        Animal second = Cat("Mimi", "276098100000002");

        Assert.True(first.CompareTo(second) < 0);
        Assert.True(second.CompareTo(first) > 0);
    }

    [Fact]
    public void CompareTo_IsAntisymmetric()
    {
        Animal a = Cat("Anton", "276098100000001");
        Animal b = Cat("Berta", "276098100000002");

        Assert.True(Math.Sign(a.CompareTo(b)) == -Math.Sign(b.CompareTo(a)));
    }

    [Fact]
    public void CompareTo_Null_IsPositive()
    {
        Animal a = Cat("Mimi", "276098100000001");
        Assert.True(a.CompareTo(null) > 0);
    }

    /// <summary>
    /// The consistency the task demands, and the reason Animal.CompareTo asks
    /// Equals first. Equality is the chip number alone, so these two are equal
    /// although their names differ -- a comparison that started with the name
    /// would answer non-zero here and break the rule.
    /// </summary>
    [Fact]
    public void CompareTo_IsZero_ExactlyWhenEqual()
    {
        Animal sameChipOtherName = Cat("Minka", "276098100000001");
        Animal reference = Cat("Mimi", "276098100000001");
        Animal different = Cat("Mimi", "276098100000002");

        Assert.True(reference.Equals(sameChipOtherName));
        Assert.Equal(0, reference.CompareTo(sameChipOtherName));

        Assert.False(reference.Equals(different));
        Assert.NotEqual(0, reference.CompareTo(different));
    }

    /// <summary>
    /// The price of the line above, measured instead of argued about.
    ///
    /// Asking Equals first buys "zero exactly when equal" and gives up
    /// transitivity: a and b are equal, a sorts before c, and c sorts before
    /// b. Sort() does not complain about that. Reshuffle a few thousand
    /// animals where every chip number appears twice and sort them again and
    /// again: almost every new input order produces a new result, and Sort()
    /// never throws. How many distinct results come out depends on the names
    /// and the shuffle, so the exercise text states no figure for it. This
    /// test keeps the smallest reproducible case on record, so that a later
    /// change to CompareTo has to decide about this again rather than
    /// discover it.
    /// </summary>
    [Fact]
    public void CompareTo_WithTwoNamesForOneChip_IsNotTransitive()
    {
        Animal a = Cat("Anna", "276098100000001");
        Animal b = Cat("Zoe", "276098100000001");   // same chip as a
        Animal c = Cat("Mia", "276098100000002");

        // Prediction: 0, because equality is the chip number alone.
        Assert.Equal(0, a.CompareTo(b));

        // Prediction: negative both times. Anna before Mia, Mia before Zoe.
        Assert.True(a.CompareTo(c) < 0);
        Assert.True(c.CompareTo(b) < 0);

        // Which is the contradiction: a equals b, and a is smaller than
        // something smaller than b. Three animals, no total order.
        Assert.True(a.CompareTo(b) == 0 && a.CompareTo(c) < 0 && c.CompareTo(b) < 0);

        // And Sort() stays quiet about it. The same three animals in two
        // different input orders come out differently, and the second result
        // is not even sorted by name.
        List<Animal> oneWay = [a, b, c];
        List<Animal> otherWay = [b, a, c];
        oneWay.Sort();
        otherWay.Sort();

        Assert.Equal(["Anna", "Mia", "Zoe"], oneWay.Select(x => x.Name));
        Assert.Equal(["Mia", "Anna", "Zoe"], otherWay.Select(x => x.Name));
    }

    [Fact]
    public void Operators_CompareByOrder()
    {
        Animal anton = Cat("Anton", "276098100000001");
        Animal berta = Cat("Berta", "276098100000002");

        Assert.True(anton < berta);
        Assert.True(berta > anton);
        Assert.True(anton <= berta);
        Assert.True(berta >= anton);
    }

    [Fact]
    public void ByAgeComparer_PutsYoungestFirst()
    {
        List<Animal> animals =
        [
            Cat("Alt", "276098100000001", age: 120),
            Cat("Jung", "276098100000002", age: 3),
            Cat("Mittel", "276098100000003", age: 40),
        ];

        animals.Sort(new ByAgeComparer());

        Assert.Equal(["Jung", "Mittel", "Alt"], animals.Select(a => a.Name));
    }

    [Fact]
    public void ByDailyFoodComparer_PutsHungriestFirst()
    {
        // 4 kg adult cat: 160 g. 3 kg rabbit: 150 g. 20 kg dog: 600 g.
        List<Animal> animals =
        [
            Cat("Katze", "276098100000001", kg: 4.0),
            new Rabbit("Hase", "276098100000002", 24, 3.0),
            new Dog("Hund", "276098100000003", 24, 20.0),
        ];

        animals.Sort(new ByDailyFoodComparer());

        Assert.Equal(["Hund", "Katze", "Hase"], animals.Select(a => a.Name));
    }
}
