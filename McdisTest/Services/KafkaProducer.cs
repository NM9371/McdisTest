using Confluent.Kafka;

namespace McdisTest.Services
{
    public class KafkaProducer
    {
        private readonly string _bootstrapServers;

        public KafkaProducer(string bootstrapServers)
        {
            _bootstrapServers = bootstrapServers;
        }

        public async Task SendTestMessageAsync(string topicName, string message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers
            };

            IProducer <Null, string> producer;
            try
            {
                producer = new ProducerBuilder<Null, string>(config).Build();
            }
            catch (Exception ex)
            {
                throw new Exception("Kafka: Произошла ошибка при подключении для публикации: "+ex.Message);
            }

            using (producer)
            {
                try
                {
                    var result = await producer.ProduceAsync(topicName, new Message<Null, string> { Value = message });
                    Console.WriteLine($"Создано событие в топике Kafka {result.TopicPartitionOffset}: {message}");
                }
                catch (Exception ex)
                {
                    throw new Exception("Kafka: Произошла ошибка при публикации сообщения: "+ex.Message);
                }
            }
        }
                
    }
}
