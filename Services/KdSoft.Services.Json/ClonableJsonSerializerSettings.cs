using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KdSoft.Services.Json
{
    public class ClonableJsonSerializerSettings: JsonSerializerSettings
    {
        public ClonableJsonSerializerSettings(IContractResolver contractResolver = null) {
            // duplicate settings from MVC: SerializerSettingsProvider.CreateSerializerSettings();
            if (contractResolver != null)
                ContractResolver = contractResolver;
            MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
            MaxDepth = 32;
        }

        public ClonableJsonSerializerSettings Clone() {
            return (ClonableJsonSerializerSettings)MemberwiseClone();
        }
    }
}
