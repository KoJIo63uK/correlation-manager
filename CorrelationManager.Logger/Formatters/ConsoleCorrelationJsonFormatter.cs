using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using CorrelationManager.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace CorrelationManager.Logger.Formatters
{
    internal sealed class ConsoleCorrelationJsonFormatter : ConsoleFormatter, IDisposable
    {
        private const string FORMATTER_NAME = "correlationJson";
        
        private readonly IDisposable _optionsReloadToken;
        private readonly CorrelationManagerOptions _correlationManagerOptions;

        public ConsoleCorrelationJsonFormatter(IOptionsMonitor<JsonConsoleFormatterOptions> options,
            CorrelationManagerOptions correlationManagerOptions)
            : base(FORMATTER_NAME)
        {
            ReloadLoggerOptions(options.CurrentValue);
            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            _correlationManagerOptions = correlationManagerOptions;
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider,
            TextWriter textWriter)
        {
            string message = logEntry.Formatter(logEntry.State, logEntry.Exception);
            if (logEntry.Exception == null && message == null)
            {
                return;
            }

            LogLevel logLevel = logEntry.LogLevel;
            string category = logEntry.Category;
            Exception exception = logEntry.Exception;

            using (var output = new MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(output, FormatterOptions.JsonWriterOptions))
                {
                    writer.WriteStartObject();

                    var timestampFormat = FormatterOptions.TimestampFormat;
                    {
                        DateTimeOffset dateTimeOffset = FormatterOptions.UseUtcTimestamp
                            ? DateTimeOffset.UtcNow
                            : DateTimeOffset.Now;
                        writer.WriteString("Timestamp", dateTimeOffset.ToString(timestampFormat));
                    }

                    writer.WriteString(nameof(logEntry.LogLevel), GetLogLevelString(logLevel));
                    writer.WriteString(nameof(logEntry.Category), category);

                    WriteCorrelationId(writer, scopeProvider);

                    writer.WriteString("Message", message);

                    if (exception != null)
                    {
                        string exceptionMessage = exception.ToString();
                        if (!FormatterOptions.JsonWriterOptions.Indented)
                        {
                            exceptionMessage = exceptionMessage.Replace(Environment.NewLine, " ");
                        }

                        writer.WriteString(nameof(Exception), exceptionMessage);
                    }

                    if (logEntry.State != null)
                    {
                        writer.WriteStartObject(nameof(logEntry.State));
                        writer.WriteString("Message", logEntry.State.ToString());
                        if (logEntry.State is IReadOnlyCollection<KeyValuePair<string, object>> stateProperties)
                        {
                            foreach (KeyValuePair<string, object> item in stateProperties)
                            {
                                var properties = item.Value.GetType().GetProperties();
                                if (item.Key != "{OriginalFormat}" && properties.Any())
                                {
                                    writer.WriteStartObject(item.Key);
                                    foreach (var o in properties)
                                    {
                                        WriteItem(writer,
                                            new KeyValuePair<string, object>(o.Name, o.GetValue(item.Value)));
                                    }

                                    writer.WriteEndObject();
                                }
                                else
                                {
                                    WriteItem(writer, item);
                                }
                            }
                        }

                        writer.WriteEndObject();
                    }

                    WriteScopeInformation(writer, scopeProvider);

                    writer.WriteEndObject();
                    writer.Flush();
                }

                textWriter.Write(Encoding.UTF8.GetString(output.ToArray()));
            }

            textWriter.Write(Environment.NewLine);
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "Trace",
                LogLevel.Debug => "Debug",
                LogLevel.Information => "Information",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
            };
        }

        private void WriteCorrelationId(Utf8JsonWriter writer, IExternalScopeProvider scopeProvider)
        {
            scopeProvider?.ForEachScope((scope, state) =>
            {
                if (scope is KeyValuePair<string, object> item &&
                    item.Key == _correlationManagerOptions.CorrelationHeaderName)
                {
                    WriteItem(state, item);
                }
            }, writer);
        }

        private void WriteScopeInformation(Utf8JsonWriter writer, IExternalScopeProvider scopeProvider)
        {
            if (FormatterOptions.IncludeScopes && scopeProvider != null)
            {
                writer.WriteStartArray("Scopes");
                scopeProvider.ForEachScope((scope, state) =>
                {
                    if (scope is IEnumerable<KeyValuePair<string, object>> scopeItems)
                    {
                        state.WriteStartObject();
                        state.WriteString("Message", scope.ToString());
                        foreach (KeyValuePair<string, object> item in scopeItems)
                        {
                            WriteItem(state, item);
                        }

                        state.WriteEndObject();
                    }
                    else
                    {
                        state.WriteStringValue(ToInvariantString(scope));
                    }
                }, writer);
                writer.WriteEndArray();
            }
        }

        private void WriteItem(Utf8JsonWriter writer, KeyValuePair<string, object> item)
        {
            var key = item.Key;
            switch (item.Value)
            {
                case bool boolValue:
                    writer.WriteBoolean(key, boolValue);
                    break;
                case byte byteValue:
                    writer.WriteNumber(key, byteValue);
                    break;
                case sbyte sbyteValue:
                    writer.WriteNumber(key, sbyteValue);
                    break;
                case char charValue:
                    writer.WriteString(key, MemoryMarshal.CreateSpan(ref charValue, 1));
                    break;
                case decimal decimalValue:
                    writer.WriteNumber(key, decimalValue);
                    break;
                case double doubleValue:
                    writer.WriteNumber(key, doubleValue);
                    break;
                case float floatValue:
                    writer.WriteNumber(key, floatValue);
                    break;
                case int intValue:
                    writer.WriteNumber(key, intValue);
                    break;
                case uint uintValue:
                    writer.WriteNumber(key, uintValue);
                    break;
                case long longValue:
                    writer.WriteNumber(key, longValue);
                    break;
                case ulong ulongValue:
                    writer.WriteNumber(key, ulongValue);
                    break;
                case short shortValue:
                    writer.WriteNumber(key, shortValue);
                    break;
                case ushort ushortValue:
                    writer.WriteNumber(key, ushortValue);
                    break;
                case null:
                    writer.WriteNull(key);
                    break;
                default:
                    writer.WriteString(key, ToInvariantString(item.Value));
                    break;
            }
        }

        private static string ToInvariantString(object obj) => Convert.ToString(obj, CultureInfo.InvariantCulture);

        internal JsonConsoleFormatterOptions FormatterOptions { get; set; }

        private void ReloadLoggerOptions(JsonConsoleFormatterOptions options)
        {
            FormatterOptions = options;
        }

        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }
    }
}