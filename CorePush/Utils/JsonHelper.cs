using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using CorePush.Utils;

[assembly: Dependency(nameof(JsonHelper), LoadHint.Default)]
namespace CorePush.Utils
{
    public class JsonHelper : IJsonHelper
    {
        private readonly JsonSerializerOptions settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, settings);
        }

        public TObject Deserialize<TObject>(string json)
        {
            return JsonSerializer.Deserialize<TObject>(json);//, settings);
        }
    }
}