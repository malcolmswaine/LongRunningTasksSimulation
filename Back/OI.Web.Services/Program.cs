using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Oi.Lib.Shared;
using OI.Web.Services;
using OI.Web.Services.Infrastructure.EventProcessing;
using OI.Web.Services.Infrastructure.Exceptions;
using Prometheus;

Console.WriteLine("--> Starting Job Broker");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();


// Add a DB Context
//var dbConnectionSting = builder.Configuration.GetConnectionString("HangfireDatabase");
//builder.Services.AddDbContext<LongrunningContext>(options => options.UseNpgsql(dbConnectionSting));


// Global exception handling service
builder.Services.AddExceptionHandler<HttpGlobalExceptionHandler>();
builder.Services.AddSingleton<HubGlobalExceptionFilter>();
builder.Services.AddProblemDetails();
builder.Services.AddTransient<IEventProcessor, EventProcessor>();
builder.Services.AddTransient<IMessageBusPublisher, MessageBusPublisher>();

// We need access to this programmatically from DI
builder.Services.AddSingleton<MessageBusConsumer>();
builder.Services.AddHostedService<MessageBusConsumer>(p => p.GetRequiredService<MessageBusConsumer>());


// Use signalling to inform clients of job status
builder.Services.AddSignalR(options =>
{
    options.AddFilter<HubGlobalExceptionFilter>();
});

var app = builder.Build();

// Demo purposes!
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

using (var scope = app.Services.CreateScope())
{
    // Might look at a nicer way of doing this...
    var eventProcessor = scope.ServiceProvider.GetRequiredService<IEventProcessor>();
    var rabbitConsumer = scope.ServiceProvider.GetRequiredService<MessageBusConsumer>();
    rabbitConsumer.AddEventProcessor(eventProcessor);
}

// Prometheus Logging
app.UseHttpMetrics();


// Create a new job
app.MapPost("Jobs", async (
        string sigrConnId,
        [FromBody] ConversionPayload conversionPayload,
        IHubContext<JobsHub> hubContext,
        IMessageBusPublisher messageBusPublisher) =>
{
    // remove any nasties
    var sanitizer = new HtmlSanitizer();
    var safeStringToConvert = sanitizer.Sanitize(conversionPayload.stringToConvert);

    // Set on client, shouldn't happen
    if (safeStringToConvert.Length > 1000)
    {
        return Results.BadRequest("String length exceeded");
    }

    messageBusPublisher.SendMessage(MessageTypeEnum.JobCreationRequest, safeStringToConvert, sigrConnId);
    await hubContext.Clients.Client(sigrConnId).SendAsync("job-requested", "Requested new job creation");

    return Results.Ok(); 
});


// Cancel a job (tempted to use delete verb, but not acutally deleting the entity)
app.MapPut("jobs/{jobId}", async (string jobId,
    [FromQuery] string sigrConnId,
    //LongRunningTasks longRunningTasks, 
    IHubContext<JobsHub> hubContext,
    IMessageBusPublisher messageBusPublisher
    ) =>
{
    messageBusPublisher.SendMessage(MessageTypeEnum.JobCancelRequest, jobId, sigrConnId);

    return Results.Ok("Cancel request sent");

})
.WithName("cancel");

app.UseExceptionHandler();

app.UseWebSockets();
app.UseRouting(); 

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.MapHub<JobsHub>("jobshub");
app.MapMetrics(); // Prometheus endpoint


app.Run();

record ConversionPayload(string stringToConvert);
