using Confluent.Kafka;
using McdisTest.Models;
using McdisTest.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public class KafkaConsumer
{
    private readonly ILogger<KafkaConsumer> _logger;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IEventObservable _eventObservable;

    public KafkaConsumer(IEventObservable eventObservable, IConfiguration configuration, ILogger<KafkaConsumer> logger)
    {
        _logger = logger;

        var bootstrapServers = configuration["KAFKA_BOOTSTRAP_SERVERS"];
        var topic = configuration["KAFKA_TOPIC"];
        var groupId = configuration["KAFKA_GROUP_ID"];

        if (string.IsNullOrEmpty(bootstrapServers))
        {
            _logger.LogCritical("Переменная окружения KAFKA_BOOTSTRAP_SERVERS не найдена");
            throw new InvalidOperationException("Переменная окружения KAFKA_BOOTSTRAP_SERVERS не найдена");
        }
        if (string.IsNullOrEmpty(topic))
        {
            _logger.LogCritical("Переменная окружения KAFKA_TOPIC не найдена");
            throw new InvalidOperationException("Переменная окружения KAFKA_TOPIC не найдена");
        }
        if (string.IsNullOrEmpty(groupId))
        {
            _logger.LogCritical("Переменная окружения KAFKA_GROUP_ID не найдена");
            throw new InvalidOperationException("Переменная окружения KAFKA_GROUP_ID не найдена");
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe(topic);
        _logger.LogInformation("Ожидание событий в топике Kafka: " + topic);

        _eventObservable = eventObservable;
    }

    public void ProcessNextMessage(CancellationToken ct)
    {
        try
        {
            var result = _consumer.Consume(ct);

            var userEvent = DeserializeUserEvent(result.Message.Value, _logger);
            if (userEvent != null)
            {
                _logger.LogInformation("Публикация события в Observable");
                _eventObservable.Publish(userEvent);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("KafkaConsumer остановлен через токен отмены.");
            _consumer.Close();
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Kafka: Произошла ошибка при подключении для чтения: " + ex.Message);
            throw new Exception("Kafka: Произошла ошибка при подключении для чтения: " + ex.Message);
        }
    }

    public static UserEvent? DeserializeUserEvent(string json, ILogger<KafkaConsumer> logger)
    {
        try
        {
            UserEvent? userEvent = JsonSerializer.Deserialize<UserEvent>(json);
            if (userEvent == null)
            {
                logger.LogWarning("Некорректный JSON UserEvent:" + json);
            }
            return userEvent;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Произошла ошибка при десериализации UserEvent:" + ex.Message);
            return null;
        }
    }
}