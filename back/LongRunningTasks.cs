namespace OneIncTest
{
    public class LongRunningTasks
    {
        public Dictionary<string, CancellationTokenSource> Tasks { get; set; } = new();
    }
}
