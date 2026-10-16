using AnimalShelter.Core.Animals;
using AnimalShelter.Core.Management;
using AnimalShelter.Core.Queries;

namespace AnimalShelter.Tests.Queries;

/// <summary>
/// Exercise 5.2: seven experiments on when a LINQ query actually runs.
///
/// Every assert carries the prediction that was written down before the test
/// was run first. Where the prediction missed, the comment says so and names
/// the number that really came out -- the wrong guess is the part worth
/// keeping, because it marks the spot where the mental model was off.
/// </summary>
public class DeferredExecutionTests
{
    private const string MimiChip = "276098100000001";
    private const string BrunoChip = "276098100000002";
    private const string RexChip = "276098100000003";
    private const string HoppelChip = "276098100000004";

    // --- 1. Changing the source after the query was written ---

    [Fact]
    public void Where_SourceGrowsAfterTheQueryWasWritten_SeesTheNewAnimal()
    {
        Cat mimi = new("Mimi", MimiChip, 24, 4.0);
        Dog rex = new("Rex", RexChip, 36, 30.0);
        Cat bruno = new("bruno", BrunoChip, 6, 1.5);

        List<Animal> animals = [mimi, rex];
        IEnumerable<Animal> cats = animals.Where(a => a is Cat);

        animals.Add(bruno);

        int count = cats.Count();

        // Prediction: 2, because Where only stored the list and the lambda; the
        // list is not read until Count() walks it, and by then bruno is in it.
        Assert.Equal(2, count);
    }

    [Fact]
    public void Where_MaterialisedWithToList_IgnoresTheLaterAddition()
    {
        Cat mimi = new("Mimi", MimiChip, 24, 4.0);
        Dog rex = new("Rex", RexChip, 36, 30.0);
        Cat bruno = new("bruno", BrunoChip, 6, 1.5);

        List<Animal> animals = [mimi, rex];
        List<Animal> cats = animals.Where(a => a is Cat).ToList();

        animals.Add(bruno);

        int count = cats.Count;

        // Prediction: 1, because ToList() copied the result right there and the
        // copy has no connection to the list it was built from.
        Assert.Equal(1, count);
    }

    // --- 2. How often the lambda runs ---

    [Fact]
    public void Select_CountedInvocations_RunsOncePerElementPerEnumeration()
    {
        List<Animal> animals =
        [
            new Cat("Mimi", MimiChip, 24, 4.0),
            new Dog("Rex", RexChip, 36, 30.0),
            new Rabbit("Hoppel", HoppelChip, 18, 2.0),
        ];

        int calls = 0;
        IEnumerable<string> names = animals.Select(a =>
        {
            calls++;
            return a.Name;
        });

        // Prediction: 0, because writing the query runs nothing at all.
        Assert.Equal(0, calls);

        int counted = names.Count();
        int afterCount = calls;

        // Prediction: 3, the number of elements -- nothing surprising about the
        // return value itself.
        Assert.Equal(3, counted);

        // Prediction: 3, and this is the one worth pausing on. Count() could skip
        // the projection entirely: it only needs the number of elements, and the
        // source is a List whose Count is already known. It does not. LINQ's Select
        // iterator runs the selector once per element when Count() asks, on purpose,
        // so that a selector with a side effect is not silently skipped.
        Assert.Equal(3, afterCount);

        _ = names.First();
        int afterFirst = calls;

        // Prediction: 4. First() pulls exactly one element and stops, so one more
        // call on top of the three from Count(). Nothing is cached between
        // enumerations.
        Assert.Equal(4, afterFirst);

        foreach (string unused in names)
        {
            _ = unused;
        }

        int afterForeach = calls;

        // Prediction: 7. A full pass is three more calls -- the query is a recipe,
        // and every enumeration cooks it again from the start.
        Assert.Equal(7, afterForeach);
    }

    // --- 3. When the exception surfaces ---

