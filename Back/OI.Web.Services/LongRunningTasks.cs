namespace OI.Web.Services
{
    public class LongRunningTasks
    {
        public Dictionary<string, CancellationTokenSource> Tasks { get; set; } = new();
    }
}
