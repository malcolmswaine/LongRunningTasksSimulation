using Hangfire;
using Oi.Lib.Shared;
using Oi.Lib.Shared.Types;
using OI.JobProcessing.Infrastructure;
using System.Text.Json;
using System.Threading;
using Oi.JobProcessing.Infrastructure.Jobs;

namespace Oi.JobProcessing.Infrastructure.EventProcessing
{
    public class EventProcessor(IMessageBusPublisher messageBusPublisher, 
        LongRunningTasks longRunningTasks,
        IServiceScopeFactory scopeFactory) : IEventProcessor
    {
        
        public void ProcessEvent(GenericMessage message)
        {
            Console.WriteLine($"ProcessEvent {message.ToString()}");

            switch (message.MessageType)
            {
                case MessageTypeEnum.JobCreationRequest:

                    using (var scope = scopeFactory.CreateScope())
                    {
                        var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();

                        CancellationTokenSource cancellationTokenSource = new();

                        string jobId = backgroundJobClient.Enqueue<LongRunningTask>(job =>
                            job.ExecuteAsync(
                                cancellationTokenSource.Token,
                                message.Payload,
                                StringTransformer.Base64Encode(message.Payload),
                                message.SigrConnId,
                                null));

                        bool added = longRunningTasks.Tasks.TryAdd(jobId, cancellationTokenSource);
                        
                        if (!added)
                        {
                            messageBusPublisher.SendMessage(MessageTypeEnum.JobError, "Error mapping task to cancellation token", message.SigrConnId);
                        }
                        else
                        {
                            messageBusPublisher.SendMessage(MessageTypeEnum.JobCreationResponse, jobId, message.SigrConnId);
                        }

                        
                    }
                  break;
                case MessageTypeEnum.JobCancelRequest:

                    using (var scope = scopeFactory.CreateScope())
                    {
                        var backgroundJobClient = scope.ServiceProvider.GetRequiredService<IBackgroundJobClient>();
                        var jobId = message.Payload;
                        var cancellationToken = longRunningTasks.Tasks[jobId];

                        var result = BackgroundJob.Delete(jobId);                        

                        messageBusPublisher.SendMessage(MessageTypeEnum.JobCancelResponse, jobId, message.SigrConnId);
                    }
                    break;
            } 
        }
    }
}
