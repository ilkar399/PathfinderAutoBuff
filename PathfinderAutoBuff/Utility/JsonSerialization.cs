using System.Linq;
using Newtonsoft.Json;
using Kingmaker.EntitySystem.Persistence.JsonUtility;

namespace PathfinderAutoBuff.Utility
{
    static class JsonSerializationSetup
    {
        private static JsonSerializerSettings cachedSettings;
        internal static JsonSerializerSettings SerializerSettings
        {
            get
            {
                if (cachedSettings == null)
                {
                    cachedSettings = new JsonSerializerSettings
                    {
                        Converters = DefaultJsonSettings.CommonConverters.ToList<JsonConverter>(),
                        ContractResolver = new OptInContractResolver(),
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        DefaultValueHandling = DefaultValueHandling.Include,
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Auto,
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                    };
                }
                return cachedSettings;
            }
        }
    }
}
