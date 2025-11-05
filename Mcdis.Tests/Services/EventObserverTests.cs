using McdisTest.Data;
using McdisTest.Models;
using McdisTest.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace McdisTest.Tests.Services
{
    public class EventObserverTests
    {
        private static IConfiguration BuildConfig(string? start = null, string? end = null)
        {
            var dict = new Dictionary<string, string?>
            {
                { "EVENT_FILTER_START_DATE", start },
                { "EVENT_FILTER_END_DATE", end }
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        }

        private static UserEvent BuildEvent(DateTime timestamp)
        {
            return new UserEvent
            {
                UserId = 1,
                EventType = "click",
                Timestamp = timestamp
            };
        }

        [Fact]
        public async Task OnNext_Should_Save_When_NoFilter()
        {
            // Arrange
            var storageMock = new Mock<IUserEventStatsStorage>();
            var loggerMock = new Mock<ILogger<EventObserver>>();
            var config = BuildConfig();

            var observer = new EventObserver(config, storageMock.Object, loggerMock.Object);
            var evt = BuildEvent(DateTime.UtcNow);

            // Act
            observer.OnNext(evt);
            await Task.Delay(50);

            // Assert
            storageMock.Verify(s => s.SaveStatsAsync(It.IsAny<UserEvent>()), Times.Once);
        }

        [Fact]
        public async Task OnNext_Should_Filter_Event_Before_StartDate()
        {
            // Arrange
            var start = DateTime.UtcNow;
            var config = BuildConfig(start: start.ToString("O"));
            var storageMock = new Mock<IUserEventStatsStorage>();
            var loggerMock = new Mock<ILogger<EventObserver>>();
            var observer = new EventObserver(config, storageMock.Object, loggerMock.Object);

            var evt = BuildEvent(start.AddMinutes(-10));

            // Act
            observer.OnNext(evt);
            await Task.Delay(50);

            // Assert
            storageMock.Verify(s => s.SaveStatsAsync(It.IsAny<UserEvent>()), Times.Never);
        }

        [Fact]
        public async Task OnNext_Should_Filter_Event_After_EndDate()
        {
            // Arrange
            var end = DateTime.UtcNow;
            var config = BuildConfig(end: end.ToString("O"));
            var storageMock = new Mock<IUserEventStatsStorage>();
            var loggerMock = new Mock<ILogger<EventObserver>>();
            var observer = new EventObserver(config, storageMock.Object, loggerMock.Object);

            var evt = BuildEvent(end.AddMinutes(1000));

            // Act
            observer.OnNext(evt);
            await Task.Delay(50);

            // Assert
            storageMock.Verify(s => s.SaveStatsAsync(It.IsAny<UserEvent>()), Times.Never);
        }

        [Fact]
        public async Task OnNext_Should_Pass_Event_Between_Dates()
        {
            // Arrange
            var start = DateTime.UtcNow.AddMinutes(-1000);
            var end = DateTime.UtcNow.AddMinutes(1000);
            var config = BuildConfig(start: start.ToString("O"), end: end.ToString("O"));
            var storageMock = new Mock<IUserEventStatsStorage>();
            var loggerMock = new Mock<ILogger<EventObserver>>();
            var observer = new EventObserver(config, storageMock.Object, loggerMock.Object);
            var evt = BuildEvent(DateTime.UtcNow);

            // Act
            observer.OnNext(evt);
            await Task.Delay(50);

            // Assert
            storageMock.Verify(s => s.SaveStatsAsync(It.IsAny<UserEvent>()), Times.Once);
        }
    }
}
