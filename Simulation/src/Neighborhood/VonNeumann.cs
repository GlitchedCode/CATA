
using System;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Simulation
{
    [JsonConverter(typeof(Json.VonNeumannConverter))]
    public class VonNeumann : Neighborhood
    {
        public uint Radius = 1;

        public VonNeumann(uint radius = 1)
        {
            this.Radius = radius;
        }

        public override uint Count()
        {
            uint ret = 1 + (Radius * 4);
            for (uint i = 1; i < Radius; i++) ret += i * 4;
            return ret;
        }

        public override State[] Get(Grid2DView<State> gridState, int row, int column)
        {
            var count = Count();
            State[] configuration = new State[count];

            configuration[0] = gridState.Get(row, column);
            int idx = 1;

            for (int i = 1; i <= Radius; i++)
                for (int j = 0; j < i; j++)
                {
                    var r = -i + j;
                    var c = j;
                    configuration[idx] = gridState.Get(row + r, column + c);
                    idx++;

                    configuration[idx] = gridState.Get(row + c, column - r);
                    idx++;

                    configuration[idx] = gridState.Get(row - r, column - c);
                    idx++;

                    configuration[idx] = gridState.Get(row - c, column + r);
                    idx++;
                }


            return configuration;
        }

        public State[] Convert(State[] input, State defaultValue)
        {
            var count = Count();
            var ret = Enumerable.Range(1, (int)Count()).Select(_ => (State)defaultValue.Clone()).ToArray();
            Array.Copy(input, 0, ret, 0, input.Length);
            return ret;
        }

        public override ConfigurationKey ConvertKey(ConfigurationKey input, State defaultValue)
        {
            var config = Enumerable.Range(1, (int)Count()).Select(_ => (State)defaultValue.Clone()).ToArray();
            var ret = Encode(config);

            Array.Copy(input.Bytes, 0, ret.Bytes, 0, Math.Min(input.Bytes.Length, ret.Bytes.Length));

            return ret;
        }
    }

    namespace Json
    {

        public class VonNeumannConverter : JsonConverter<VonNeumann>
        {
            public override VonNeumann Read(
                    ref Utf8JsonReader reader,
                    Type typeToConvert,
                    JsonSerializerOptions options
                )
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                VonNeumann ret = null;
                var validated = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        switch (reader.GetString())
                        {
                            case "Radius":
                                ret = new VonNeumann(reader.GetUInt32());
                                break;
                            case "Type":
                                if (reader.GetString() == "VonNeumann")
                                    validated = true;
                                break;
                            default:
                                break;
                        }
                    }
                }

                if (ret != null && validated)
                    return ret;

                throw new JsonException("invalid json object");
            }

            public override void Write(
                Utf8JsonWriter writer,
                VonNeumann vonNeumannValue,
                JsonSerializerOptions options
            )
            {
                writer.WriteStartArray();

                writer.WriteStringValue("VonNeumann");

                writer.WriteStartObject();
                writer.WriteNumber("Radius", vonNeumannValue.Radius);
                writer.WriteEndObject();

                writer.WriteEndArray();
            }

        }

    }
}
