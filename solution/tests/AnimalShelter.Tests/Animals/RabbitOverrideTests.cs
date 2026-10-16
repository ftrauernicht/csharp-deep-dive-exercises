using AnimalShelter.Core.Animals;

namespace AnimalShelter.Tests.Animals;

/// <summary>
/// Exercise 1.5, written down as assertions: Rabbit.Describe() is declared
/// `override`, and these three calls record what that buys.
/// </summary>
public class RabbitOverrideTests
{
    [Fact]
    public void Describe_RabbitHeldInARabbitVariable_ReturnsTheRabbitVersion()
    {
        Rabbit a = new Rabbit("Hoppel", "276098100000001", 18, 2.0);

        // Prediction: "Rabbit Hoppel" -- the variable is already of the derived
        // type, so the compiler finds Rabbit.Describe() without any dispatch.
        Assert.Equal("Rabbit Hoppel", a.Describe());
    }

    [Fact]
    public void Describe_RabbitHeldInAnAnimalVariable_ReturnsTheRabbitVersion()
    {
        Animal b = new Rabbit("Hoppel", "276098100000002", 18, 2.0);

        // Prediction: "Rabbit Hoppel" -- `override` makes the call virtual, so the
        // object's real type decides, not the declared type of the variable.
        Assert.Equal("Rabbit Hoppel", b.Describe());
    }

    [Fact]
    public void ToString_RabbitHeldInAnAnimalVariable_ReturnsTheRabbitDescription()
    {
        Animal b = new Rabbit("Hoppel", "276098100000003", 18, 2.0);

        // Prediction: "Rabbit Hoppel" -- Animal.ToString() calls Describe() on
        // itself, and that inner call is dispatched into the override as well.
        Assert.Equal("Rabbit Hoppel", b.ToString());
    }

    // What `new string Describe()` instead of `override` would have changed:
    // only the first of the three would still read "Rabbit Hoppel". A `new`
    // member hides the base one instead of replacing it, so the base method
    // stays in the virtual slot: the second assert would get the base format
    // "Hoppel, 18 months", and the third one the same, because Animal.ToString()
    // is compiled against Animal and therefore never sees a hiding member in a
    // subclass. That split -- two different answers for one and the same object,
    // depending on which variable one happens to be holding it in -- is the
    // reason the finished code uses `override`.
}
