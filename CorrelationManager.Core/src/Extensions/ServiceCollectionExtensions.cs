using System;
using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
            
            services.AddScoped<ICorrelationManager, CorrelationManager.Core.Services.CorrelationManager>();
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