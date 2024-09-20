using Microsoft.Extensions.Logging;
using OI.Web.Services.Models.Enums;
using OI.Web.Services.Models;

namespace OI.Web.Services
{
    public class CheckPoint(ILogger<CheckPoint> logger) : ICheckPoint
    {
        // Save audit of job progress state
        public void JobProgress(int jobId, JobStateEnum jobState, string sentToClient)
        {
            using (var context = new LongrunningContext())
            {
                var job = context.Oijobs.FirstOrDefault(x => x.JobId == jobId);
                if (job != null)
                {
                    job.JobStateId = (int)jobState;
                    job.ReturnedData = sentToClient;
                    job.UpdatedDateTime = DateTime.UtcNow;
                    context.SaveChanges();
                }

                logger.LogInformation($"job-processing-step sent {sentToClient}");
            }
        }

        // Save a audit of the job starting state
        public void JobStart(int jobId, string originalString, string encodedString)
        {
            using (var context = new LongrunningContext())
            {
                Oijob ioj = new()
                {
                    JobId = jobId,
                    OriginalString = originalString,
                    EncodedString = encodedString,
                    CreatedDateTime = DateTime.UtcNow,
                    UpdatedDateTime = DateTime.UtcNow,
                    JobStateId = (int)JobStateEnum.Ready,
                };

                try
                {
                    context.Oijobs.Add(ioj);
                    context.SaveChanges();
                    logger.LogInformation($"Starting conversion job {jobId} with string: {encodedString}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.ToString());
                    throw; // The client should know something has gone wrong
                }

            }
        }
    }

    public interface ICheckPoint
    {
        public void JobStart(int jobId, string originalString, string encodedString);
        void JobProgress(int jobId, JobStateEnum jobState, string sentToClient);

    }
}
