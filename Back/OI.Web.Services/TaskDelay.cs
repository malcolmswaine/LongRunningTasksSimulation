﻿namespace OI.Web.Services
{
    public class TaskDelay : ITaskDelay
    {
        public async Task Delay(int lengthInMilliSeconds)
        { 
            await Task.Delay(lengthInMilliSeconds).ConfigureAwait(false);
            return;
        }
    }

    public interface ITaskDelay
    {
        Task Delay(int lengthInMilliSeconds);
    }
}
