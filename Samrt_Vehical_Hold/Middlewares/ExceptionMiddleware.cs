using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Samrt_Vehical_Hold.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // Handle Not Found manually (404)
                if (context.Response.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    await HandleExceptionAsync(context, null, HttpStatusCode.NotFound);
                }
                else if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    await HandleExceptionAsync(context, null, HttpStatusCode.Unauthorized);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unhandled Exception: {ex.Message}");
                await HandleExceptionAsync(context, ex, HttpStatusCode.InternalServerError);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var result = JsonSerializer.Serialize(new
            {
                status = statusCode,
                error = statusCode switch
                {
                    HttpStatusCode.NotFound => "The requested resource was not found.",
                    HttpStatusCode.Unauthorized => "Unauthorized access.",
                    _ => exception?.Message ?? "An unexpected error occurred."
                },
                detail = exception?.InnerException?.Message
            });

            return context.Response.WriteAsync(result);
        }
    }
}
