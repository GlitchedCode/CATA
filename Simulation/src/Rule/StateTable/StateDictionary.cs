
namespace Simulation;

public class StateDictionary : StateTable
{
    Dictionary<ConfigurationKey, StateCounter> stateTable = new();
    public int StatesCount { get; private set; }
    Random rng = new();

    public static ConfigurationKey Encode(State[] configuration)
    {
        var bitCount = configuration[0].BitsCount;
        var statesPerByte = 8 / bitCount;

        byte[] buf = new byte[(configuration.Length / statesPerByte) + 1];
        for (int i = 0; i < buf.Length; i++)
            buf[i] = 0;

        for (int i = 0; i < configuration.Length; i++)
        {
            var bufIdx = i / statesPerByte;
            var shift = (i % statesPerByte) * bitCount;

            var val = configuration[i].Value;
            buf[bufIdx] |= (byte)(val << shift);
        }

        return new ConfigurationKey(buf);
    }

    public static State[] Decode(ConfigurationKey config, int bitCount)
    {
        var buf = config.Bytes;
        var statesPerByte = 8 / bitCount;

        var ret = new State[buf.Length * statesPerByte];
        var stateIdx = 0;

        for (int i = 0; i < buf.Length; i++)
        {
            var bitIdx = 0;
            var leftShift = 0;
            while ((leftShift = 8 - bitIdx - bitCount) >= 0)
            {
                var val = (buf[i] << leftShift);
                val = val >> (8 - bitCount);
                ret[stateIdx] = new State(bitCount, val);
                stateIdx++;
                bitIdx += bitCount;
            }
        }

        return ret;
    }

    public StateDictionary(int stateCount = 2, Random rng = null)
    {
        if (rng != null) this.rng = rng;
        StatesCount = stateCount;
    }

    void EnsureKey(ConfigurationKey key)
    {
        if (!stateTable.ContainsKey(key))
            stateTable[key] = new(StatesCount, rng);
    }


    public override bool Contains(State[] configuration)
        => stateTable.ContainsKey(Encode(configuration));

    public override StateCounter Get(State[] configuration)
    {
        var key = Encode(configuration);
        if (stateTable.ContainsKey(key))
            return stateTable[key];
        else
        {
            var ret = new StateCounter(StatesCount, rng);
            ret.Set(DefaultState);
            return ret;
        }
    }

    public override void Increment(State[] configuration, int stateValue, uint amount)
    {
        var key = Encode(configuration);
        EnsureKey(key);
        stateTable[key].Increment(stateValue, amount);
    }

    public override void Set(State[] configuration, int stateValue)
    {
        var key = Encode(configuration);
        EnsureKey(key);
        stateTable[key].Set(stateValue);
    }

    public override void Unset(State[] configuration)
        => stateTable.Remove(Encode(configuration));

    public override void Clear()
        => stateTable.Clear();

    public override IEnumerable<State[]> EnumerateConfigurations()
    {
        var bitCount = (int)Math.Ceiling(Math.Log2(StatesCount));
        foreach (var k in stateTable.Keys)
            yield return Decode(k, bitCount);
        yield break;
    }
}
