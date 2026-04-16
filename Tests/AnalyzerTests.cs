namespace Test;

using Simulation;
using Analysis;

public class Analyzer1DTests
{
    // ── SingleRule ────────────────────────────────────────────────────────────

    [Fact]
    public void SingleRule_RecoversBinaryRuleFromDynamics()
    {
        // Run a known Wolfram rule, then verify the inferred rule has low variance.
        var originalRule = new WolframRule(30);
        var dynamics = GenerateDynamics(originalRule, cells: 40, steps: 50);

        var p = new Analyzer1D.Params
        {
            StatesCount = 2,
            StartingRadius = 1,
            MaxRadius = 4,
            VarianceThreshold = 0.1,
            LookBackAmount = 0
        };

        var inferred = Analyzer1D.SingleRule(dynamics, p);

        Assert.NotNull(inferred);
        Assert.True(inferred.AverageVariance() <= p.VarianceThreshold,
            $"Average variance {inferred.AverageVariance()} exceeds threshold {p.VarianceThreshold}");
    }

    [Fact]
    public void SingleRule_ThrowsOnTooFewStates()
    {
        var p = new Analyzer1D.Params { LookBackAmount = 1 };
        var dynamics = GenerateDynamics(new WolframRule(30), cells: 10, steps: 2);

        // Needs at least LookBackAmount + 4 = 5 states; we have 3 → should throw
        Assert.Throws<Exception>(() => Analyzer1D.SingleRule(dynamics, p));
    }

    // ── TimeSeries ────────────────────────────────────────────────────────────

    [Fact]
    public void TimeSeries_ReturnsCorrectLength()
    {
        var dynamics = GenerateDynamics(new WolframRule(110), cells: 30, steps: 20);
        var p = new Analyzer1D.Params
        {
            StatesCount = 2,
            StartingRadius = 1,
            MaxRadius = 3,
            VarianceThreshold = 0.5,
            LookBackAmount = 0
        };

        var series = Analyzer1D.TimeSeries(dynamics, p);

        int expected = dynamics.Length - (int)p.LookBackAmount - 4;
        Assert.True(expected > 0, "test data too short");
        Assert.Equal(expected, series.Length);
    }

    [Fact]
    public void TimeSeries_AllRulesNonNull()
    {
        var dynamics = GenerateDynamics(new WolframRule(30), cells: 30, steps: 15);
        var p = new Analyzer1D.Params
        {
            StatesCount = 2,
            StartingRadius = 1,
            MaxRadius = 3,
            VarianceThreshold = 0.5,
            LookBackAmount = 0
        };

        var series = Analyzer1D.TimeSeries(dynamics, p);
        Assert.All(series, r => Assert.NotNull(r));
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    static Simulation.Container.Array<State>[] GenerateDynamics(
        WolframRule rule, int cells, int steps)
    {
        var space = new Simulation.Container.Array<State>(cells, new State(1, 0));
        var sim = new Model<Simulation.Container.Array<State>>(space);
        sim.Rule = rule;
        sim.Randomize(new Random(1));

        var history = new List<Simulation.Container.Array<State>>();
        history.Add(new Simulation.Container.Array<State>(sim.CurrentState));
        for (int i = 0; i < steps; i++)
        {
            sim.Advance();
            history.Add(new Simulation.Container.Array<State>(sim.CurrentState));
        }
        return history.ToArray();
    }
}
