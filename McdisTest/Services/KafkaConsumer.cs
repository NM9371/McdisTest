using Confluent.Kafka;
using McdisTest.Data;
using System.Text.Json;

namespace McdisTest.Services
{
    public class KafkaConsumer
    {
        private readonly string _bootstrapServers;
        private readonly string _topic;
        private readonly string _groupId;
        private readonly EventObservable _eventObservable;

        public KafkaConsumer(string bootstrapServers, string topic, string groupId, EventObservable eventObservable)
        {
            _bootstrapServers = bootstrapServers;
            _topic = topic;
            _groupId = groupId;
            _eventObservable = eventObservable;
        }

        public void Start()
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = _groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };


            IConsumer<Ignore, string> consumer;
            try
            {
                consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            }
            catch (Exception ex)
            {
                throw new Exception("Kafka: Произошла ошибка при подключении для чтения: " + ex.Message);
            }

            try
            {
                consumer.Subscribe(_topic);
                Console.WriteLine($"Ожидание событий в топике Kafka: " + _topic);
            }
            catch (Exception ex)
            {
                throw new Exception("Kafka: Произошла ошибка при подписке на топик: " + ex.Message);
            }

            try
            {
                while (true)
                {
                    var result = consumer.Consume();

                    // Парсим JSON-сообщение в UserEvent
                    var userEvent = DeserializeUserEvent(result.Message.Value);

                    if (userEvent != null)
                    {
                        // Публикуем событие в observable
                        _eventObservable.Publish(userEvent);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }

        public static UserEvent? DeserializeUserEvent(string json)
        {
            try
            {
                UserEvent? userEvent = JsonSerializer.Deserialize<UserEvent>(json);
                if (userEvent == null)
                {
                    Console.WriteLine("Некорректный JSON UserEvent:" + json);
                }
                return userEvent;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка при десериализации UserEvent:" + ex.Message);
                return null;
            }
        }
    }
}