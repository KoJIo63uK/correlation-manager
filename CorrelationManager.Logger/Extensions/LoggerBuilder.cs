using CorrelationManager.Logger.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace CorrelationManager.Logger.Extensions
{
    public static class LoggerBuilder
    {
        /// <summary>
        /// Registration ConsoleCorrelationJsonLogger
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddConsoleCorrelationJsonLogger(this ILoggingBuilder builder)
        {
            builder.AddConsoleFormatter<ConsoleCorrelationJsonFormatter, JsonConsoleFormatterOptions>();

            return builder;
        }
    }
}