using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OI.JobProcessing.Models;
using Testcontainers.PostgreSql;

namespace OI.Web.Tests.Int
{
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("longrunning")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        public Task InitializeAsync()
        {
            return dbContainer.StartAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Configure DI
            builder.ConfigureTestServices(services => {
                
                // Find the DB Context
                var descriptor = services.SingleOrDefault(s =>
                   s.ServiceType == typeof(DbContextOptions<LongrunningContext>));

                if (descriptor != null)
                { 
                    services.Remove(descriptor);
                }

                services.AddDbContext<LongrunningContext>(options => {

                    options.UseNpgsql(dbContainer.GetConnectionString());
                });
            });
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            return dbContainer.StopAsync();
        }
    }
}
