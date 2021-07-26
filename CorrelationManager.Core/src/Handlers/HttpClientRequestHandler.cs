using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Services;

namespace CorrelationManager.Core.Handlers
{
    /// <summary>
    /// Add correlation header to http requests
    /// </summary>
    public class HttpClientRequestHandler : DelegatingHandler
    {
        private readonly ICorrelationManager _correlationManager;

        public HttpClientRequestHandler(ICorrelationManagerFactory factory)
        {
            _correlationManager = factory.CreateCorrelationManager();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains(_correlationManager.CorrelationHeader.Key))
            {
                request.Headers.Add(_correlationManager.CorrelationHeader.Key,
                    _correlationManager.CorrelationHeader.Value.ToString());
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}