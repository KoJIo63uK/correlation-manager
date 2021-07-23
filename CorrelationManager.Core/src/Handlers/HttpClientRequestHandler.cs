using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CorrelationManager.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CorrelationManager.Core.Handlers
{
    /// <summary>
    /// Add correlation header to http requests
    /// </summary>
    public class HttpClientRequestHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly ICorrelationManager _correlationManager;

        public HttpClientRequestHandler(IHttpContextAccessor accessor, ICorrelationManager correlationManager)
        {
            _accessor = accessor;
            _correlationManager = correlationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var correlationManager = _accessor.HttpContext != null ? 
                _accessor.HttpContext.RequestServices.GetRequiredService<ICorrelationManager>() : 
                _correlationManager;
            
            if (!request.Headers.Contains(correlationManager.CorrelationHeader.Key))
            {
                request.Headers.Add(correlationManager.CorrelationHeader.Key,
                    correlationManager.CorrelationHeader.Value.ToString());
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}