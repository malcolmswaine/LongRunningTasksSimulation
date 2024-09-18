using Microsoft.AspNetCore.SignalR;

namespace OneIncTest
{
    public class LongRunningTask(
        ILogger<LongRunningTask> logger, 
        IHubContext<JobsHub> hubContext
        )
    {
        Random random = new();

        public async Task ExecuteAsync(CancellationToken cancellationToken, string sigrConnId, string stringToConvert)
        {
            logger.LogInformation($"Starting conversion job for string: {stringToConvert}");

            for(int i = 0; i < stringToConvert.Length; i++) {
                

                // User cancelled task
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogInformation($"job-cancelled {i}");
                    await hubContext.Clients.Client(sigrConnId).SendAsync("job-cancelled", i);
                   
                    return;
                }
                else
                {
                    // Process a job step
                    var delay = random.Next(1, 5);
                    await Task.Delay(delay * 1000, cancellationToken);

                    // We might have cancelled during the delay
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        var currentLetter = stringToConvert[i];
                        await hubContext.Clients.Client(sigrConnId).SendAsync("job-processing-step", currentLetter);
                        logger.LogInformation($"job-processing-step {currentLetter}");
                    }
                }
            }

            // We've finished processing
            await hubContext.Clients.Client(sigrConnId).SendAsync("job-complete");
            logger.LogInformation("job-complete");

        }
    }
}
