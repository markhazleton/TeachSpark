using TeachSpark.Web.Services.Implementations;
using TeachSpark.Web.Services.Interfaces;

namespace TeachSpark.Web.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the CRUD services with the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddCrudServices(this IServiceCollection services)
        {
            // Register the service implementations
            services.AddScoped<IWorksheetService, WorksheetService>();
            services.AddScoped<ICommonCoreStandardService, CommonCoreStandardService>();
            services.AddScoped<IWorksheetTemplateService, WorksheetTemplateService>();

            return services;
        }
    }
}
