namespace Test;
using System.Collections.Generic;

public class IntegrationTest
{
    [Theory]
    [InlineData(0.1, 100, 0.075)]
    [InlineData(0.3, 400, 0.075)]
    [InlineData(0.7, 3000, 0.05)]
    [InlineData(0.5, 1500, 0.05)]
    public void TestBoolDistribution(double trueProbability, uint samples, double eps)
    {
        var rng = new Random();
        var rule = new Rule();
        rule.Neighborhood = new VonNeumann(1);

        var config = new bool[] { false, false, false, false, false };
        Assert.Equal((int)rule.Neighborhood.Count(), config.Length);

        var configKey = Neighborhood.Encode(config);

        for (int i = 0; i < samples; i++)
            rule.Increment(configKey, rng.NextDouble() < trueProbability);

        Assert.InRange(rule.Distribution(configKey)[0], trueProbability - eps, trueProbability + eps);

        var counters = new int[2]{0,0};
        for (int i = 0; i < samples; i++)
            counters[rule.Get(configKey) ? 0 : 1]++;

        var ratio = (double)counters[0] / (double)(counters[0] + counters[1]);
        Assert.InRange(ratio, trueProbability - eps, trueProbability + eps);
    }
}
