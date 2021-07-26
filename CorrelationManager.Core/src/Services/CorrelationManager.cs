using System;
using System.Collections.Generic;
using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Options;
using Microsoft.Extensions.Logging;

namespace CorrelationManager.Core.Services
{
    internal class CorrelationManager: ICorrelationManager
    {
        private readonly string _correlationHeaderName;
        private readonly ILogger<CorrelationManager> _logger;

        public CorrelationManager(CorrelationManagerOptions correlationHeaderName, ILogger<CorrelationManager> logger)
        {
            _logger = logger;
            _correlationHeaderName = correlationHeaderName.CorrelationHeaderName;
        }
        /// <inheritdoc cref="ICorrelationManager"/>
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        /// <inheritdoc cref="ICorrelationManager"/>
        public KeyValuePair<string, object> CorrelationHeader => new(_correlationHeaderName, CorrelationId);
        /// <inheritdoc cref="ICorrelationManager"/>
        public IDisposable CreateScope()
        {
            return _logger.BeginScope(CorrelationHeader);
        }
    }
}