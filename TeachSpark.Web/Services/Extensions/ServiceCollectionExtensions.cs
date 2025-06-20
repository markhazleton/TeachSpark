using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using TeachSpark.Web.Configuration;
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
        }        /// <summary>
                 /// Registers LLM and worksheet generation services with the dependency injection container.
                 /// Call this method to prepare for LLM integration (services will be implemented later).
                 /// </summary>
                 /// <param name="services">The service collection to add services to.</param>
                 /// <param name="configuration">The configuration to bind settings from.</param>
                 /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddWorksheetGenerationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind configuration sections
            services.Configure<LlmConfiguration>(configuration.GetSection(LlmConfiguration.SectionName));
            services.Configure<WorksheetGenerationConfiguration>(configuration.GetSection(WorksheetGenerationConfiguration.SectionName));

            // Register LLM usage metrics as singleton for tracking across requests
            services.AddSingleton<LlmUsageMetrics>();

            // Register memory cache for LLM response caching
            services.AddMemoryCache();            // Register model registry service
            services.AddSingleton<IModelRegistryService, OpenAIModelRegistryService>();

            // Register Markdown rendering service
            services.AddScoped<MarkdownRenderingService>();            // Register OpenAI client for model registry
            services.AddSingleton<OpenAI.OpenAIClient>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<LlmConfiguration>>().Value;
                var apiKey = config.ApiKey;

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("OpenAI API key not configured. Please set LlmConfiguration:ApiKey in appsettings.json or user secrets.");
                }

                return new OpenAI.OpenAIClient(apiKey);
            });

            // Register OpenAI chat client using Microsoft.Extensions.AI
            services.AddSingleton<IChatClient>(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<LlmConfiguration>>().Value;
                var openAiClient = serviceProvider.GetRequiredService<OpenAI.OpenAIClient>();

                return openAiClient.AsChatClient(config.DefaultModel);
            });

            // Register LLM service implementations
            services.AddScoped<ILlmService, OpenAILlmService>();

            // TODO: Implement these services
            // services.AddScoped<IPromptEngineService, PromptEngineService>();
            // services.AddScoped<IContentValidationService, ContentValidationService>();
            // services.AddScoped<IWorksheetGenerationService, WorksheetGenerationService>();

            return services;
        }
    }
}
