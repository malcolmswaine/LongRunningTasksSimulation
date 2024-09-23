using Hangfire.Server;
using Microsoft.AspNetCore.SignalR;
using OI.Web.Services.Models;
using OI.Web.Services.Models.Enums;

namespace OI.Web.Services
{
    public class LongRunningTask(ILogger<LongRunningTask> logger, 
        IHubContext<JobsHub> hubContext,
        ICheckPoint checkPoint,
        ITaskDelay taskDelay)
    {
        Random random = new();

        public async Task<string> ExecuteAsync(CancellationToken cancellationToken, string sigrConnId,
            string originalString, string encodedString, PerformContext context)
        {
            // Hangfire job id is a string type...
            string jobId = context.BackgroundJob.Id;
            string sentToClient = "";

            // Save a record of the job starting
            checkPoint.JobStart(int.Parse(jobId), originalString, encodedString);

            // Process each step
            for (int i = 0; i < encodedString.Length; i++) {

                try
                {
                    // User cancelled task
                    if (cancellationToken.IsCancellationRequested)
                    {
                        checkPoint.JobProgress(int.Parse(jobId), JobStateEnum.Cancelled, sentToClient);
                        await hubContext.Clients.Client(sigrConnId).SendAsync("job-cancelled", i);

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
                            await hubContext.Clients.Client(sigrConnId).SendAsync("job-processing-step", currentLetter, cancellationToken);

                            checkPoint.JobProgress(int.Parse(jobId), JobStateEnum.Running, sentToClient);
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception thrown running job steps", null);
                    checkPoint.JobProgress(int.Parse(jobId), JobStateEnum.Error, e.ToString());
                    await hubContext.Clients.Client(sigrConnId).SendAsync("job-error", "There was an error processing the job");
                }
            }

            // We've finished processing
            await hubContext.Clients.Client(sigrConnId).SendAsync("job-complete");
            checkPoint.JobProgress(int.Parse(jobId), JobStateEnum.Complete, sentToClient);

            return sentToClient;
        }
    }

}
