using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace CorrelationManager.Logger.Middlewares
{
    /// <summary>
    /// Log inner request information
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, ILogger<RequestLoggingMiddleware> logger)
        {
            httpContext.Items.Add(RequestInformation.REQUEST_AT, DateTime.Now);

            httpContext.Response.OnStarting(state =>
            {
                var context = state as HttpContext;
                var requestInformation = new RequestInformation(context);

                logger.Log(logLevel: LogLevel.Information,
                    eventId: new EventId(),
                    state: requestInformation.State,
                    exception: null,
                    formatter: requestInformation.Callback);
                
                return Task.CompletedTask;
            }, httpContext);

            await _next(httpContext);
        }
    }

    internal class RequestInformation
    {
        public const string REQUEST_AT = "RequestAt";
        
        private readonly string _host;
        private readonly string _scheme;
        private readonly string _pathBase;
        private readonly string _path;
        private readonly string _query;
        private readonly string _requestContentType;
        private readonly long? _requestContentLength;
        private readonly string _fullPath;
        private readonly string _referer;
        private readonly string _xForwardedFor;
        private readonly string _protocol;
        private readonly string _method;
        private readonly string _remoteIp;
        private readonly string _remoteHost;
        private readonly int _remotePort;
        private readonly DateTime _requestReceivedAt;
        private readonly double _responseTimeMs;
        private readonly int _responseStatus;
        private readonly string _responseContentType;
        private readonly DateTime _responseSentAt;

        public RequestInformation(HttpContext context)
        {
            string remoteHost = null;
            if (context.Connection.RemoteIpAddress != null)
            {
                try
                {
                    remoteHost = Dns.GetHostEntry(context.Connection.RemoteIpAddress).HostName;
                }
                catch
                {
                    remoteHost = context.Connection.RemoteIpAddress.ToString();
                }
            }

            var requestReceivedAt = context.Items[REQUEST_AT] as DateTime? ?? default;
            var responseSentAt = DateTime.Now;

            _fullPath = context.Request.Path;
            _referer = context.Request.Headers[HeaderNames.Referer];
            _xForwardedFor = context.Request.Headers[HeaderNames.Referer];
            _protocol = context.Request.Protocol;
            _method = context.Request.Method;
            _remoteIp = context.Connection.RemoteIpAddress?.ToString();
            _remoteHost = remoteHost;
            _remotePort = context.Connection.RemotePort;
            _requestReceivedAt = requestReceivedAt;
            _responseContentType = context.Response.ContentType;
            _responseSentAt = responseSentAt;
            _responseTimeMs = (responseSentAt - requestReceivedAt).TotalMilliseconds;
            _responseStatus = context.Response.StatusCode;

            _host = context.Request.Host.Value;
            _scheme = context.Request.Scheme;
            _pathBase = context.Request.PathBase;
            _query = context.Request.QueryString.Value;
            _requestContentType = context.Request.ContentType;
            _requestContentLength = context.Request.ContentLength;
            _path = context.Request.Path;
            
            _fullPath = string.Format(CultureInfo.InvariantCulture, 
                "{0}://{1}{2}{3}{4}",
                _scheme,
                _host,
                _pathBase,
                _path,
                _query);
        }
        
        public IReadOnlyList<KeyValuePair<string, object>> State => new[]
        {
            new KeyValuePair<string, object>(nameof(_fullPath), _fullPath),
            new KeyValuePair<string, object>(nameof(_referer), _referer),
            new KeyValuePair<string, object>(nameof(_xForwardedFor), _xForwardedFor),
            new KeyValuePair<string, object>(nameof(_protocol), _method),
            new KeyValuePair<string, object>(nameof(_remoteIp), _remoteIp),
            new KeyValuePair<string, object>(nameof(_remoteHost), _remoteHost),
            new KeyValuePair<string, object>(nameof(_remotePort), _remotePort),
            new KeyValuePair<string, object>(nameof(_requestReceivedAt), _requestReceivedAt),
            new KeyValuePair<string, object>(nameof(_responseTimeMs), _responseTimeMs),
            new KeyValuePair<string, object>(nameof(_responseStatus), _responseStatus),
            new KeyValuePair<string, object>(nameof(_responseContentType), _responseContentType),
            new KeyValuePair<string, object>(nameof(_responseSentAt), _responseSentAt)
        };

        public Func<IReadOnlyList<KeyValuePair<string, object>>, Exception, string> Callback =>
            (_, _) => ToString();

        public override string ToString()
        {
            return string.Format(
                    CultureInfo.InvariantCulture,
                    "Request {0} {1} {2}://{3}{4}{5}{6} {7} {8} {9}",
                    _protocol,
                    _method,
                    _scheme,
                    _host,
                    _pathBase,
                    _path,
                    _query,
                    _requestContentType,
                    _requestContentLength,
                    _responseStatus);
        }
    }
}