using System.Text.Json;
using System.Text.Json.Serialization;

namespace McdisTest.Data
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

        public static UserEvent? FromJson(string json)
        {
            try
            {
                UserEvent? userEvent = JsonSerializer.Deserialize<UserEvent>(json);
                return userEvent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка при десериализации UserEvent:"+ex.Message);
                return null;
            }
        }
    }
}
