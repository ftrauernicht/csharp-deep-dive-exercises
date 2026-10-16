using AnimalShelter.Core.Animals;

namespace AnimalShelter.Core.Clinic;

public class Veterinarian
{
    /// <summary>
    /// The end state of exercise 2.1. What used to be a chain of `is Cat`,
    /// `is Dog dog`, `is Rabbit` and a NotSupportedException is now one line,
    /// and the tests written against the old version never changed.
    /// </summary>
    public decimal CalculateTreatmentCost(Animal animal) => animal.TreatmentCostInEuro;

    /// <summary>
    /// Gives every vaccinatable animal the shots it still needs and returns
    /// how many were given.
    ///
    /// `is IVaccinatable` is fine here while `is Cat` was not, and the
    /// difference is worth naming: this asks whether the animal can do the
    /// thing at all, which is a question only the type system can answer. The
    /// old code asked which species it was in order to look up a number the
    /// animal already knew.
    /// </summary>
    public int VaccinateAllMissing(IEnumerable<Animal> animals)
    {
        int given = 0;

        foreach (Animal animal in animals)
        {
            if (animal is not IVaccinatable vaccinatable)
            {
                continue;
            }

            // ToList() on purpose: the loop below calls Vaccinate(), which adds to
            // ReceivedVaccinations -- the very collection this query subtracts.
            // Measured, it would also work without it, because Except() reads its
            // second source completely before yielding the first element. That is
            // an implementation detail and not a promise, so the loop does not
            // build on it. See LINQ.md.
            IEnumerable<string> missing = vaccinatable.RequiredVaccinations
                .Except(vaccinatable.ReceivedVaccinations)
                .ToList();

            foreach (string vaccination in missing)
            {
                vaccinatable.Vaccinate(vaccination);
                given++;
            }
        }

        return given;
    }
}
