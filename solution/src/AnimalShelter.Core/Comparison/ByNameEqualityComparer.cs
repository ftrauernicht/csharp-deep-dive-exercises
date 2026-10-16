using AnimalShelter.Core.Animals;

namespace AnimalShelter.Core.Comparison;

/// <summary>
/// Exercise Z1. Treats two animals as the same when their names match, case
/// ignored -- a different answer than Animal.Equals, which only looks at the
/// chip number. Handing this to a HashSet or to Distinct changes what
/// "already present" means without touching Animal at all.
/// </summary>
public class ByNameEqualityComparer : IEqualityComparer<Animal>
{
    public bool Equals(Animal? x, Animal? y)
    {
        if (x is null || y is null)
        {
            return ReferenceEquals(x, y);
        }

        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(Animal obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
    }
}
