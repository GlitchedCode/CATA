
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Simulation
{
    [JsonConverter(typeof(Json.RuleConverter))]
    public class Rule : ICloneable
    {
        Dictionary<ConfigurationKey, StateCounter> stateTable = new();
        public int DefaultState;
        public int StatesCount { get; protected set; }
        public int BitsCount { get; protected set; }

        private Neighborhood __neighborhood;
        public Neighborhood Neighborhood
        {
            get => __neighborhood;
            set
            {
                if (value == null) throw new NullReferenceException();
                __neighborhood = value;
            }
        }

        public IEnumerable<ConfigurationKey> ConfigurationKeys
        {
            get => stateTable.Keys;
        }

        Random rng = new();

        public Rule(int statesCount, int defaultState = 0)
        {
            DefaultState = defaultState;
            StatesCount = statesCount;
            BitsCount = (int)Math.Ceiling(Math.Log2(StatesCount));
            __neighborhood = new VonNeumann();
        }

        void EnsureKey(ConfigurationKey key)
        {
            if (!stateTable.ContainsKey(key))
                stateTable[key] = new(StatesCount, rng);
        }

        public void Increment(ConfigurationKey configurationKey, int stateValue, uint amount = 1)
        {
            EnsureKey(configurationKey);
            stateTable[configurationKey].Increment(stateValue, amount);
        }

        public void Set(ConfigurationKey configurationKey, int stateValue)
        {
            EnsureKey(configurationKey);
            stateTable[configurationKey].Set(stateValue);
        }

        public void Unset(ConfigurationKey configurationKey)
        {
            stateTable.Remove(configurationKey);
        }

        public void Reset()
        {
            stateTable.Clear();
        }

        public State Get(ConfigurationKey configurationKey)
        {
            int state;
            if (stateTable.ContainsKey(configurationKey))
                state = stateTable[configurationKey].Get();
            else
                state = DefaultState;

            return new State(BitsCount, state);
        }

        public static Rule operator +(Rule a, Rule b)
        {
            var ret = (Rule)a.Clone();

            foreach (var k in b.stateTable.Keys)
            {
                var counter = b.stateTable[k];
                for (int i = 0; i < ret.StatesCount; i++)
                    ret.Increment(k, i, counter[i]);
            }

            return ret;
        }

        public double[] Distribution(ConfigurationKey configurationKey)
        {
            if (stateTable.ContainsKey(configurationKey))
                return stateTable[configurationKey].Distribution();

            var ret = Enumerable.Repeat(0.0, StatesCount).ToArray();
            ret[0] = 1;
            return ret;
        }

        public double Variance(ConfigurationKey configurationKey)
        {
            if (stateTable.ContainsKey(configurationKey))
                return stateTable[configurationKey].Variance();

            return 0d;
        }

        public double AverageDifference(Rule other)
        {
            var ret = 0d;
            var count = 0d;

            foreach (var key in ConfigurationKeys)
            {
                var thisDist = Distribution(key);

                var otherKey = other.Neighborhood.ConvertKey(
                    key, new State(BitsCount, DefaultState));
                var otherDist = other.Distribution(otherKey);

                ret += Math.Abs(thisDist[0] - otherDist[0]);
                ret += Math.Abs(thisDist[1] - otherDist[1]);

                count += 1d;
            }

            return ret / count;
        }

        public double AverageVariance()
        {
            var ret = 0d;
            var count = 0d;

            foreach (var key in ConfigurationKeys)
            {
                ret += Variance(key);
                count += 1d;
            }

            return ret / count;
        }

        public object Clone()
        {
            var ret = new Rule(StatesCount, DefaultState);
            ret.Neighborhood = Neighborhood;
            foreach (var k in stateTable.Keys)
            {
                var counter = stateTable[k];
                for (int i = 0; i < StatesCount; i++)
                    ret.Increment(k, i, counter[i]);
            }
            return ret;
        }
    }


    public class RandomRule
    {
        public static Rule Make(Neighborhood neighborhood, int stateCount, Random rng = null)
        {
            if (rng == null) rng = new Random();

            var ret = new Rule(stateCount);
            ret.Neighborhood = neighborhood;

            foreach (var config in neighborhood.EnumerateConfigurations(stateCount))
            {
                if (rng.Next(2) == 0)
                    ret.Set(config, rng.Next(stateCount));
                else
                    for (int i = 0; i < stateCount; i++)
                        ret.Increment(config, i, (uint)rng.Next() % 5000);
            }


            return ret;
        }
    }

    namespace Json
    {
        public class RuleConverter : JsonConverter<Rule>
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

            public override Rule Read(
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

                Rule ret = new Rule(stateCount, defaultState);
                ret.Neighborhood = neighborhood;

                const int samples = 100000;
                foreach (var k in distributionTable.Keys)
                {
                    var dist = distributionTable[k];
                    for (int i = 0; i < dist.Length; i++)
                        ret.Increment(k, i, (uint)(samples * dist[i]));
                }

                return ret;
            }

            public override void Write(
                Utf8JsonWriter writer,
                Rule ruleValue,
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
                foreach (var k in ruleValue.ConfigurationKeys)
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
