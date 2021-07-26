using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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

        public CorrelationManagerFactory(CorrelationManagerOptions options, IHttpContextAccessor accessor)
        {
            _options = options;
            _accessor = accessor;
        }

        public ICorrelationManager CreateCorrelationManager() =>
            _accessor.HttpContext != null
                ? _accessor.HttpContext.RequestServices.GetRequiredService<ICorrelationManager>()
                : new CorrelationManager(_options);
    }
}