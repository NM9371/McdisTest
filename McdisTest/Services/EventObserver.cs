using McdisTest.Data;
using McdisTest.Models;
using Microsoft.Extensions.Logging;

namespace McdisTest.Services
{
    public class EventObserver : IObserver<UserEvent>
    {
        private readonly IUserEventStatsStorage _storage;
        private readonly ILogger<EventObserver> _logger;

        public EventObserver(IUserEventStatsStorage storage, ILogger<EventObserver> logger)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _logger = logger;
        }

        public void OnCompleted()
        {
            _logger.LogInformation("Наблюдатель: Работа завершена");
        }

        public void OnError(Exception error)
        {
            _logger.LogInformation("Наблюдатель: Произошла ошибка:"+error.Message);
        }

        public void OnNext(UserEvent value)
        {
            _logger.LogInformation($"Наблюдатель: Получено новое событие {value.EventType} от пользователя {value.UserId} в {value.Timestamp}");
            Task.Run(async () => await _storage.SaveStatsAsync(value));
        }
    }
}
