using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Management;

namespace AnimalShelter.Core.Queries;

/// <summary>
/// Everything the shelter can be asked about, in LINQ method syntax.
///
/// Note what these methods return. The ones typed IEnumerable hand back a
/// query, not a result: nothing has been computed when the method returns,
/// and the shelter is read when somebody enumerates it. The ones typed double,
/// bool, Animal? or Dictionary have already run. Exercise 5.2 is about
/// noticing the difference.
/// </summary>
public class ShelterReport
{
    private readonly Shelter shelter;

    public ShelterReport(Shelter shelter)
    {
        this.shelter = shelter ?? throw new ArgumentNullException(nameof(shelter));
    }

    public IEnumerable<Cat> Cats() => shelter.OfType<Cat>();

    public IEnumerable<Cat> Kittens() => shelter.OfType<Cat>().Where(cat => cat.AgeInMonths < 12);

    /// <summary>Uses Animal.CompareTo from exercise 3.4, not a lambda.</summary>
    public IEnumerable<string> NamesSorted() => shelter.OrderBy(animal => animal).Select(a => a.Name);

    public double TotalDailyFoodInKg() =>
        shelter.Sum(animal => animal.CalculateDailyFoodInGrams()) / 1000.0;

    public Dictionary<string, int> CountBySpecies() =>
        shelter.GroupBy(animal => animal.GetType().Name)
               .ToDictionary(group => group.Key, group => group.Count());

    public IEnumerable<Animal> NotFullyVaccinated() =>
        shelter.OfType<IVaccinatable>()
               .Where(v => !v.IsFullyVaccinated)
               .Cast<Animal>();

    public Animal? Oldest() =>
        shelter.OrderByDescending(animal => animal.AgeInMonths).FirstOrDefault();

    public IEnumerable<Animal> HeaviestThree() =>
        shelter.OrderByDescending(animal => animal.WeightInKg).Take(3);

    public bool HasOutdoorCats() => shelter.OfType<Cat>().Any(cat => cat.IsOutdoor);
}
