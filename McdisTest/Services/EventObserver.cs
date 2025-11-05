using McdisTest.Data;
using McdisTest.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace McdisTest.Services
{
    public class EventObserver : IObserver<UserEvent>
    {
        private readonly IUserEventStatsStorage _storage;
        private readonly ILogger<EventObserver> _logger;
        private readonly DateTime? _filterStartDate = null;
        private readonly DateTime? _filterEndDate = null;


        public EventObserver(IConfiguration configuration, IUserEventStatsStorage storage, ILogger<EventObserver> logger)
        {
            if (DateTime.TryParse(configuration["EVENT_FILTER_START_DATE"], out var start))
                _filterStartDate = DateTime.SpecifyKind(start, DateTimeKind.Utc);

            if (DateTime.TryParse(configuration["EVENT_FILTER_END_DATE"], out var end))
                _filterEndDate = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            _storage = storage;
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

        public void OnNext(UserEvent userEvent)
        {
            if (!EventFilter(userEvent))
            {
                _logger.LogInformation($"Наблюдатель: Событие {userEvent.EventType} от пользователя {userEvent.UserId} в {userEvent.Timestamp} отфильтровано");
                return;
            }

            _logger.LogInformation($"Наблюдатель: Получено новое событие {userEvent.EventType} от пользователя {userEvent.UserId} в {userEvent.Timestamp}");
            Task.Run(async () => await _storage.SaveStatsAsync(userEvent));
        }

        private bool EventFilter(UserEvent userEvent)
        {
            if (_filterStartDate == null && _filterEndDate == null)
                return true;

            var ts = userEvent.Timestamp;

            if (_filterStartDate != null && ts < _filterStartDate.Value)
                return false;

            if (_filterEndDate != null && ts > _filterEndDate.Value)
                return false;

            return true;
        }
    }
}
