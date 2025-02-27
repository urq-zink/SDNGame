using Newtonsoft.Json;
using System.Numerics;

namespace SDNGame.Utils
{
    public class Vector4Converter : JsonConverter<Vector4>
    {
        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            reader.Read();
            reader.Read();
            float x = (float)(reader.ReadAsDouble() ?? 0f);
            reader.Read();
            float y = (float)(reader.ReadAsDouble() ?? 0f);
            reader.Read();
            float z = (float)(reader.ReadAsDouble() ?? 0f);
            reader.Read();
            float w = (float)(reader.ReadAsDouble() ?? 0f);
            reader.Read();
            return new Vector4(x, y, z, w);
        }

        public override void WriteJson(JsonWriter writer, Vector4 value, Newtonsoft.Json.JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WritePropertyName("Z");
            writer.WriteValue(value.Z);
            writer.WritePropertyName("W");
            writer.WriteValue(value.W);
            writer.WriteEndObject();
        }
    }
}
