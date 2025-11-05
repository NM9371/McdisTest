using McdisTest.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace McdisTest.Data
{
    public class UserEventStatsPGStorage : IUserEventStatsStorage
    {
        private readonly string _connectionString;
        private readonly ILogger<UserEventStatsPGStorage> _logger;

        public UserEventStatsPGStorage(IConfiguration configuration, ILogger<UserEventStatsPGStorage> logger)
        {
            _logger = logger;

            var conn = configuration["POSTGRES_CONNECTION"];
            if (string.IsNullOrEmpty(conn))
            {
                _logger.LogCritical("Переменная окружения POSTGRES_CONNECTION не найдена");
                throw new InvalidOperationException("Переменная окружения POSTGRES_CONNECTION не найдена");
            }
            _connectionString = conn;
        }

        public async Task SaveStatsAsync(UserEvent userEvent)
        {
            const string sql = @"   
                INSERT INTO user_event_stats(user_id, event_type, count)
                VALUES (@user_id, @event_type, 1)
                ON CONFLICT (user_id, event_type)
                DO UPDATE SET count = user_event_stats.count + 1;
            ";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await conn.OpenAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("PGSQL: Произошла ошибка во время открытия подключения: "+ex.Message);
                }

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("user_id", userEvent.UserId);
                    cmd.Parameters.AddWithValue("event_type", userEvent.EventType);
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                        _logger.LogInformation($"PGSQL: В БД сохранено новое событие {userEvent.EventType} от пользователя {userEvent.UserId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("PGSQL: Произошла ошибка при сохранении данных: "+ex.Message);
                    }
                }
            }
        }

        public async Task CreateStatsTableAsync()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS user_event_stats (
                   user_id INT NOT NULL,
                   event_type VARCHAR(50) NOT NULL,
                   count INT NOT NULL,
                   PRIMARY KEY (user_id, event_type)
                 );";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    await conn.OpenAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("PGSQL: Произошла ошибка во время открытия подключения: "+ex.Message);
                }

                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    try
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("PGSQL: Произошла ошибка при создании таблицы: "+ex.Message);
                    }
                }
            }
        }
    }
}
