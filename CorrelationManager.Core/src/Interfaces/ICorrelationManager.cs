using System;
using System.Collections.Generic;

namespace CorrelationManager.Core.Interfaces
{
    public interface ICorrelationManager
    {
        /// <summary>
        /// Correlation id
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// Header for correlation
        /// </summary>
        public KeyValuePair<string, object> CorrelationHeader { get; }
        /// <summary>
        /// Create logging scope
        /// </summary>
        /// <returns></returns>
        IDisposable CreateScope();
    }
}