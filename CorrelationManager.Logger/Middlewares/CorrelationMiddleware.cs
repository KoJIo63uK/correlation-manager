using System;
using System.Linq;
using System.Threading.Tasks;
using CorrelationManager.Core.Constants;
using CorrelationManager.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CorrelationManager.Logger.Middlewares
{
    /// <summary>
    /// Catch and save correlation header
    /// </summary>
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationMiddleware> _logger;

        public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        
        public Task InvokeAsync(HttpContext context, ICorrelationManager correlationManager)
        {
            var correlationHeader = context.Request.Headers
                .SingleOrDefault(h => String.Equals(h.Key, Headers.CORRELATION_ID,
                    StringComparison.InvariantCultureIgnoreCase)).Value;

            if (!string.IsNullOrWhiteSpace(correlationHeader)) correlationManager.CorrelationId = correlationHeader;
            
            // ensures all entries are tagged with some values
            using (_logger.BeginScope(correlationManager.CorrelationHeader))
            {
                // Call the next delegate/middleware in the pipeline
                return _next(context);
            }
        }
    }
}