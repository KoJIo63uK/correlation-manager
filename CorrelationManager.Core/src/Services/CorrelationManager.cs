using System;
using System.Collections.Generic;
using CorrelationManager.Core.Interfaces;
using CorrelationManager.Core.Options;

namespace CorrelationManager.Core.Services
{
    internal class CorrelationManager: ICorrelationManager
    {
        private readonly string _correlationHeaderName;

        public CorrelationManager(CorrelationManagerOptions correlationHeaderName)
        {
            _correlationHeaderName = correlationHeaderName.CorrelationHeaderName;
        }
        /// <inheritdoc cref="ICorrelationManager"/>
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
        /// <inheritdoc cref="ICorrelationManager"/>
        public KeyValuePair<string, object> CorrelationHeader => new(_correlationHeaderName, CorrelationId);
    }
}