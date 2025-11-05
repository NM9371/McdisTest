using McdisTest.Data;
using McdisTest.Models;
using McdisTest.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;
        services.AddSingleton<IConfiguration>(configuration);

        services.AddTransient<IUserEventStatsStorage, UserEventStatsPGStorage>();

        services.AddSingleton<IEventObservable, EventObservable>();
        services.AddSingleton<IObserver<UserEvent>, EventObserver>();

        services.AddSingleton<KafkaConsumer>();
        services.AddHostedService<KafkaBackgroundService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    var observable = services.GetRequiredService<IEventObservable>();
    var observer = services.GetRequiredService<IObserver<UserEvent>>();
    var storage = services.GetRequiredService<IUserEventStatsStorage>();

    // Выполняем подписку наблюдателя на наблюдаемого
    logger.LogInformation("Выполняется подписка наблюдателя на наблюдаемого");
    observable.Subscribe(observer);

    // Создаём таблицу в PGSQL, если не существует
    await storage.CreateStatsTableAsync();
}

await host.RunAsync();