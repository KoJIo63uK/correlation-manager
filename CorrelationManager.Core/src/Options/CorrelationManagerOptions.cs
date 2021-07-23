using CorrelationManager.Core.Constants;

namespace CorrelationManager.Core.Options
{
    public class CorrelationManagerOptions
    {
        /// <summary>
        /// Configuration section name for correlation manager
        /// </summary>
        public const string CORRELATION_MANAGER_SECTION = "CorrelationManager.Core";
        /// <summary>
        /// Can
        /// </summary>
        public string CorrelationHeaderName { get; set; } = Headers.CORRELATION_ID;
    }
}