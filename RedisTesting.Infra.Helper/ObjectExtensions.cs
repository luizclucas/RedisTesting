using Newtonsoft.Json;

namespace RedisTesting.Infra.Helper
{
    public static class ObjectExtensions
    {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        public static string ToJson(this object o, JsonSerializerSettings settings = null)
        {
            return JsonConvert.SerializeObject(o, settings ?? DefaultSerializerSettings);
        }

        /// <summary>
        /// Deserializes a JSON string into an object.
        /// </summary>
        public static T FromJsonTo<T>(this string s, JsonSerializerSettings settings = null)
            where T : class, new()
        {
            return JsonConvert.DeserializeObject<T>(s ?? "", settings ?? DefaultSerializerSettings) ?? new T();
        }

        public static T Clone<T>(this object o, JsonSerializerSettings settings = null) where T : class, new()
        {
            return o.ToJson().FromJsonTo<T>();
        }
    }
}
