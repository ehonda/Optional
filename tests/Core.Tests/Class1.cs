namespace Tests;

public class PlaceholderTests
{
    [Test]
    public async Task Placeholder_Test_ShouldPass()
    {
        // Arrange
        var expected = 42;

        // Act
        var actual = 42;

        // Assert
        await Assert.That(actual).IsEqualTo(expected);
    }
}
