using McdisTest.Data;
using McdisTest.Services;
using System.Text.Json;

// Берем настройки подключения Kafka и PostgreSQL из переменных окружения
var bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS");
var topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC");
var groupId = Environment.GetEnvironmentVariable("KAFKA_GROUP_ID");
var pgConnection = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION");
if (bootstrapServers == null || topic == null || groupId == null)
{
    throw new Exception("Не удалось получить настройки Kafka из переменных окружения");
}
if (pgConnection == null)
{
    throw new Exception("Не удалось получить настройки PostgreSQL из переменных окружения");
}

// Создаём хранилище
var storage = new DataStorage(pgConnection);

// Observable и Observer
var observable = new EventObservable();
var observer = new EventObserver(storage);
observable.Subscribe(observer);

// Producer для отправки тестовых сообщений
var producer = new KafkaProducer(bootstrapServers);

// Создаём таблицу в PGSQL, если не существует
await storage.CreateStatsTableAsync();

// Отправляем тестовые JSON-сообщения
var userEventJson = JsonSerializer.Serialize(new
{
    userId = 123,
    eventType = "click",
    timestamp = DateTime.UtcNow,
    data = new { buttonId = "submit" }
});

await producer.SendTestMessageAsync(topic, userEventJson);
await producer.SendTestMessageAsync(topic, userEventJson);
await producer.SendTestMessageAsync(topic, userEventJson);
userEventJson = JsonSerializer.Serialize(new
{
    userId = 123,
    eventType = "pop",
    timestamp = DateTime.UtcNow,
    data = new { buttonId = "submit" }
});

await producer.SendTestMessageAsync(topic, userEventJson);
userEventJson = JsonSerializer.Serialize(new
{
    userId = 321,
    eventType = "pop",
    timestamp = DateTime.UtcNow,
    data = new { buttonId = "submit" }
});

await producer.SendTestMessageAsync(topic, userEventJson);

// Consumer
var consumer = new KafkaConsumer(bootstrapServers, topic, groupId, observable);
consumer.Start();
