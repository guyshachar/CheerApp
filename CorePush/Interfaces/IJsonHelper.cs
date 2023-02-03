//using Newtonsoft.Json;
//using Newtonsoft.Json.Serialization;

namespace CorePush.Interfaces
{
    public interface IJsonHelper
    {
        string Serialize(object obj);

        TObject Deserialize<TObject>(string json);
    }
}