using System.Collections.Concurrent;

namespace OI.Web.Services
{
    public class LongRunningTasks
    {
        public ConcurrentDictionary<string, CancellationTokenSource> Tasks { get; set; } = new();
    }
}
