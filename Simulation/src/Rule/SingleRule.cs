
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Simulation
{
    [JsonConverter(typeof(Json.PastStateRuleConverter))]
    public class SingleRule : Rule, ICloneable
    {
        StateTable stateTable;
        int _StatesCount;
        Random rng = new();

        private Neighborhood __neighborhood;

        public override int GetDefaultState() => stateTable.DefaultState;
        public override void SetDefaultState(int v) => stateTable.DefaultState = v;
        public override int GetStatesCount() => _StatesCount;
        public override void SetStatesCount(int v) { }
        public override int GetBitsCount() => (int)Math.Ceiling(Math.Log2(StatesCount));
        public override void SetBitsCount(int v) { }
        public override Neighborhood GetNeighborhood() => __neighborhood;
        public override void SetNeighborhood(Neighborhood v)
        {
            if (v == null) throw new NullReferenceException();
            __neighborhood = v;
        }
        //public override IEnumerable<ConfigurationKey> GetConfigurationKeys() => stateTable.Keys;


        public SingleRule(int statesCount, int defaultState = 0)
        {
            _StatesCount = statesCount;
            stateTable = new StateTree(statesCount, rng);
            __neighborhood = new Radius1D();
        }

        public void Increment(State[] config, int stateValue, uint amount = 1)
            => stateTable.Increment(config, stateValue, amount);

        public void Set(State[] config, int stateValue)
            => stateTable.Set(config, stateValue);

        public void Unset(State[] config)
            => stateTable.Unset(config);

        public bool Contains(State[] config)
            => stateTable.Contains(config);

        public void Reset()
        {
            stateTable.Clear();
        }

        public override State Get(State[] config)
            => new State(BitsCount, stateTable.Get(config).Get());

        public static SingleRule operator +(SingleRule a, SingleRule b)
        {
            var ret = (SingleRule)a.Clone();

            foreach (var k in b.stateTable.EnumerateConfigurations())
            {
                var counter = b.stateTable[k];
                for (int i = 0; i < ret.StatesCount; i++)
                    ret.Increment(k, i, counter[i]);
            }

            return ret;
        }

        public override double[] Distribution(State[] config)
            => stateTable[config].Distribution();

        public double Variance(State[] config)
            => stateTable[config].Variance();

        public override double AverageDifference(SingleRule other)
        {
            var ret = 0d;
            var count = 0d;

            foreach (var config in stateTable.EnumerateConfigurations())
            {
                var thisDist = Distribution(config);
                var otherDist = other.Distribution(config);

                ret += Math.Abs(thisDist[0] - otherDist[0]);
                ret += Math.Abs(thisDist[1] - otherDist[1]);

                count += 1d;
            }

            return ret / count;
        }

        public override double AverageVariance()
        {
            var ret = 0d;
            var count = 0d;

            foreach (var config in stateTable.EnumerateConfigurations())
            {
                ret += Variance(config);
                count += 1d;
            }

            return ret / count;
        }

        public object Clone()
        {
            var ret = new SingleRule(StatesCount, DefaultState);
            ret.Neighborhood = Neighborhood;
            foreach (var config in stateTable.EnumerateConfigurations())
            {
                var counter = stateTable[config];
                for (int i = 0; i < StatesCount; i++)
                    ret.Increment(config, i, counter[i]);
            }
            return ret;
        }

        public override IEnumerable<State[]> EnumerateConfigurations()
            => stateTable.EnumerateConfigurations();


        /* RANDOM GENERATION */

        public static SingleRule Random(IEnumerable<State[]> configurations, int stateCount, Random rng = null)
        {
            if (rng == null) rng = new Random();

            var ret = new SingleRule(stateCount);

            foreach (var config in configurations)
            {
                if (rng.Next(2) == 0)
                    ret.Set(config, rng.Next(stateCount));
                else
                    for (int i = 0; i < stateCount; i++)
                        ret.Increment(config, i, (uint)rng.Next() % 5000);
            }

            return ret;
        }

        public static SingleRule Random(Neighborhood neighborhood, int stateCount, Random rng = null)
        {
            if (rng == null) rng = new Random();
            var ret = Random(neighborhood.EnumerateConfigurations(stateCount), stateCount, rng);
            ret.Neighborhood = neighborhood;
            return ret;
        }

        /*
         *
        public static Rule Make2D(Neighborhood neighborhood, int stateCount, Random rng = null)
        {
            if (rng == null) rng = new Random();
            var ret = Make(neighborhood.Enumerate2DConfigurations(stateCount), stateCount, rng);
            ret.Neighborhood = neighborhood;
            return ret;
        }
         */
    }


    namespace Json
    {
        public class PastStateRuleConverter : JsonConverter<SingleRule>
        {
            static Neighborhood NeighborhoodDeserialize(
                ref Utf8JsonReader reader,
                JsonSerializerOptions options
            )
            {
                if (reader.TokenType != JsonTokenType.StartArray)
                    throw new JsonException();

                var type = reader.GetString();

                switch (type)
                {
                    case "VonNeumann":
                        var converter = new VonNeumannConverter();
                        return converter.Read(ref reader, typeof(VonNeumann), options);

                    default:
                        break;
                }

                return null;
            }

            public override SingleRule Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options
            )
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException();
                }

                int defaultState = 0;
                int stateCount = 2;
                Neighborhood neighborhood = null;
                Dictionary<ConfigurationKey, double[]> distributionTable = new();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        switch (reader.GetString())
                        {
                            case "DefaultState":
                                defaultState = reader.GetInt16();
                                break;

                            case "StateCount":
                                stateCount = reader.GetInt16();
                                break;

                            case "Neighborhood":
                                neighborhood = NeighborhoodDeserialize(ref reader, options);
                                break;

                            case "StateTable":
                                if (reader.TokenType != JsonTokenType.StartObject)
                                    throw new JsonException();

                                while (true)
                                {
                                    if (reader.TokenType == JsonTokenType.EndObject)
                                        break;

                                    if (reader.TokenType == JsonTokenType.PropertyName)
                                    {
                                        var key = new ConfigurationKey(reader.GetString());
                                        if (reader.TokenType != JsonTokenType.StartArray)
                                            throw new JsonException();

                                        var dist = new List<double>();
                                        double current;
                                        while (reader.TryGetDouble(out current))
                                            dist.Add(current);

                                        distributionTable[key] = dist.ToArray();
                                    }

                                }

                                break;

                            default:
                                break;
                        }
                    }

                }

                SingleRule ret = new(stateCount, defaultState);
                ret.Neighborhood = neighborhood;

                const int samples = 100000;
                foreach (var k in distributionTable.Keys)
                {
                    var dist = distributionTable[k];
                    for (int i = 0; i < dist.Length; i++)
                        ret.Increment(OldNeighborhood.Decode(k, (int)Math.Ceiling(Math.Log2(stateCount))), 
                        i, (uint)(samples * dist[i]));
                }

                return ret;
            }

            public override void Write(
                Utf8JsonWriter writer,
                SingleRule ruleValue,
                JsonSerializerOptions options
            )
            {
                writer.WriteStartObject();

                writer.WriteNumber("DefaultState", ruleValue.DefaultState);
                writer.WriteNumber("StatesCount", ruleValue.StatesCount);

                writer.WritePropertyName("Neighborhood");
                writer.WriteRawValue(JsonSerializer.Serialize(ruleValue.Neighborhood));

                writer.WritePropertyName("StateTable");

                writer.WriteStartObject();
                foreach (var k in ruleValue.EnumerateConfigurations())
                {
                    var pname = k.ToString();
                    writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(pname) ?? pname);

                    writer.WriteStartArray();

                    var dist = ruleValue.Distribution(k);
                    foreach (var p in dist)
                        writer.WriteNumberValue(p);

                    writer.WriteEndArray();
                }
                writer.WriteEndObject();

                writer.WriteEndObject();
            }


        }
    }
}
