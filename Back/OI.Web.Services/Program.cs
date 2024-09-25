using Ganss.Xss;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OI.Web.Services;
using OI.Web.Services.Infrastructure;
using OI.Web.Services.Infrastructure.Exceptions;
using OI.Web.Services.Models;
using Prometheus;

Console.WriteLine("--> Starting Job Server");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();


// Add a DB Context
var dbConnectionSting = builder.Configuration.GetConnectionString("HangfireDatabase");
builder.Services.AddDbContext<LongrunningContext>(options => options.UseNpgsql(dbConnectionSting));


// Why hangfire?
// Offers some out-of-the box features for job retry on failure, monitoring, management and persisting background job state
builder.Services.AddHangfire(
    config => config.UsePostgreSqlStorage(
        options => options.UseNpgsqlConnection(dbConnectionSting)));
builder.Services.AddHangfireServer(
    options => {
        options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
        }
    );

// We don't want this for demo if we're stopping and starting local dev server often
GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute() { Attempts = 0 });


builder.Services.AddTransient<ITaskDelay, TaskDelay>();
builder.Services.AddTransient<ICheckPoint, CheckPoint>();
builder.Services.AddTransient<LongRunningTask>();
// We need to keep a record of the running tasks in memory in case we need to cancel them
builder.Services.AddSingleton<LongRunningTasks>();
// Global exception handling service
builder.Services.AddExceptionHandler<HttpGlobalExceptionHandler>();
builder.Services.AddSingleton<HubGlobalExceptionFilter>();
builder.Services.AddProblemDetails();


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

// Prometheus Logging
app.UseHttpMetrics();

// For demo let's keep it simple and migrate every time
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LongrunningContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception e)
    {
        Console.WriteLine($"Warning: migrating data returned response {e.ToString()}");
    }
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

    // remove any nasties
    var sanitizer = new HtmlSanitizer();
    var safeStringToConvert = sanitizer.Sanitize(conversionPayload.stringToConvert);

    // Set on client, shouldn't happen
    if (safeStringToConvert.Length > 1000)
    {
        return Results.BadRequest("String length exceeded");
    }

    // Start the job
    string jobId = backgroundJobClient.Enqueue<LongRunningTask>(job => 
        job.ExecuteAsync(
            cancellationTokenSource.Token, 
            sigrConnId,
            safeStringToConvert,
            StringTransformer.Base64Encode(safeStringToConvert), 
            null));

    // We need to map the jobs to the cancellation tokens
    bool added = longRunningTasks.Tasks.TryAdd(jobId, cancellationTokenSource);
    if (!added)
    { 
        return Results.BadRequest("Error mapping task to connection");
    }

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
    var jobDetails = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
    if (jobDetails != null)
    {
        try
        {
            longRunningTasks.Tasks[jobId].Cancel();            
            var result = BackgroundJob.Delete(jobId);

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

app.UseExceptionHandler();

app.UseWebSockets();
app.UseRouting(); //.UseEndpoints(endpoints => endpoints.MapMetrics()); // Map Prometheus

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.MapHub<JobsHub>("jobshub");
app.MapMetrics(); // Prometheus endpoint


app.Run();

public partial class Program { } // Expose for in tests
record ConversionPayload(string stringToConvert);
