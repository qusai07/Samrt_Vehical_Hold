using Microsoft.AspNetCore.Builder;

namespace Samrt_Vehical_Hold.Startup
{
    public static class MiddlewareConfig
    {
        public static void UseGlobalMiddlewares(this WebApplication app)
        {
            app.UseCors("AllowLocalNetwork");

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Request from {context.Connection.RemoteIpAddress} to {context.Request.Path}");
                await next();
            });

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }
        }
    }
}
