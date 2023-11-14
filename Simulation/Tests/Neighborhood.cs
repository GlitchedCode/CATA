namespace Test;
using System.Collections.Generic;

public class NeighborhoodTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3.03)]
    public void VonNeumannBoolConfigurations(uint radius)
    {
        var neighborhood = new VonNeumann(radius);
        List<ConfigurationKey> configs = new();

        foreach (var k in Neighborhood.EnumerateBooleanConfigurations(neighborhood))
            configs.Add(k);

        Assert.Equal(configs.Count, Math.Pow(2, (int)neighborhood.Count()));

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
