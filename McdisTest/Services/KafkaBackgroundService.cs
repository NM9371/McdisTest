using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class KafkaBackgroundService : BackgroundService
{
    private readonly KafkaConsumer _consumer;
    private readonly ILogger<KafkaBackgroundService> _logger;

    public KafkaBackgroundService(KafkaConsumer consumer, ILogger<KafkaBackgroundService> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                _consumer.ProcessNextMessage(ct);
                // Таймаут перед повторным чтением сообщений
                await Task.Delay(100, ct);
            }
        }
        catch(Exception ex)
        {
            _logger.LogCritical("KafkaBackgroundService: Произошла ошибка при запуске KafkaConsumer: " + ex.Message);
            throw new Exception("KafkaBackgroundService: Произошла ошибка при запуске KafkaConsumer: " + ex.Message);
        }
    }
}
