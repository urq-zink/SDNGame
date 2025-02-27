using Newtonsoft.Json;

namespace SDNGame.Serialization
{
    public interface ISerializable
    {
        void Serialize(JsonWriter writer);
        void Deserialize(JsonReader element);
    }
}
