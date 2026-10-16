using AnimalShelter.Core.Animals;

namespace AnimalShelter.Tests.Animals;

/// <summary>
/// Exercise Z5: the same job as exercise 3.2, done by the compiler.
///
/// Animal spells its equality out by hand -- Equals(Animal?), the
/// Equals(object?) override, GetHashCode, and the == and != operators, with
/// the null handling in each of them written out. ChipNumber declares one
/// positional parameter and gets all of it generated, plus ToString,
/// Deconstruct and the `with` expression. The tests below are the proof, one
/// generated member at a time.
///
/// The record is deliberately not used inside Animal: swapping the string for
/// it would change every constructor call in exercises 1 to 5. It stands
/// beside it as a comparison.
/// </summary>
public class ChipNumberRecordTests
{
    private const string Valid = "276098100000001";
    private const string Other = "276098100000002";

    // --- equality: generated here, hand-written in Animal ---
    //
    // Every member these three tests touch is a member exercise 3.2 had to
    // write by hand in Animal.cs: Equals(T?), Equals(object?), GetHashCode,
    // operator == and operator !=. Here the record declaration is the whole
    // implementation -- the compiler emits them from the parameter list, and
    // keeps them in step with it. Add a second parameter to the record and
    // all of them take it into account; add a field to Animal and its Equals
    // keeps answering the old question until somebody remembers to edit it.

    [Fact]
    public void TwoChipNumbers_WithTheSameValue_AreEqual()
    {
        ChipNumber first = new(Valid);
        ChipNumber second = new(Valid);

        Assert.Equal(first, second);
        Assert.NotSame(first, second);

        // The comparisons go through locals on purpose. Written inside the
        // assert, xUnit2024 would ask for Assert.Equal instead -- but the
        // generated Equals overload and the generated == operator are
        // precisely what is under test here, and Assert.Equal would reach
        // only one of them.
        bool equalsSaysSo = first.Equals(second);
        bool operatorSaysSo = first == second;
        bool differentOperatorSaysSo = first != second;
        bool objectEqualsSaysSo = first.Equals((object)second);

        Assert.True(equalsSaysSo);
        Assert.True(operatorSaysSo);
        Assert.False(differentOperatorSaysSo);
        Assert.True(objectEqualsSaysSo);
    }

    [Fact]
    public void TwoChipNumbers_WithDifferentValues_AreNotEqual()
    {
        ChipNumber first = new(Valid);
        ChipNumber second = new(Other);

        Assert.NotEqual(first, second);

        bool equalsSaysSo = first.Equals(second);
        bool operatorSaysSo = first == second;

        Assert.False(equalsSaysSo);
        Assert.False(operatorSaysSo);
    }

    [Fact]
    public void TwoChipNumbers_WithTheSameValue_HaveTheSameHashCode()
    {
        ChipNumber first = new(Valid);
        ChipNumber second = new(Valid);

        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void HashSet_WithTwoEqualChipNumbers_HoldsOne()
    {
        // The generated Equals and GetHashCode agree with each other, so a
        // hash set treats the two as one entry. This is the same contract
        // Animal has to honour by hand, and the same test that would fail if
        // only one of the two members were written.
        HashSet<ChipNumber> set = [new ChipNumber(Valid), new ChipNumber(Valid)];

        Assert.Single(set);
        Assert.Contains(new ChipNumber(Valid), set);
    }

    // --- the members Animal does not have at all ---

    [Fact]
    public void With_ProducesACopyAndLeavesTheOriginalAlone()
    {
        ChipNumber original = new(Valid);

        ChipNumber copy = original with { Value = Other };

        Assert.Equal(Other, copy.Value);
        Assert.Equal(Valid, original.Value);
        Assert.NotEqual(original, copy);
        Assert.NotSame(original, copy);
    }

    [Fact]
    public void With_WithNoChangeAtAll_ProducesAnEqualCopy()
    {
        ChipNumber original = new(Valid);

        ChipNumber copy = original with { };

        Assert.Equal(original, copy);
        Assert.NotSame(original, copy);
    }

    [Fact]
    public void Deconstruct_HandsBackThePositionalParameter()
    {
        ChipNumber chip = new(Valid);

        // `var (value) = chip;` does not compile: a deconstruction needs at
        // least two variables, and this record has one positional parameter.
        // The generated Deconstruct is still there and still takes one out
        // argument, so it is called directly.
        chip.Deconstruct(out string value);

        Assert.Equal(Valid, value);
    }

    [Fact]
    public void ToString_ReturnsTheBareValue()
    {
        ChipNumber chip = new(Valid);

        // Without the override a record prints "ChipNumber { Value = ... }".
        // The class overrides ToString, so the generated version is replaced
        // by the plain number.
        Assert.Equal(Valid, chip.ToString());
        Assert.DoesNotContain("ChipNumber", chip.ToString());
    }

    // --- Parse: the one piece that is not generated ---

    [Fact]
    public void Parse_WithFifteenDigits_ReturnsTheValue()
    {
        ChipNumber chip = ChipNumber.Parse(Valid);

        Assert.Equal(Valid, chip.Value);
        Assert.Equal(new ChipNumber(Valid), chip);
        Assert.Equal(15, ChipNumber.Length);
    }

    [Theory]
    [InlineData("27609810000000")]           // fourteen digits, one too few
    [InlineData("2760981000000012")]         // sixteen, one too many
    [InlineData("27609810000000X")]          // right length, a letter in it
    [InlineData("276098 00000001")]          // right length, a space in it
    [InlineData("")]                         // nothing at all
    [InlineData(null)]                       // no string at all
    public void Parse_WithAnythingButFifteenDigits_Throws(string? value)
    {
        // The parameter is nullable so that the null case can be written as
        // InlineData; Parse itself declares it non-nullable and checks for
        // null anyway, which is why the call suppresses the warning.
        Assert.Throws<ArgumentException>(() => ChipNumber.Parse(value!));
    }
}
