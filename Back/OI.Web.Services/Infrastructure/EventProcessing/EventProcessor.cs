using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Oi.Lib.Shared;
using Oi.Lib.Shared.Types;
using OI.Web.Services;
using System.Text.Json;
using System.Threading;

namespace OI.Web.Services.Infrastructure.EventProcessing
{
    public class EventProcessor(IHubContext<JobsHub> hubContext) : IEventProcessor
    {
        
        public async void ProcessEvent(GenericMessage message)
        {
            Console.WriteLine($"ProcessEvent {message.ToString()}");

            string jobId = "";
            string sigrConnId = "";

            switch (message.MessageType)
            {
                case MessageTypeEnum.JobCreationResponse:

                    jobId = message.Payload;
                    sigrConnId = message.SigrConnId;
                    
                    await hubContext.Clients.Client(sigrConnId).SendAsync("job-started", jobId);


                    break;
                case MessageTypeEnum.JobProcessingStepResponse:

                    jobId = message.Payload;
                    sigrConnId = message.SigrConnId;
                    await hubContext.Clients.Client(sigrConnId).SendAsync("job-processing-step", jobId);


                    break;
                case MessageTypeEnum.JobCancelResponse:

                    jobId = message.Payload;
                    sigrConnId = message.SigrConnId;
                    await hubContext.Clients.Client(sigrConnId).SendAsync("job-cancelled", jobId);

                    break;

                case MessageTypeEnum.JobComplete:

                    jobId = message.Payload;
                    sigrConnId = message.SigrConnId;
                    await hubContext.Clients.Client(sigrConnId).SendAsync("job-complete", jobId);

                    break;

                case MessageTypeEnum.JobError:

                    var error = message.Payload;
                    sigrConnId = message.SigrConnId;
                    await hubContext.Clients.Client(sigrConnId).SendAsync("job-error", error);

                    break;
            }


        }


    }


}
