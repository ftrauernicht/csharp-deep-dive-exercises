namespace AnimalShelter.Core.Animals;

/// <summary>
/// Base class for every animal in the shelter.
///
/// Identity is the chip number and nothing else: a chip number is globally
/// unique by design, so two records carrying the same one describe the same
/// animal. That holds across species -- a Cat and a Dog with the same chip
/// number are a data error, not two animals, and treating them as equal is
/// what makes the error visible.
/// </summary>
public abstract class Animal : IEquatable<Animal>, IComparable<Animal>
{
    private const int ChipNumberLength = 15;

    protected Animal(string name, string chipNumber, int ageInMonths, double weightInKg)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must not be empty.", nameof(name));
        }

        if (chipNumber is null
            || chipNumber.Length != ChipNumberLength
            || !chipNumber.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                $"Chip number must consist of exactly {ChipNumberLength} digits.",
                nameof(chipNumber));
        }

        if (ageInMonths < 0)
        {
            throw new ArgumentException("Age must not be negative.", nameof(ageInMonths));
        }

        if (weightInKg <= 0)
        {
            throw new ArgumentException("Weight must be greater than 0.", nameof(weightInKg));
        }

        Name = name;
        ChipNumber = chipNumber;
        AgeInMonths = ageInMonths;
        WeightInKg = weightInKg;
    }

    public string Name { get; set; }

    public string ChipNumber { get; }

    public int AgeInMonths { get; set; }

    public double WeightInKg { get; set; }

    public abstract string MakeSound();

    /// <summary>Daily food in grams. 30 g per kilogram unless a species says otherwise.</summary>
    public virtual double CalculateDailyFoodInGrams() => WeightInKg * 30;

    /// <summary>
    /// What treating this animal costs. This is what exercise 2.1 moves out of
    /// Veterinarian, and it is abstract rather than virtual on purpose: a new
    /// species cannot be added without someone deciding its price. The old
    /// version answered that question with a NotSupportedException at runtime.
    /// </summary>
    public abstract decimal TreatmentCostInEuro { get; }

    public virtual string Describe() => $"{Name}, {AgeInMonths} months";

    public override string ToString() => Describe();

    // --- Equality (exercise 3.2) ---

    public bool Equals(Animal? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || ChipNumber == other.ChipNumber;
    }

    public override bool Equals(object? obj) => Equals(obj as Animal);

    public override int GetHashCode() => ChipNumber.GetHashCode();

    public static bool operator ==(Animal? left, Animal? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Animal? left, Animal? right) => !(left == right);

    // --- Ordering (exercise 3.4) ---

    /// <summary>
    /// Sorts by name, ignoring case, then by chip number.
    ///
    /// The first check is not decoration. The task demands that CompareTo
    /// returns 0 exactly when Equals is true, and Equals only looks at the
    /// chip number -- so two records with the same chip and different names
    /// are equal. Comparing the names first would return non-zero for them
    /// and break that promise. Asking Equals first is what keeps it.
    ///
    /// It costs something, and the price is written down rather than hidden:
    /// the comparison is no longer transitive. For a("Anna", chip X),
    /// b("Zoe", chip X) and c("Mia", chip Y) it says a == b, a &lt; c and
    /// c &lt; b. That holds only because two animals with one chip number are
    /// a pair that cannot exist: a chip number is unique. The assumption is
    /// the whole justification, so it belongs here in writing.
    /// CompareTo_WithTwoNamesForOneChip_IsNotTransitive pins the effect down.
    /// </summary>
    public int CompareTo(Animal? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (Equals(other))
        {
            return 0;
        }

        int byName = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        return byName != 0
            ? byName
            : string.CompareOrdinal(ChipNumber, other.ChipNumber);
    }

    public static bool operator <(Animal? left, Animal? right) => Compare(left, right) < 0;

    public static bool operator >(Animal? left, Animal? right) => Compare(left, right) > 0;

    public static bool operator <=(Animal? left, Animal? right) => Compare(left, right) <= 0;

    public static bool operator >=(Animal? left, Animal? right) => Compare(left, right) >= 0;

    private static int Compare(Animal? left, Animal? right)
    {
        if (left is null)
        {
            return right is null ? 0 : -1;
        }

        return left.CompareTo(right);
    }
}
