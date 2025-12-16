using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.FirebaseDatabase
{
    /// <summary>
    /// Общие опции JSON для десериализации данных из Firebase
    /// </summary>
    public static class JsonOptions
    {
        public static JsonSerializerOptions Default { get; } = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}

