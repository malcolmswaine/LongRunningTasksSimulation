using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace OI.Web.Services.Infrastructure
{
    

    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) =>
            _logger = logger;


        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, $"Exception ocurred: {exception.Message}");


            httpContext.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;


            ProblemDetails problem = new()
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "Server error",
                Title = "Server error",
                Detail = "An internal server has occurred"
            };


            string json = JsonSerializer.Serialize(problem);
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(json);

            return true;
        }
    }
}
