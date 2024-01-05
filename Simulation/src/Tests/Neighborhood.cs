namespace Test;
using System.Collections.Generic;

public class NeighborhoodTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(1, 3)]
    [InlineData(1, 4)]
    [InlineData(1, 5)]
    [InlineData(1, 6)]
    [InlineData(1, 7)]
    [InlineData(1, 8)]
    public void VonNeumannConfigurations(uint radius, int stateCount)
    {
        var neighborhood = new VonNeumann(radius);
        List<State[]> configs = new();

        foreach (var k in neighborhood.EnumerateConfigurations(stateCount))
            configs.Add(k);

        Console.WriteLine($"{radius} {stateCount}");
        Assert.Equal(configs.Count, Math.Pow(stateCount, (int)neighborhood.Count()));

        while(configs.Count > 0)
        {
            var key = configs[0];
            var matches = 0;
            foreach(var other in configs)
                if(key == other)
                    matches++;

            Assert.Equal(1, matches);
            configs.RemoveAt(0);
        }

    }
}
