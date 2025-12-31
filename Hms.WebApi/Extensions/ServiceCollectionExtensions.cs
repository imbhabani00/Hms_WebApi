using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using NetCore.AutoRegisterDi;

namespace Hms.WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHMSApplicationServices(this IServiceCollection services)
        {
            var serviceAssembly = typeof(Hms.Service.IUserService).Assembly;
            var repositoryAssembly = typeof(Hms.Repository.IUserRepository).Assembly;
            var domainAssembly = typeof(Hms.Domains.User.User).Assembly;
            var helperAssembly = typeof(Hms.Helpers.AccessCodeHelper).Assembly;

            services.RegisterAssemblyPublicNonGenericClasses(serviceAssembly)
                .Where(c => c.Name.EndsWith("Service"))
                .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            services.RegisterAssemblyPublicNonGenericClasses(repositoryAssembly)
                .Where(c => c.Name.EndsWith("Repository"))
                .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);

            var assemblies = new[]
            {
                serviceAssembly,
                repositoryAssembly,
                domainAssembly,
                helperAssembly
            }.Distinct().ToArray();

            services.AddAutoMapper(assemblies);

            return services;
        }
    }
}