using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;

namespace OI.Web.Services.Infrastructure.Exceptions
{
    // https://docs.hangfire.io/en/latest/extensibility/using-job-filters.html

    public class HangfireExceptionHandler : JobFilterAttribute, IElectStateFilter
    {
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public void OnStateElection(ElectStateContext context)
        {
            var failedState = context.CandidateState as FailedState;
            if (failedState != null)
            {
                Logger.WarnFormat(
                    "Job `{0}` has been failed due to an exception `{1}`",
                    context.BackgroundJob.Id,
                    failedState.Exception);
            }
        }

    }
}
