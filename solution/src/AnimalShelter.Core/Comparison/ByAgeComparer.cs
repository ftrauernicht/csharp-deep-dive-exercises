using AnimalShelter.Core.Animals;

namespace AnimalShelter.Core.Comparison;

/// <summary>Youngest first. Exercise 3.5.</summary>
public class ByAgeComparer : IComparer<Animal>
{
    public int Compare(Animal? x, Animal? y)
    {
        if (x is null)
        {
            return y is null ? 0 : -1;
        }

        if (y is null)
        {
            return 1;
        }

        return x.AgeInMonths.CompareTo(y.AgeInMonths);
    }
}
