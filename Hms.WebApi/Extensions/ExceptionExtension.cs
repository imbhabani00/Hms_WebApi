using Microsoft.AspNetCore.Diagnostics;

namespace Hms.WebApi.Extensions
{
    public static class ExceptionExtension
    {
        public static void UseExceptionHandling(this IApplicationBuilder app,
                ILoggerFactory loggerFactory,
                IWebHostEnvironment env)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json";

                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorFeature != null && errorFeature.Error != null)
                    {
                        var logger = loggerFactory.CreateLogger("GlobalExceptionHandler");
                        logger.LogError(errorFeature.Error, "Unhandled exception occurred");

                        var response = new
                        {
                            StatusCode = 500,
                            Message = env.IsDevelopment()
                                ? errorFeature.Error.Message
                                : "An unexpected error occurred. Please contact support."
                        };

                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
                    }
                });
            });
        }
    }
}

