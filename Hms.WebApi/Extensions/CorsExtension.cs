namespace Hms.WebApi.Extensions
{
    public static class CorsExtension
    {
        public static void AddCustomCors(this IServiceCollection services, string policyName)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                {
                    builder
                        .WithOrigins(
                        //IIs Express
                        "http://localhost:11215",     // WebApp IIS Express HTTP
                        "https://localhost:44346",    // WebApp IIS Express HTTPS
                        "http://localhost:11214",     // API IIS Express HTTP
                        "https://localhost:44347"

                        //Https setup
                        //"http://localhost:5173",        // WebApp HTTP
                        //"https://localhost:7174",       // WebApp HTTPS  
                        //"http://localhost:5262",        // API HTTP
                        //"https://localhost:7173"        // API HTTPS
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
        }
    }
}
