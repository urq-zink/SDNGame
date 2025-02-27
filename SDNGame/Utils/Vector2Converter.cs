using Newtonsoft.Json;
using System.Numerics;

namespace SDNGame.Utils
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            reader.Read();
            reader.Read();
            float x = (float)(reader.ReadAsDouble() ?? 0f);
            reader.Read();
            float y = (float)(reader.ReadAsDouble() ?? 0f);
            reader.Read();
            return new Vector2(x, y);
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WriteEndObject();
        }
    }
}
