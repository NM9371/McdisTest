using McdisTest.Data;

namespace McdisTest.Services
{
    public class EventObserver : IObserver<UserEvent>
    {
        private readonly string _name;
        private readonly DataStorage _storage;

        public EventObserver(DataStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public void OnCompleted()
        {
            Console.WriteLine("Наблюдатель: Работа завершена");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("Наблюдатель: Произошла ошибка:"+error.Message);
        }

        public void OnNext(UserEvent value)
        {
            Console.WriteLine($"Наблюдатель: Получено новое событие {value.EventType} от пользователя {value.UserId} в {value.Timestamp}");
            Task.Run(async () => await _storage.SaveStatsAsync(value));
        }
    }
}
