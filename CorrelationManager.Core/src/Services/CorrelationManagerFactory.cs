using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CorrelationManager.Core.Services
{
    public interface ICorrelationManagerFactory
    {
        ICorrelationManager CreateCorrelationManager();
    }

    public class CorrelationManagerFactory : ICorrelationManagerFactory
    {
        private readonly CorrelationManagerOptions _options;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILoggerFactory _loggerFactory;

        public CorrelationManagerFactory(CorrelationManagerOptions options, IHttpContextAccessor accessor, ILoggerFactory loggerFactory)
        {
            _options = options;
            _accessor = accessor;
            _loggerFactory = loggerFactory;
        }

        public ICorrelationManager CreateCorrelationManager() =>
            _accessor.HttpContext != null
                ? _accessor.HttpContext.RequestServices.GetRequiredService<ICorrelationManager>()
                : new CorrelationManager(_options, _loggerFactory.CreateLogger<CorrelationManager>());
    }
}