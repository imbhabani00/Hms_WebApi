using System.Reflection;

namespace Hms.WebApi.Extensions
{
    public static class AutoMapperExtension
    {
        public static IServiceCollection AddCustomAutoMapper(this IServiceCollection services)
        {
            return services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }
    }
}
