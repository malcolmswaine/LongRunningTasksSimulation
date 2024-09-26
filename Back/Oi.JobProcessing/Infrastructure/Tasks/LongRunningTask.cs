using Hangfire.Server;
using Oi.JobProcessing.Infrastructure.Tasks;
using Oi.Lib.Shared;
using OI.JobProcessing.Infrastructure.Exceptions;

namespace Oi.JobProcessing.Infrastructure.Jobs
{
    [HangfireExceptionHandler]
    public class LongRunningTask(ILogger<LongRunningTask> logger,
        ICheckPoint checkPoint,
        ITaskDelay taskDelay,
        IMessageBusPublisher messageBusPublisher)
    {
        Random random = new();

        [HangfireExceptionHandler]
        public async Task<string> ExecuteAsync(
            CancellationToken cancellationToken,
            string originalString,
            string encodedString,
            string sigrConnId,
            PerformContext context)
        {
            // Hangfire job id is a string type...
            string jobId = context.BackgroundJob.Id;
            string sentToClient = "";

            logger.LogInformation($"Starting job {jobId}");

            // Save a record of the job starting
            checkPoint.JobStart(int.Parse(jobId), originalString, encodedString);

            // Process each step
            for (int i = 0; i < encodedString.Length; i++)
            {

                try
                {
                    // User cancelled task
                    if (cancellationToken.IsCancellationRequested)
                    {
                        checkPoint.JobProgress(int.Parse(jobId), JobStateEnum.Cancelled, sentToClient);
                        //await hubContext.Clients.Client(sigrConnId).SendAsync("job-cancelled", i);
                        messageBusPublisher.SendMessage(MessageTypeEnum.JobCancelResponse, sentToClient, sigrConnId);

                        return sentToClient;
                    }
                    else
                    {
                        // Random wait
                        var delay = random.Next(1, 5);
                        await taskDelay.Delay(delay * 1000);

                        // We might have cancelled during the delay
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            // Process a job step
                            var currentLetter = encodedString[i];
                            sentToClient += currentLetter;

                            messageBusPublisher.SendMessage(MessageTypeEnum.JobProcessingStepResponse, currentLetter.ToString(), sigrConnId);
                            checkPoint.JobProgress(int.Parse(jobId), JobStateEnum.Running, sentToClient);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception thrown running job steps", null);
                    checkPoint.JobProgress(int.Parse(jobId), JobStateEnum.Error, e.ToString());
                    // await hubContext.Clients.Client(sigrConnId).SendAsync("job-error", "Server Error");
                    messageBusPublisher.SendMessage(MessageTypeEnum.JobError, "Server Error", sigrConnId);
                    return sentToClient;
                }
            }

            // We've finished processing
            //await hubContext.Clients.Client(sigrConnId).SendAsync("job-complete");
            checkPoint.JobProgress(int.Parse(jobId), JobStateEnum.Complete, sentToClient);
            messageBusPublisher.SendMessage(MessageTypeEnum.JobComplete, sentToClient, sigrConnId);

            return sentToClient;
        }
    }

}
