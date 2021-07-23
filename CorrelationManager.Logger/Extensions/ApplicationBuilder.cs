using CorrelationManager.Logger.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace CorrelationManager.Logger.Extensions
{
    public static class ApplicationBuilder
    {
        /// <summary>
        /// Registration correlation middlware
        /// </summary>
        /// <param name="app"></param>
        public static void UseCorrelationLogger(this IApplicationBuilder app)
        {
            app.UseMiddleware<CorrelationMiddleware>();
        }
    }
}