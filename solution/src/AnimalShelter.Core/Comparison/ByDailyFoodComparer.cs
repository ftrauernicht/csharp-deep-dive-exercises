using AnimalShelter.Core.Animals;

namespace AnimalShelter.Core.Comparison;

/// <summary>Hungriest first. Exercise 3.5.</summary>
public class ByDailyFoodComparer : IComparer<Animal>
{
    public int Compare(Animal? x, Animal? y)
    {
        if (x is null)
        {
            return y is null ? 0 : 1;
        }

        if (y is null)
        {
            return -1;
        }

        return y.CalculateDailyFoodInGrams().CompareTo(x.CalculateDailyFoodInGrams());
    }
}
