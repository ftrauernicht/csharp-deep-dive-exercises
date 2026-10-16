namespace AnimalShelter.Core.Animals;

/// <summary>
/// Exercise Z5. A record that stands beside Animal rather than inside it.
///
/// Swapping the plain string in Animal for this would change every
/// constructor in exercises 1 to 5, so the solution keeps the string and uses
/// this to show what the compiler writes for you: Equals, GetHashCode,
/// ToString, Deconstruct, the == operator and a `with` expression -- the whole
/// of exercise 3.2, generated. Compare it with what Animal needed by hand.
/// </summary>
public record ChipNumber(string Value)
{
    public const int Length = 15;

    public static ChipNumber Parse(string value)
    {
        if (value is null || value.Length != Length || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                $"Chip number must consist of exactly {Length} digits.", nameof(value));
        }

        return new ChipNumber(value);
    }

    public override string ToString() => Value;
}
