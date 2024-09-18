using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OneIncTest;
using OneIncTest.Services;

Console.WriteLine("--> Starting Job Server");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

// Why hangfire?
// I want to be able to keep job state, offer job management and if moved 'out of process', scalability.
builder.Services.AddHangfire(
    config => config.UsePostgreSqlStorage(
        options => options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("HangfireDatabase"))));
builder.Services.AddHangfireServer(options => options.SchedulePollingInterval = TimeSpan.FromSeconds(1));

// We need to keep a record of the running tasks in memory in case we need to cancel them
builder.Services.AddTransient<LongRunningTask>();
builder.Services.AddSingleton<LongRunningTasks>();

// We're going to signal the clients - will try to use sockets, but downgrade to long polling on failure
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Create a new job
app.MapPost("Jobs", async (
        string sigrConnId,
        [FromBody] ConversionPayload conversionPayload,
        IBackgroundJobClient backgroundJobClient, 
        IHubContext<JobsHub> hubContext,
        LongRunningTasks longRunningTasks) =>
{
    CancellationTokenSource cancellationTokenSource = new();

    // Start the job
    string jobId = backgroundJobClient.Enqueue<LongRunningTask>(job => 
        job.ExecuteAsync(
            cancellationTokenSource.Token, 
            sigrConnId,
            StringTransformService.Base64Encode(conversionPayload.stringToConvert)));

    // We need to map the jobs to the cancellation tokens
    longRunningTasks.Tasks.Add(jobId, cancellationTokenSource);

    // Let the caller know, we've started the job
    await hubContext.Clients.Client(sigrConnId).SendAsync("ReceiveNotification", $"Started processing job with ID: {jobId}");

    return Results.AcceptedAtRoute("JobDetails", new { jobId }, jobId);
});


// Get the details of a current job
app.MapGet("jobs/{jobId}", (string jobId) =>
{
    var jobDetails = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
    return jobDetails.History.OrderByDescending(h => h.CreatedAt).First().StateName;
})
.WithName("JobDetails");


// Cancel a job (tempted to use delete verb, but not acutally deleting the entity)
app.MapPut("jobs/{jobId}", async (string jobId,
    [FromQuery] string sigrConnId,
    LongRunningTasks longRunningTasks, 
    IHubContext<JobsHub> hubContext,
    ILogger<LongRunningTask> logger ) =>
{
    // TODO - use token to validate we are the owner

    var jobDetails = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
    if (jobDetails != null)
    {
        try
        {
            longRunningTasks.Tasks[jobId].Cancel();            
            var result = BackgroundJob.Delete(jobId);

            //await hubContext.Clients.Client(hubContext.)

            await hubContext.Clients.Client(sigrConnId).SendAsync("job-cancelled", jobId);
            logger.LogInformation($"job-cancelled: {jobId}");
        }
        catch (Exception e)
        {
            await hubContext.Clients.Client(sigrConnId).SendAsync("job-error", e.ToString());
            logger.LogInformation($"job-error {e.ToString()}");
        }

    }
    else
    {
        return Results.NotFound();
    }

    return Results.Ok("cancelled");

})
.WithName("cancel");


app.UseWebSockets();
app.UseRouting();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.MapHub<JobsHub>("jobshub");

app.Run();

record ConversionPayload(string stringToConvert);
