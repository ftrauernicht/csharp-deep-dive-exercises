using AnimalShelter.Core.Animals;

namespace AnimalShelter.Tests.Comparison;

/// <summary>Exercise 3.1 and 3.2.</summary>
public class EqualityTests
{
    private const string Chip = "276098100000001";
    private const string OtherChip = "276098100000002";

    [Fact]
    public void Prediction_TwoRecordsOfTheSameAnimal()
    {
        Animal a = new Cat("Mimi", Chip, 36, 4.0);
        Animal b = new Cat("Mimi", Chip, 36, 4.0);

        // Prediction before 3.2 was implemented: both false, and the set holds 2.
        // After 3.2 the chip number decides, so:
        // Prediction: a == b is true, because operator== now calls Equals.
        Assert.True(a == b);

        // Prediction: a.Equals(b) is true.
        Assert.True(a.Equals(b));

        // Prediction: the set holds 1 -- equal objects with equal hash codes
        // land in the same bucket and the second one is recognised as present.
        HashSet<Animal> set = [a, b];
        Assert.Single(set);
    }

    [Fact]
    public void Equals_IsReflexive()
    {
        Animal a = new Dog("Rex", Chip, 24, 18.0);
        Assert.True(a.Equals(a));
    }

    [Fact]
    public void Equals_IsSymmetric()
    {
        Animal a = new Dog("Rex", Chip, 24, 18.0);
        Animal b = new Dog("Rex", Chip, 24, 18.0);

        Assert.True(a.Equals(b));
        Assert.True(b.Equals(a));
    }

    [Fact]
    public void Equals_IsTransitive()
    {
        Animal a = new Dog("Rex", Chip, 24, 18.0);
        Animal b = new Dog("Rex", Chip, 24, 18.0);
        Animal c = new Dog("Rex", Chip, 24, 18.0);

        Assert.True(a.Equals(b));
        Assert.True(b.Equals(c));
        Assert.True(a.Equals(c));
    }

    [Fact]
    public void Equals_Null_IsFalse()
    {
        Animal a = new Cat("Mimi", Chip, 36, 4.0);

        Assert.False(a.Equals(null));
        Assert.False(a.Equals((object?)null));
    }

    [Fact]
    public void Operators_HandleNullOnEitherSide()
    {
        Animal? nothing = null;
        Animal? alsoNothing = null;
        Animal a = new Cat("Mimi", Chip, 36, 4.0);

        Assert.True(nothing == alsoNothing);
        Assert.False(a == nothing);
        Assert.False(nothing == a);
        Assert.True(a != nothing);
    }

    [Fact]
    public void GetHashCode_IsEqualForEqualAnimals()
    {
        Animal a = new Cat("Mimi", Chip, 36, 4.0);
        Animal b = new Cat("Minka", Chip, 12, 3.2);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Equals_SameChipDifferentName_IsEqual()
    {
        Animal a = new Cat("Mimi", Chip, 36, 4.0);
        Animal b = new Cat("Minka", Chip, 36, 4.0);

        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_SameNameDifferentChip_IsNotEqual()
    {
        Animal a = new Cat("Mimi", Chip, 36, 4.0);
        Animal b = new Cat("Mimi", OtherChip, 36, 4.0);

        Assert.False(a.Equals(b));
    }

    /// <summary>
    /// The decision the task asks you to make and write down: the chip number
    /// is the identity, so the species does not enter into it. A cat and a dog
    /// carrying the same chip are one animal recorded twice, and that is a
    /// mistake worth surfacing rather than hiding.
    /// </summary>
    [Fact]
    public void Equals_SameChipDifferentSpecies_IsEqual()
    {
        Animal cat = new Cat("Mimi", Chip, 36, 4.0);
        Animal dog = new Dog("Rex", Chip, 36, 18.0);

        Assert.True(cat.Equals(dog));
        Assert.Equal(cat.GetHashCode(), dog.GetHashCode());
    }
}
