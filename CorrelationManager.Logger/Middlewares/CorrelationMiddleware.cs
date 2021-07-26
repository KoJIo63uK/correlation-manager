using System;
using System.Linq;
using System.Threading.Tasks;
using CorrelationManager.Core.Constants;
using CorrelationManager.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace CorrelationManager.Logger.Middlewares
{
    /// <summary>
    /// Catch and save correlation header
    /// </summary>
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public Task InvokeAsync(HttpContext context, ICorrelationManager correlationManager)
        {
            var correlationHeader = context.Request.Headers
                .SingleOrDefault(h => String.Equals(h.Key, Headers.CORRELATION_ID,
                    StringComparison.InvariantCultureIgnoreCase)).Value;

            if (!string.IsNullOrWhiteSpace(correlationHeader)) correlationManager.CorrelationId = correlationHeader;
            
            // ensures all entries are tagged with some values
            using (correlationManager.CreateScope())
            {
                context.Response.OnStarting(_ =>
                {
                    context.Response.Headers.Add(correlationManager.CorrelationHeader.Key,
                        correlationManager.CorrelationHeader.Value.ToString());
                
                    return Task.CompletedTask;
                }, context);
                // Call the next delegate/middleware in the pipeline
                return _next(context);
            }
        }
    }
}