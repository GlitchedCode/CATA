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
        var rule = new SingleRule(2);
        rule.Neighborhood = new VonNeumann(1);

        var config = Enumerable.Range(1, 5).Select(_ => new State(1, 0)).ToArray();
        Assert.Equal((int)rule.Neighborhood.Count(), config.Length);

        var configKey = OldNeighborhood.Encode(config);

        for (int i = 0; i < samples; i++)
            rule.Increment(config, rng.NextDouble() < trueProbability ? 1 : 0);

        Assert.InRange(rule.Distribution(config)[1], trueProbability - eps, trueProbability + eps);

        var counters = new int[2] { 0, 0 };
        for (int i = 0; i < samples; i++)
            counters[rule.Get(config).Value]++;

        var ratio = (double)counters[1] / (double)(counters[0] + counters[1]);
        Assert.InRange(ratio, trueProbability - eps, trueProbability + eps);
    }
}
