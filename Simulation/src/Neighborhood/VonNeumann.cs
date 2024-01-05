
using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Data;

namespace Simulation
{
    [JsonConverter(typeof(Json.VonNeumannConverter))]
    public class VonNeumann : Neighborhood2D
    {
        public uint Radius = 1;

        public VonNeumann(uint radius = 1, uint lookBack = 0, int rows = 1, int cols = 1)
        {
            this.Rows = rows;
            this.Columns = cols;
            this.Radius = radius;
            this.LookBack = lookBack;
        }

        public override uint Count()
        {
            uint ret = 1 + (Radius * 4);
            for (uint i = 1; i < Radius; i++) ret += i * 4;
            return ret * (LookBack + 1);
        }

        public override State[] Get(Container.Array<State>.View[] states, int index)
        {
            var count = Count();
            State[] configuration = new State[count];
            var segment = new ArraySegment<Container.Array<State>.View>
                (states, 0, Math.Min((int)LookBack + 1, states.Length));

            int idx = 0;

            int row = GetRowFromKey(index);
            int column = GetColumnFromKey(index);

            foreach (var state in segment)
            {
                configuration[idx] = state.Get(index);
                idx++;

                for (int i = 1; i <= Radius; i++)
                    for (int j = 0; j < i; j++)
                    {
                        var r = -i + j;
                        var c = j;
                        configuration[idx] = state.Get(GetKeyFromCoords(row + r, column + c));
                        idx++;

                        configuration[idx] = state.Get(GetKeyFromCoords(row + c, column - r));
                        idx++;

                        configuration[idx] = state.Get(GetKeyFromCoords(row - r, column - c));
                        idx++;

                        configuration[idx] = state.Get(GetKeyFromCoords(row - c, column + r));
                        idx++;
                    }
            }

            return configuration;
        }

        public override State[] Get(Container.Grid2D<State>[] states, int row, int column)
        {
            var count = Count();
            State[] configuration = new State[count];
            var segment = new ArraySegment<Container.Grid2D<State>>
                (states, 0, Math.Min((int)LookBack + 1, states.Length));

            int idx = 0;

            foreach (var state in segment)
            {
                configuration[idx] = state.Get(row, column);
                idx++;

                for (int i = 1; i <= Radius; i++)
                    for (int j = 0; j < i; j++)
                    {
                        var r = -i + j;
                        var c = j;
                        configuration[idx] = state.Get(row + r, column + c);
                        idx++;

                        configuration[idx] = state.Get(row + c, column - r);
                        idx++;

                        configuration[idx] = state.Get(row - r, column - c);
                        idx++;

                        configuration[idx] = state.Get(row - c, column + r);
                        idx++;
                    }
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
