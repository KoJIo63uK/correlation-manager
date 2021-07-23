using CorrelationManager.Logger.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace CorrelationManager.Logger.Extensions
{
    public static class ApplicationBuilder
    {
        /// <summary>
        /// Registration correlation middleware
        /// </summary>
        /// <param name="app"></param>
        public static void UseCorrelationLogger(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationMiddleware>();
        }
        /// <summary>
        /// Registration request logging middleware
        /// </summary>
        /// <param name="app"></param>
        public static void UseRequestLogger(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}