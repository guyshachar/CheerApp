//using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;

namespace CorePush.Utils
{
    public interface IJsonHelper
    {
        string Serialize(object obj);

        TObject Deserialize<TObject>(string json);
    }
}