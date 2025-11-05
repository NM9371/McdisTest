using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

public class KafkaConsumerTests
{
    [Theory]
    [InlineData("""
            {
              "userId": 123,
              "eventType": "click",
              "timestamp": "2025-04-16T12:34:56Z",
              "data": {
                "buttonId": "submit"
                }
            }
            """, 
        123, 
        "click")]
    [InlineData("""
            {
              "userId": 456,
              "eventType": "hover",
              "timestamp": "2025-04-16T12:34:56Z",
              "data": {
                "formId": "card",
                "fieldId": "email"
                }
            }
            """, 
        456, 
        "hover")]
    [InlineData("""
            {
              "userId": 123,
              "eventType": "click",
              "timestamp": "2025-04-16T12:34:56Z",
              "data": {}
            }
            """, 
        123, 
        "click")]
    public void DeserializeUserEvent_ShouldDeserializeValidJson(string json, int userId, string eventType)
    {
        //Arrange
        var loggerMock = new Mock<ILogger<KafkaConsumer>>();

        //Act
        var result = KafkaConsumer.DeserializeUserEvent(json, loggerMock.Object);

        //Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.EventType.Should().Be(eventType);
    }


    [Theory]
    [InlineData("""
            {
              "userId": "123",
              "eventType": "click",
              "timestamp": "2025-04-16T12:34:56Z",
              "data": {
                "buttonId": "submit"
                }
            }
            """)]
    [InlineData("""
            {
              "userId": 123,
              "eventType": 456,
              "timestamp": "2025-04-16T12:34:56Z",
              "data": {
                "buttonId": "ok"
                }
            }
            """)]
    [InlineData("")]
    public void DeserializeUserEvent_ShouldReturnNull_WhenJsonInvalid(string json)
    {
        //Arrange
        var loggerMock = new Mock<ILogger<KafkaConsumer>>();

        //Act
        var result = KafkaConsumer.DeserializeUserEvent(json, loggerMock.Object);

        //Assert
        result.Should().BeNull();
    }
}