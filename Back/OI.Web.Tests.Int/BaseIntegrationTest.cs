using Microsoft.Extensions.DependencyInjection;
using OI.Web.Services.Models;

namespace OI.Web.Tests.Int
{

    public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly IServiceScope scope;
        protected readonly LongrunningContext dbContext;

        public BaseIntegrationTest(IntegrationTestWebAppFactory factory)
        {
            scope = factory.Services.CreateScope();
            dbContext = scope.ServiceProvider.GetRequiredService<LongrunningContext>();
        }
    }
}
