using System.Collections;
using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Collections;

namespace AnimalShelter.Core.Management;

/// <summary>
/// The shelter itself. Animals live in the BasketDictionary from exercise 4,
/// keyed by chip number -- the class that was built two exercises ago is now
/// load-bearing, which is the point of building it.
/// </summary>
public class Shelter : IEnumerable<Animal>
{
    private readonly BasketDictionary<string, Animal> animals = new();

    /// <summary>Kennel number to chip number. Exercise Z6.</summary>
    private readonly Dictionary<int, string> kennels = [];

    public Shelter(string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name must not be empty.", nameof(name));
        }

        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than 0.", nameof(capacity));
        }

        Name = name;
        Capacity = capacity;
    }

    public string Name { get; }

    public int Capacity { get; }

    public int AnimalCount => animals.Count;

    public void Admit(Animal animal)
    {
        ArgumentNullException.ThrowIfNull(animal);

        if (animals.ContainsKey(animal.ChipNumber))
        {
            throw new InvalidOperationException(
                $"An animal with chip number {animal.ChipNumber} is already here.");
        }

        if (AnimalCount >= Capacity)
        {
            throw new InvalidOperationException($"{Name} is full.");
        }

        animals.Add(animal.ChipNumber, animal);
        kennels[NextFreeKennel()] = animal.ChipNumber;
    }

    public Animal Adopt(string chipNumber)
    {
        Animal animal = this[chipNumber];
        animals.Remove(chipNumber);

        // Find first, remove afterwards. Removing from the dictionary while
        // the foreach is still running happens to work here -- the break
        // leaves immediately, and .NET has allowed Remove during enumeration
        // since Core 3.0 -- but it is the exact pattern exercise Z2 is about,
        // and it would turn into an InvalidOperationException the moment
        // somebody drops the break to clean up a second entry.
        int? occupied = null;

        foreach (KeyValuePair<int, string> kennel in kennels)
        {
            if (kennel.Value == chipNumber)
            {
                occupied = kennel.Key;
                break;
            }
        }

        if (occupied is int kennelNumber)
        {
            kennels.Remove(kennelNumber);
        }

        return animal;
    }

    public Animal this[string chipNumber]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(chipNumber);

            if (animals.TryGetValue(chipNumber, out Animal? animal))
            {
                return animal;
            }

            throw new KeyNotFoundException($"No animal with chip number {chipNumber}.");
        }
    }

    /// <summary>
    /// Exercise Z6. Two indexers on one class, told apart by the type of what
    /// is in the brackets: a string is a chip number, an int is a kennel.
    /// </summary>
    public Animal this[int kennelNumber]
    {
        get
        {
            if (!kennels.TryGetValue(kennelNumber, out string? chipNumber))
            {
                throw new KeyNotFoundException($"Kennel {kennelNumber} is empty.");
            }

            return this[chipNumber];
        }
    }

    public int KennelOf(string chipNumber)
    {
        foreach (KeyValuePair<int, string> kennel in kennels)
        {
            if (kennel.Value == chipNumber)
            {
                return kennel.Key;
            }
        }

        throw new KeyNotFoundException($"No animal with chip number {chipNumber}.");
    }

    public IEnumerator<Animal> GetEnumerator() => animals.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private int NextFreeKennel()
    {
        int kennel = 1;

        while (kennels.ContainsKey(kennel))
        {
            kennel++;
        }

        return kennel;
    }
}
