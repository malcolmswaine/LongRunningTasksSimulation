using System.Collections.Concurrent;

namespace Oi.JobProcessing.Infrastructure.Jobs
{
    public class LongRunningTasks
    {
        public ConcurrentDictionary<string, CancellationTokenSource> Tasks { get; set; } = new();
    }
}
