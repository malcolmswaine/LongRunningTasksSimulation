using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Oi.JobProcessing.Infrastructure.EventProcessing;
using Oi.JobProcessing.Infrastructure.Jobs;
using Oi.JobProcessing.Infrastructure.Tasks;
using Oi.Lib.Shared;
using OI.JobProcessing.Infrastructure.Exceptions;
using OI.JobProcessing.Models;
using Prometheus;

Console.WriteLine("--> Starting Job Processor");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();


// Add a DB Context
var dbConnectionSting = builder.Configuration.GetConnectionString("HangfireDatabase");
builder.Services.AddDbContext<LongrunningContext>(options => options.UseNpgsql(dbConnectionSting));



builder.Services.AddTransient<ITaskDelay, TaskDelay>();
builder.Services.AddTransient<ICheckPoint, CheckPoint>();
builder.Services.AddTransient<IEventProcessor, EventProcessor>();
builder.Services.AddTransient<LongRunningTask>();
builder.Services.AddTransient<IMessageBusPublisher, MessageBusPublisher>();
// We need to keep a record of the running tasks in memory in case we need to cancel them
builder.Services.AddSingleton<LongRunningTasks>();


// Global exception handling service
builder.Services.AddExceptionHandler<HttpGlobalExceptionHandler>();
builder.Services.AddSingleton<HubGlobalExceptionFilter>();
builder.Services.AddProblemDetails();

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


builder.Services.AddSingleton<MessageBusConsumer>();
builder.Services.AddHostedService<MessageBusConsumer>(p => p.GetRequiredService<MessageBusConsumer>());


var app = builder.Build();

// Prometheus Logging
app.UseHttpMetrics();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// For demo let's keep it simple and migrate every time
using (var scope = app.Services.CreateScope())
{
    var eventProcessor = scope.ServiceProvider.GetRequiredService<IEventProcessor>();
    var rabbitConsumer = scope.ServiceProvider.GetRequiredService<MessageBusConsumer>();
    rabbitConsumer.AddEventProcessor(eventProcessor);


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

app.UseExceptionHandler();

app.UseWebSockets();
app.UseRouting(); //.UseEndpoints(endpoints => endpoints.MapMetrics()); // Map Prometheus

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseHangfireDashboard();

app.MapMetrics(); // Prometheus endpoint

app.Run();

public partial class Program { } // Expose for in tests
