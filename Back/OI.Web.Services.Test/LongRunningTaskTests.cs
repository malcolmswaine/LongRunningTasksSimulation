using Hangfire.Server;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using OI.Web.Services.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Any;
using OI.Web.Services.Infrastructure;

namespace OI.Web.Services.Test
{
    public class Tests
    {

        /// <summary>
        /// Prove that the string transformation process is sound
        /// </summary>
        [Test]
        public void TransformString_WhenTransformed_ExpectBase64()
        {
            var testString = "Hello World!";
            var expected = "SGVsbG8gV29ybGQh";

            var transformed = StringTransformer.Base64Encode(testString);

            Assert.That(transformed, Is.EqualTo(expected));
        }

        /// <summary>
        /// Run the background job and check the string output 
        /// sent to the mock client is correct
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task RunBackgroundTask_WhenComplete_ExpectConvertedString()
        {
            var testString = "Goodbye World!";
            var expected = "R29vZGJ5ZSBXb3JsZCE=";

            // Arrange
            var logger = new NullLogger<LongRunningTask>();
            var mockHub = new Mock<IHubContext<JobsHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClient = new Mock<ISingleClientProxy>();

            mockClients.Setup(clients => clients.Client(It.IsAny<string>())).Returns(mockClient.Object);
            mockHub.Setup(hub => hub.Clients).Returns(mockClients.Object);

            var checkPointMock = new Mock<ICheckPoint>();
            checkPointMock.Setup(m => m.JobStart(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

            // We don't really need a delay at unit test level
            var taskDelay = new Mock<ITaskDelay>();
            taskDelay.Setup(m => m.Delay(It.IsAny<int>()));

            var cancellationToken = new CancellationTokenSource().Token;            


            // Act
            var testLongRunningTask = new LongRunningTask(logger,
                mockHub.Object, checkPointMock.Object, taskDelay.Object);

            var transformed = StringTransformer.Base64Encode(testString);

            var processedString = await testLongRunningTask.ExecuteAsync(
                cancellationToken,
                "",
                testString,
                transformed,
                null);


            // Assert
            Assert.That(checkPointMock.Invocations.Count(), Is.EqualTo(22));
            Assert.That(mockClient.Invocations.Count(), Is.EqualTo(21));
            Assert.That(processedString, Is.EqualTo(expected));
        }


        [Test]
        public async Task RunBackgroundTaskWithCancelledSet_WhenComplete_ExpectNoStringConversion()
        {
            var testString = "Hi World!";
            var expected = "";

            // Arrange
            var logger = new NullLogger<LongRunningTask>();
            var mockHub = new Mock<IHubContext<JobsHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClient = new Mock<ISingleClientProxy>();

            mockClients.Setup(clients => clients.Client(It.IsAny<string>())).Returns(mockClient.Object);
            mockHub.Setup(hub => hub.Clients).Returns(mockClients.Object);

            var checkPointMock = new Mock<ICheckPoint>();
            checkPointMock.Setup(m => m.JobStart(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()));

            // We don't really need a delay at unit test level
            var taskDelay = new Mock<ITaskDelay>();
            taskDelay.Setup(m => m.Delay(It.IsAny<int>()));

            // We want to be able to cancel
            CancellationTokenSource cancellationTokenSource = new();
            

            // Act
            var testLongRunningTask = new LongRunningTask(logger,
                mockHub.Object, checkPointMock.Object, taskDelay.Object);

            var transformed = StringTransformer.Base64Encode(testString);

            // cancel the job
            cancellationTokenSource.Cancel();

            var processedString = await testLongRunningTask.ExecuteAsync(
                cancellationTokenSource.Token,
                "",
                testString,
                transformed,
                null);


            // Assert
            Assert.That(mockClient.Invocations.Count(), Is.EqualTo(1)); // we told the client we cancelled
            Assert.That(processedString, Is.EqualTo(expected));
        }
    }
}