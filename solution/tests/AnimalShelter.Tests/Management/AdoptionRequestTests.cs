using AnimalShelter.Core.Adoption;
using AnimalShelter.Core.Animals;

namespace AnimalShelter.Tests.Management;

/// <summary>
/// Exercise Z7. Adoption requests are kept in a SortedSet, ordered by the day
/// they came in -- which makes CompareTo carry two jobs at once: it sorts, and
/// it decides what the set considers a duplicate.
/// </summary>
public class AdoptionRequestTests
{
    private static readonly DateOnly January = new(2026, 1, 10);
    private static readonly DateOnly February = new(2026, 2, 20);
    private static readonly DateOnly March = new(2026, 3, 5);

    private static Cat Mimi() => new("Mimi", "276098100000001", 36, 4.0);

    private static Dog Rex() => new("Rex", "276098100000002", 24, 18.0);

    private static Rabbit Floki() => new("Floki", "276098100000003", 12, 2.0);

    [Fact]
    public void SortedSet_SeveralRequests_ComesOutOrderedByReceivedOn()
    {
        // Added out of order on purpose: the set does the sorting, not the caller.
        SortedSet<AdoptionRequest> requests =
        [
            new AdoptionRequest("Carla", Mimi(), March),
            new AdoptionRequest("Anna", Rex(), January),
            new AdoptionRequest("Bernd", Floki(), February),
        ];

        Assert.Equal([January, February, March], requests.Select(r => r.ReceivedOn));
        Assert.Equal(["Anna", "Bernd", "Carla"], requests.Select(r => r.Applicant));
    }

    [Fact]
    public void SortedSet_RequestsOnTheSameDay_KeepsBothOfThem()
    {
        var first = new AdoptionRequest("Anna", Mimi(), February);
        var second = new AdoptionRequest("Bernd", Rex(), February);

        SortedSet<AdoptionRequest> requests = [first];

        // Prediction: had CompareTo compared the date alone, this Add would
        // have returned false and the second request would have vanished with
        // no exception and no warning -- a SortedSet treats two items whose
        // CompareTo returns 0 as the same item, so it keeps the one it already
        // has and reports the insert as "already present". The chip-number and
        // applicant tie-breaks in CompareTo are what prevents that; the test
        // below proves the prediction on a type that lacks them.
        bool secondWasAdded = requests.Add(second);

        Assert.True(secondWasAdded);
        Assert.Equal(2, requests.Count);
        Assert.Contains(first, requests);
        Assert.Contains(second, requests);
    }

    /// <summary>
    /// The counter-proof for the prediction above. DateOnlyRequest is the
    /// mistake on purpose: its CompareTo looks at the date and nothing else.
    /// Two requests from the same day therefore compare as equal, and the set
    /// silently keeps one of them.
    /// </summary>
    [Fact]
    public void SortedSet_ComparingTheDateAlone_SilentlyDropsTheSecondRequest()
    {
        var first = new DateOnlyRequest("Anna", February);
        var second = new DateOnlyRequest("Bernd", February);

        SortedSet<DateOnlyRequest> requests = [first];

        bool secondWasAdded = requests.Add(second);

        // No exception was thrown -- the only sign that something was lost is
        // the return value of Add, and a caller who ignores it never finds out.
        Assert.False(secondWasAdded);

        DateOnlyRequest survivor = Assert.Single(requests);
        Assert.Equal("Anna", survivor.Applicant);
        Assert.Equal(0, first.CompareTo(second));
    }

    [Fact]
    public void CompareTo_Null_IsPositive()
    {
        var request = new AdoptionRequest("Anna", Mimi(), February);

        Assert.True(request.CompareTo(null) > 0);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ApplicantIsEmpty_ThrowsArgumentException(string applicant)
    {
        Assert.Throws<ArgumentException>(() => new AdoptionRequest(applicant, Mimi(), February));
    }

    [Fact]
    public void Constructor_AnimalIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new AdoptionRequest("Anna", null!, February));
    }

    [Fact]
    public void Constructor_ValidArguments_KeepsThem()
    {
        Cat mimi = Mimi();

        var request = new AdoptionRequest("Anna", mimi, February);

        Assert.Equal("Anna", request.Applicant);
        Assert.Same(mimi, request.Animal);
        Assert.Equal(February, request.ReceivedOn);
    }

    /// <summary>
    /// A deliberately broken stand-in for AdoptionRequest: it compares the
    /// date and stops there. Nothing but CompareTo differs from the real
    /// class, which is what makes it a proof rather than an illustration.
    /// </summary>
    private sealed class DateOnlyRequest(string applicant, DateOnly receivedOn)
        : IComparable<DateOnlyRequest>
    {
        public string Applicant { get; } = applicant;

        public DateOnly ReceivedOn { get; } = receivedOn;

        public int CompareTo(DateOnlyRequest? other) =>
            other is null ? 1 : ReceivedOn.CompareTo(other.ReceivedOn);
    }
}
