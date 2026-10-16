using AnimalShelter.Core.Animals;

namespace AnimalShelter.Tests.Animals;

public class DescribeTests
{
    [Fact]
    public void Describe_OutdoorCat_MentionsOutdoor()
    {
        // isOutdoor is a constructor argument, so the variable can stay an Animal.
        Animal cat = new Cat("Mimi", "276098100000001", 36, 4.0, isOutdoor: true);

        Assert.Equal("Mimi, 36 months (outdoor)", cat.Describe());
    }

    [Fact]
    public void Describe_IndoorCat_MentionsIndoor()
    {
        Animal cat = new Cat("Nala", "276098100000002", 24, 3.5, isOutdoor: false);

        Assert.Equal("Nala, 24 months (indoor)", cat.Describe());
    }

    [Fact]
    public void Describe_CatDefaultsToIndoor_WhenIsOutdoorIsOmitted()
    {
        Animal cat = new Cat("Felix", "276098100000003", 30, 4.2);

        Assert.Equal("Felix, 30 months (indoor)", cat.Describe());
    }

    [Fact]
    public void Describe_Dog_UsesTheInheritedFormat()
    {
        // Dog overrides neither Describe nor the food rule, which is what makes
        // the cat's version visibly its own.
        Animal dog = new Dog("Rex", "276098100000004", 48, 15.0);

        Assert.Equal("Rex, 48 months", dog.Describe());
    }
}
