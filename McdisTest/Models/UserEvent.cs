using System.Text.Json;
using System.Text.Json.Serialization;

namespace McdisTest.Models
{
    public class UserEvent
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }
    }
}
