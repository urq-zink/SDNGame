using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SDNGame.Utils;

namespace SDNGame.Serialization
{
    public class Serializer : ISerializable
    {
        private readonly Dictionary<string, Type> _typeRegistry = new();
        private readonly JsonSerializerSettings _settings;

        public Serializer()
        {
            _settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new Vector2Converter(), new Vector4Converter() }
            };

            RegisterType("Transform", typeof(Core.Transform));
            RegisterType("Scene", typeof(Scenes.Scene));
        }

        public void RegisterType(string key, Type type)
        {
            if (!typeof(ISerializable).IsAssignableFrom(type))
                throw new ArgumentException($"Type {type.Name} must implemented ISerializable.");
            _typeRegistry[key] = type;
        }

        public void Save(string filePath, Dictionary<string, ISerializable> objects)
        {
            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(stream);
            using var writer = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };

            writer.WriteStartObject();
            writer.WritePropertyName("Objects");
            writer.WriteStartObject();

            foreach (var (key, obj) in objects)
            {
                writer.WritePropertyName(key);
                writer.WriteStartObject();
                writer.WritePropertyName("Type");
                writer.WriteValue(_typeRegistry.FirstOrDefault(x => x.Value == obj.GetType()).Key);
                writer.WritePropertyName("Data");
                obj.Serialize(writer);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        public Dictionary<string, ISerializable> Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Save file not found: {filePath}");

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);

            var root = JObject.Load(reader);
            var objects = root["Objects"] as JObject;
            var result = new Dictionary<string, ISerializable>();

            foreach (var property in objects.Properties())
            {
                string key = property.Name;
                string typeName = property.Value["Type"].Value<string>();
                var dataToken = property.Value["Data"];

                if (!_typeRegistry.TryGetValue(typeName, out Type type))
                    throw new InvalidOperationException($"Unknown type: {typeName}");

                var instance = (ISerializable)Activator.CreateInstance(type);
                using var dataReader = new JsonTextReader(new StringReader(dataToken.ToString()));
                instance.Deserialize(dataReader);
                result[key] = instance;
            }

            return result;
        }

        public void Serialize(JsonWriter writer)
        {
        }

        public void Deserialize(JsonReader element)
        {
        }
    }
}
