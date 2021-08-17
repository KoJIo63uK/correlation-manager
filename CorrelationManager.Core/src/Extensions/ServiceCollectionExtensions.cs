using System;
using CorrelationManager.Core.Handlers;
using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Options;
using CorrelationManager.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;

namespace CorrelationManager.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add and configure correlation manager
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public static void AddCorrelationManager(
            this IServiceCollection services,
            Action<CorrelationManagerOptions> options = null)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            
            if (options != null)
            {
                services.ConfigureCorrelationManager(options);
            }
            else
            {
                services.Configure<CorrelationManagerOptions>(
                    configuration.GetSection(CorrelationManagerOptions.CORRELATION_MANAGER_SECTION));
            }
            
            services.TryAddSingleton(s => 
                s.GetRequiredService<IOptions<CorrelationManagerOptions>>().Value);
            
            services.AddScoped<ICorrelationManager, Services.CorrelationManager>();
            services.AddSingleton<ICorrelationManagerFactory, CorrelationManagerFactory>();
            
            services.AddScoped<HttpClientRequestHandler>();
            services.AddHttpContextAccessor();
            services.ConfigureAll<HttpClientFactoryOptions>(factoryOptions =>
            {
                factoryOptions.HttpMessageHandlerBuilderActions.Add(builder =>
                {
                    builder.AdditionalHandlers.Add(builder.Services.GetRequiredService<HttpClientRequestHandler>());
                });
            });
        }

        /// <summary>
        /// Configure correlation manager
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        public static void ConfigureCorrelationManager(
            this IServiceCollection services,
            Action<CorrelationManagerOptions> options)
        {
            services.Configure(options);
        }
    }
}