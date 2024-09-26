using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Oi.JobProcessing.Infrastructure.Jobs;
using Oi.JobProcessing.Infrastructure.Tasks;
using Oi.Lib.Shared;
using OI.JobProcessing.Infrastructure;
using OI.Web.Services;

namespace OI.Web.Tests.Int
{
    public class LongRunningTaskTests : BaseIntegrationTest
    {
        private NullLogger<LongRunningTask> logger;
        private Mock<IHubContext<JobsHub>> mockHub;
        private Mock<IHubClients> mockClients;
        private Mock<ISingleClientProxy> mockClient;
        private Mock<ITaskDelay> taskDelay;
        private CancellationToken cancellationToken;
        private Mock<IStorageConnection> storageConnection;
        private Mock<IJobCancellationToken> jobCancellationToken;
        private Mock<IMessageBusPublisher> messageBusPublisher;

        public LongRunningTaskTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
            logger = new NullLogger<LongRunningTask>();
            mockHub = new Mock<IHubContext<JobsHub>>();
            mockClients = new Mock<IHubClients>();
            mockClient = new Mock<ISingleClientProxy>();
            messageBusPublisher = new Mock<IMessageBusPublisher>();
            cancellationToken = new CancellationTokenSource().Token;

            mockClients.Setup(clients => clients.Client(It.IsAny<string>())).Returns(mockClient.Object);
            mockHub.Setup(hub => hub.Clients).Returns(mockClients.Object);

            // We don't really need a delay at unit test level
            taskDelay = new Mock<ITaskDelay>();
            taskDelay.Setup(m => m.Delay(It.IsAny<int>()));

            storageConnection = new Mock<IStorageConnection>();
            jobCancellationToken = new Mock<IJobCancellationToken>();

            messageBusPublisher.Setup(m => m.SendMessage(It.IsAny<MessageTypeEnum>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        [Fact]
        public async Task RunBackgroundTask_WhenComplete_ExpectCheckpointsToHaveUpdatedDatabase()
        {
            var testString = "hey there world!";
            var expected = "aGV5IHRoZXJlIHdvcmxkIQ==";

            // Arrange
            // We want a real checkpoint so we can write to a real DB
            var checkpointLogger = new NullLogger<CheckPoint>();
            var checkpoint = new CheckPoint(checkpointLogger, dbContext);

            var backgroundJob = new BackgroundJob("1", null, DateTime.Now);
            var performContext = new PerformContext(storageConnection.Object, backgroundJob, jobCancellationToken.Object);


            // Act
            var testLongRunningTask = new LongRunningTask(logger,
                checkpoint, taskDelay.Object, messageBusPublisher.Object);

            var transformed = StringTransformer.Base64Encode(testString);

            var processedString = await testLongRunningTask.ExecuteAsync(
                cancellationToken,
                testString,
                transformed,
                "",
                performContext);


            // Assert
            var jobRecord = dbContext.Oijobs.First(j => j.JobId == 1);
            Assert.Equal(jobRecord.OriginalString, testString);
            Assert.Equal(jobRecord.EncodedString, expected);
            Assert.Equal(jobRecord.ReturnedData, expected);
            Assert.Equal(jobRecord.JobStateId, (int)Oi.Lib.Shared.JobStateEnum.Complete);

            Assert.Equal(expected, processedString);
        }


        [Fact]
        public async Task CancelBackgroundTask_ExpectJobDatabaseRecordToBeInCancelledState()
        {
            var testString = "dobry den world!";
            var expected = "ZG9icnkgZGVuIHdvcmxkIQ==";

            // Arrange
            // We want a real checkpoint so we can write to a real DB
            var checkpointLogger = new NullLogger<CheckPoint>();
            var checkpoint = new CheckPoint(checkpointLogger, dbContext);

            var backgroundJob = new BackgroundJob("2", null, DateTime.Now);
            var performContext = new PerformContext(storageConnection.Object, backgroundJob, jobCancellationToken.Object);


            // We want to cancel
            CancellationTokenSource cancellationTokenSource = new();

            // Act
            var testLongRunningTask = new LongRunningTask(logger,
                checkpoint, taskDelay.Object, messageBusPublisher.Object);

            var transformed = StringTransformer.Base64Encode(testString);

            // cancel the job
            cancellationTokenSource.Cancel();

            var processedString = await testLongRunningTask.ExecuteAsync(
                cancellationTokenSource.Token,
                testString,
                transformed,
                "",
                performContext);


            // Assert
            var jobRecord = dbContext.Oijobs.First(j => j.JobId == 2);
            Assert.Equal(jobRecord.OriginalString, testString);
            Assert.Equal(jobRecord.EncodedString, expected);
            Assert.Equal(jobRecord.ReturnedData, String.Empty);
            Assert.Equal(jobRecord.JobStateId, (int)Oi.Lib.Shared.JobStateEnum.Cancelled);

            Assert.Equal(String.Empty, processedString);
        }
    }
}