    [Fact]
    public void Select_LambdaThatThrows_ThrowsOnEnumerationNotOnDefinition()
    {
        List<Animal> animals =
        [
            new Cat("Mimi", MimiChip, 24, 4.0),
            new Dog("Boom", RexChip, 36, 30.0),
            new Rabbit("Hoppel", HoppelChip, 18, 2.0),
        ];

        int calls = 0;

        // This line builds the query. If deferred execution were a myth, it would
        // already blow up here, and the test would fail before the first assert.
        IEnumerable<string> query = animals.Select(animal =>
        {
            calls++;
            return animal.Name == "Boom"
                ? throw new InvalidOperationException($"{animal.Name} is not a name.")
                : animal.Name;
        });

        int callsAfterDefinition = calls;

        // Prediction: 0 -- building a query touches no element, so the lambda that
        // would throw has not run yet.
        Assert.Equal(0, callsAfterDefinition);

        // Prediction: not null. The query object exists even though it holds a
        // lambda that is guaranteed to throw; a broken query is a perfectly valid
        // value until somebody asks it for data.
        Assert.NotNull(query);

        // Prediction: this is the line that throws. Assert.Throws wraps the
        // enumeration, not the definition above, because that is where the lambda
        // finally runs.
        InvalidOperationException error =
            Assert.Throws<InvalidOperationException>(() => query.ToList());

        // Prediction: the message is the one the lambda built from Boom's own name,
        // so it really is the projection that failed and not the enumerator.
        Assert.Equal("Boom is not a name.", error.Message);

        int callsAfterThrow = calls;

        // Prediction: 2. Streaming stopped at the second element; Hoppel was never
        // reached, which is why a failing query can still have produced output.
        Assert.Equal(2, callsAfterThrow);
    }

    // --- 4. Adopting while the loop is still reading the shelter ---

