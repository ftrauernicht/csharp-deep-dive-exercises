using AnimalShelter.Core.Animals;

namespace AnimalShelter.Tests.Animals;

/// <summary>
/// Every rule enforced by the constructor of the abstract Animal class. Animal
/// cannot be instantiated, so each case goes through Cat as a stand-in -- the
/// rules live in the base constructor and apply to every species alike.
/// </summary>
public class AnimalValidationTests
{
    private const string ValidChipNumber = "276098100000001";

    [Fact]
    public void Constructor_AllValuesValid_StoresThem()
    {
        // The anchor for every negative case below: they differ in one value only.
        Animal cat = new Cat("Mimi", ValidChipNumber, 36, 4.0);

        Assert.Equal("Mimi", cat.Name);
        Assert.Equal(ValidChipNumber, cat.ChipNumber);
        Assert.Equal(36, cat.AgeInMonths);
        Assert.Equal(4.0, cat.WeightInKg);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NameIsEmpty_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => new Cat(name, ValidChipNumber, 36, 4.0));
    }

    [Theory]
    [InlineData("27609810000000")]     // 14 digits, one too few
    [InlineData("2760981000000012")]   // 16 digits, one too many
    [InlineData("27609810000000X")]    // 15 characters, but the last one is a letter
    public void Constructor_ChipNumberIsNotFifteenDigits_ThrowsArgumentException(string chipNumber)
    {
        Assert.Throws<ArgumentException>(() => new Cat("Mimi", chipNumber, 36, 4.0));
    }

    [Fact]
    public void Constructor_ChipNumberIsNull_ThrowsArgumentException()
    {
        // The null check sits in the same rule as the length check, so it raises
        // the same exception type rather than an ArgumentNullException.
        Assert.Throws<ArgumentException>(() => new Cat("Mimi", null!, 36, 4.0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-36)]
    public void Constructor_AgeIsNegative_ThrowsArgumentException(int ageInMonths)
    {
        Assert.Throws<ArgumentException>(() => new Cat("Mimi", ValidChipNumber, ageInMonths, 4.0));
    }

    [Fact]
    public void Constructor_AgeIsZero_IsAccepted()
    {
        // A newborn is zero months old. Only negative ages are rejected.
        Animal cat = new Cat("Mimi", ValidChipNumber, 0, 0.5);

        Assert.Equal(0, cat.AgeInMonths);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.5)]
    public void Constructor_WeightIsNotGreaterThanZero_ThrowsArgumentException(double weightInKg)
    {
        Assert.Throws<ArgumentException>(() => new Cat("Mimi", ValidChipNumber, 36, weightInKg));
    }
}
