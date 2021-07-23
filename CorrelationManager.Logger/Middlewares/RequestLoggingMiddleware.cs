using System;
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
                logger.LogInformation("{@RequestInformation}", new RequestInformation(context));
                return Task.CompletedTask;
            }, httpContext);

            await _next(httpContext);
        }
    }

    internal class RequestInformation
    {
        public string Path { get; set; }
        public string Referer { get; set; }
        public string XForwardedFor { get; set; }
        public string Protocol { get; set; }
        public string Method { get; set; }
        public string RemoteIp { get; set; }
        public string RemoteHost { get; set; }
        public int RemotePort { get; set; }
        public DateTime RequestReceivedAt { get; set; }
        public double ResponseTimeMs { get; set; }
        public int ResponseStatus { get; set; }
        public string ResponseContentType { get; set; }
        public DateTime ResponseSentAt { get; set; }

        public const string REQUEST_AT = "RequestAt";

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

            Path = context.Request.Path;
            Referer = context.Request.Headers[HeaderNames.Referer];
            XForwardedFor = context.Request.Headers[HeaderNames.Referer];
            Protocol = context.Request.Protocol;
            Method = context.Request.Method;
            RemoteIp = context.Connection.RemoteIpAddress?.ToString();
            RemoteHost = remoteHost;
            RemotePort = context.Connection.RemotePort;
            RequestReceivedAt = requestReceivedAt;
            ResponseContentType = context.Response.ContentType;
            ResponseSentAt = responseSentAt;
            ResponseTimeMs = (responseSentAt - requestReceivedAt).TotalMilliseconds;
            ResponseStatus = context.Response.StatusCode;
        }
    }
}