    [Fact]
    public void Kittens_AdoptedInsideTheForeach_ThrowsBecauseTheSourceChanged()
    {
        Shelter shelter = ShelterWithThreeKittens();
        ShelterReport report = new(shelter);

        // Prediction: InvalidOperationException. Kittens() is a deferred query over
        // the shelter, so the foreach still has an open enumerator on the
        // BasketDictionary when Adopt() removes an entry from it, and the
        // dictionary's version counter notices on the next step.
        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (Animal kitten in report.Kittens())
            {
                shelter.Adopt(kitten.ChipNumber);
            }
        });

        // Prediction: the message comes from BasketDictionary.GetEnumerator, not
        // from Shelter or from LINQ -- the shelter itself has no idea a query is
        // running.
        Assert.Contains("modified while it was being enumerated", error.Message);

        int left = shelter.AnimalCount;

        // Prediction: 2 of 3. Exactly one adoption got through before the next
        // MoveNext() failed, which is the nastiest part: the operation is half
        // done, not undone.
        Assert.Equal(2, left);
    }

    [Fact]
    public void Kittens_MaterialisedBeforeTheForeach_AdoptsAllOfThem()
    {
        Shelter shelter = ShelterWithThreeKittens();
        ShelterReport report = new(shelter);

        // ToList() sits on the query and not inside the loop, and that position is
        // the whole fix: it finishes reading the shelter and closes the enumerator
        // before the first Adopt() runs. Written as
        // `foreach (... in report.Kittens())` with a ToList() somewhere in the body
        // it would change nothing, because the loop's own enumerator would still be
        // open on the dictionary that Adopt() modifies.
        foreach (Animal kitten in report.Kittens().ToList())
        {
            shelter.Adopt(kitten.ChipNumber);
        }

        int left = shelter.AnimalCount;

        // Prediction: 0, because the loop now walks a private copy and the shelter
        // is free to change underneath it.
        Assert.Equal(0, left);
    }

    // --- 5. The order in which elements are pulled ---

    /// <summary>
    /// Passes every element through and writes a line per element as it goes, so
    /// that the log length says how far into the source an operator reached.
    /// </summary>
    private static IEnumerable<Animal> WithLog(IEnumerable<Animal> source, List<string> log)
    {
        foreach (Animal animal in source)
        {
            log.Add($"yield {animal.Name}");
            yield return animal;
        }
    }

    [Fact]
    public void Where_FollowedByFirst_StopsAtTheFirstMatch()
    {
        List<Animal> animals = ThreeAnimalsDogFirst();
        List<string> log = [];

        _ = WithLog(animals, log).Where(a => a is Cat).First();

        int yields = log.Count;

        // Prediction: 2. Rex is pulled and rejected, Mimi is pulled and accepted,
        // and First() stops there -- Hoppel is never touched.
        Assert.Equal(2, yields);

        // Prediction: Rex before Mimi, in source order; Where does not reorder and
        // does not look ahead.
        Assert.Equal(new[] { "yield Rex", "yield Mimi" }, log);
    }

    [Fact]
    public void OrderBy_FollowedByFirst_PullsEverythingAnyway()
    {
        List<Animal> animals = ThreeAnimalsDogFirst();
        List<string> log = [];

        _ = WithLog(animals, log).OrderBy(a => a.Name).First();

        int yields = log.Count;

        // Prediction: 3. OrderBy cannot know which element comes first without
        // seeing all of them, so it buffers the whole source before it yields
        // anything. "Deferred" does not mean "streaming".
        Assert.Equal(3, yields);
    }

    [Fact]
    public void Take_PullsOnlyAsManyElementsAsItYields()
    {
        List<Animal> animals = ThreeAnimalsDogFirst();
        List<string> log = [];

        _ = WithLog(animals, log).Take(2).ToList();

        int yields = log.Count;

        // Prediction: 2. Take counts down and stops asking once it has two, so the
        // third element is never pulled -- no element is fetched and discarded.
        Assert.Equal(2, yields);
    }

    [Fact]
    public void SelectThenWhere_RunsEachElementThroughTheWholeChain()
    {
        List<Animal> animals = ThreeAnimalsDogFirst();
        List<string> log = [];

        List<Animal> result = WithLog(animals, log)
            .Select(a =>
            {
                log.Add($"select {a.Name}");
                return a;
            })
            .Where(a =>
            {
                log.Add($"where {a.Name}");
                return true;
            })
            .ToList();

        int produced = result.Count;

        // Prediction: 3, since the Where predicate accepts everything -- the
        // interesting part is the order in the log below, not the result.
        Assert.Equal(3, produced);

        // Prediction: interleaved, one element at a time through the whole chain --
        // not all three through Select and then all three through Where. Each
        // operator pulls from the one before it on demand, so an element is carried
        // to the end of the pipeline before the next one is fetched. This is why a
        // long chain over a huge source never materialises an intermediate list.
        Assert.Equal(
            new[]
            {
                "yield Rex", "select Rex", "where Rex",
                "yield Mimi", "select Mimi", "where Mimi",
                "yield Hoppel", "select Hoppel", "where Hoppel",
            },
            log);
    }

    // --- 6. The captured variable ---

    [Fact]
    public void Where_CapturedVariableChangedAfterTheQuery_UsesTheNewValue()
    {
        List<Animal> animals =
        [
            new Cat("bruno", BrunoChip, 6, 1.5),
            new Cat("Mimi", MimiChip, 24, 4.0),
            new Dog("Rex", RexChip, 120, 30.0),
        ];

        int minAge = 12;
        IEnumerable<Animal> old = animals.Where(a => a.AgeInMonths >= minAge);

        minAge = 100;

        int count = old.Count();

        // Prediction: 1, not 2. The lambda captured the variable and not its value,
        // so by the time Count() runs the comparison it reads 100 -- only Rex
        // qualifies. With minAge = 12 the same query object would have answered 2.
        Assert.Equal(1, count);
    }

    // --- 7. Distinct on two records of the same animal ---

    [Fact]
    public void Distinct_TwoCatsWithTheSameChipNumber_CollapsesToOne()
    {
        Cat asFiled = new("Mimi", MimiChip, 24, 4.0);
        Cat asReported = new("Minka", MimiChip, 24, 4.0);

        List<Animal> list = [asFiled, asReported];

        int distinct = list.Distinct().Count();

        // Prediction: 1. Distinct fills a HashSet with the default equality
        // comparer, which uses Animal's own members: GetHashCode returns
        // ChipNumber.GetHashCode(), so both land in the same bucket, and there
        // Equals compares nothing but the chip number and says yes. The different
        // name never enters the decision -- if GetHashCode had been built from the
        // name instead, the two would have gone to different buckets and Equals
        // would never have been asked, so Distinct would have reported 2.
        Assert.Equal(1, distinct);
    }

    // --- Fixtures ---

    private static Shelter ShelterWithThreeKittens()
    {
        Shelter shelter = new("Test", 10);
        shelter.Admit(new Cat("Mimi", MimiChip, 6, 1.2));
        shelter.Admit(new Cat("bruno", BrunoChip, 4, 1.0));
        shelter.Admit(new Cat("Nala", RexChip, 11, 1.8));
        return shelter;
    }

    private static List<Animal> ThreeAnimalsDogFirst() =>
    [
        new Dog("Rex", RexChip, 36, 30.0),
        new Cat("Mimi", MimiChip, 24, 4.0),
        new Rabbit("Hoppel", HoppelChip, 18, 2.0),
    ];
}
