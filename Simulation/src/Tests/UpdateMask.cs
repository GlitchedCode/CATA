namespace Test;

public class UpdateMaskTests
{
    [Theory]
    [InlineData(0.0, 1000, 0.02)]
    [InlineData(0.5, 1000, 0.05)]
    [InlineData(1.0, 1000, 0.02)]
    public void RandomUpdateMask_UpdateFractionMatchesProbability(
        double probability, int samples, double eps)
    {
        var rng = new Random(0);
        var mask = new RandomUpdateMask(probability, rng);

        int updated = 0;
        for (int i = 0; i < samples; i++)
        {
            if (mask.Get(i)) updated++;
            mask.Advance();
        }

        double actual = (double)updated / samples;
        Assert.InRange(actual, probability - eps, probability + eps);
    }

    [Fact]
    public void FullyAsyncUpdateMask_ExactlyOneCellUpdatedPerAdvance()
    {
        int cellCount = 20;
        var space = new Array<State>(cellCount, new State(1, 0));
        var mask = new FullyAsyncUpdateMask<State>(space, random: false);

        // Over one full cycle every cell should be updated exactly once
        int totalUpdated = 0;
        for (int step = 0; step < cellCount; step++)
        {
            int updatedThisStep = 0;
            for (int c = 0; c < cellCount; c++)
                if (mask.Get(c)) updatedThisStep++;

            Assert.Equal(1, updatedThisStep);
            totalUpdated += updatedThisStep;
            mask.Advance();
        }

        Assert.Equal(cellCount, totalUpdated);
    }

    [Fact]
    public void DefaultUpdateMask_AllCellsUpdated()
    {
        int cellCount = 50;
        var mask = new UpdateMask();

        for (int c = 0; c < cellCount; c++)
            Assert.True(mask.Get(c));
    }
}
