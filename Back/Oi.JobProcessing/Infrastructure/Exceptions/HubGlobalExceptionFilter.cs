using Microsoft.AspNetCore.SignalR;

namespace OI.JobProcessing.Infrastructure.Exceptions
{
    public class HubGlobalExceptionFilter(ILogger<HubGlobalExceptionFilter> logger) : IHubFilter
    {
        public async ValueTask<object> InvokeMethodAsync(
            HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            try
            {
                return await next(invocationContext);
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception calling '{invocationContext.HubMethodName}': {ex}");
                throw;
            }
        }
    }
}
