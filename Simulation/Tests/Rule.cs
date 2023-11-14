namespace Test;

public class BinaryStateCounterTests
{
    [Theory]
    [InlineData(0.5, 400, 0.075)]
    [InlineData(0.3, 400, 0.075)]
    [InlineData(0.2, 400, 0.075)]
    public void BinaryDistribution(double trueProbability, uint samples, double eps)
    {
        var rng = new Random();
        var counter = new BinaryStateCounter();

        for (int i = 0; i < samples; i++)
            counter.Increment(rng.NextDouble() < trueProbability);

        Assert.InRange(counter.Distribution()[0], trueProbability - eps, trueProbability + eps);

        var counters = new int[2]{0,0};
        for (int i = 0; i < samples; i++)
            counters[counter.Get() ? 0 : 1]++;

        var ratio = (double)counters[0] / (double)(counters[0] + counters[1]);
        Assert.InRange(ratio, trueProbability - eps, trueProbability + eps);
    }
}
