// Sourced from a file licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Ase.Extensions.Http.Utils;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Mime;

namespace Ase.Extensions.Http
{
    public class LoggingHttpMessageHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly HttpClientFactoryOptions? _options;
        private readonly IJsonFormatter? jsonFormatter;
        private static readonly Func<string, bool> _shouldNotRedactHeaderValue = (header) => false;

        public LoggingHttpMessageHandler(
            ILogger logger,
            HttpClientFactoryOptions? options,
            IJsonFormatter? jsonFormatter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options;
            this.jsonFormatter = jsonFormatter;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Func<string, bool> shouldRedactHeaderValue = _options?.ShouldRedactHeaderValue ?? _shouldNotRedactHeaderValue;

            // Not using a scope here because we always expect this to be at the end of the pipeline, thus there's
            // not really anything to surround.
            Log.RequestStart(_logger, request, shouldRedactHeaderValue);

            if (ShouldLogRequestBody(request, _logger))
            {
                if (request.Content is ByteArrayContent bc)
                {
                    var jsonReq = await bc.ReadAsStringAsync(cancellationToken);
                    if (!string.IsNullOrWhiteSpace(jsonReq))
                    {
                        // log string here
                        var bodyStringFormatted = jsonFormatter?.Format(jsonReq) ?? jsonReq;
                        if (!string.IsNullOrWhiteSpace(bodyStringFormatted))
                        {
                            Log.LogRequestBody(_logger, bodyStringFormatted);
                        }
                    }
                }

            }
            var stopwatch = ValueStopwatch.StartNew();
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            Log.RequestEnd(_logger, response, stopwatch.GetElapsedTime(), shouldRedactHeaderValue);
            if (response.IsSuccessStatusCode && _logger.IsEnabled(LogLevel.Trace))
            {
                if (response.Content is not null && ShouldLogResponseBody(response, _logger))
                {
                    var jsonResp = await response.Content.ReadAsStringAsync(cancellationToken);
                    var jsonFormatted = jsonFormatter?.Format(jsonResp) ?? jsonResp;
                    Log.ResponseBody(_logger, jsonFormatted);
                }
            }
            return response;
        }

        private static bool ShouldLogRequestBody(HttpRequestMessage request, ILogger logger)
        {
            return logger.IsEnabled(LogLevel.Trace)
                &&
                request.Content?.Headers.ContentType?.MediaType is not null
                && RequestUtils.JsonContentTypes.Contains(request.Content.Headers.ContentType.MediaType);
        }

        private static bool ShouldLogResponseBody(HttpResponseMessage response, ILogger logger)
        {
            return logger.IsEnabled(LogLevel.Trace)
                &&
                response.Content?.Headers.ContentType?.MediaType is not null
                && RequestUtils.JsonContentTypes.Contains(response.Content.Headers.ContentType.MediaType);
        }

        // Used in tests.
        internal static class Log
        {
            public static class EventIds
            {
                public static readonly EventId RequestStart = new(100, "RequestStart");
                public static readonly EventId RequestEnd = new(101, "RequestEnd");

                public static readonly EventId RequestHeader = new(102, "RequestHeader");
                public static readonly EventId ResponseHeader = new(103, "ResponseHeader");
            }

            private static readonly Action<ILogger, HttpMethod, Uri?, Exception?> _requestStart = LoggerMessage.Define<HttpMethod, Uri?>(
                LogLevel.Information,
                EventIds.RequestStart,
                "Sending HTTP request {HttpMethod} {Uri}");

            private static readonly Action<ILogger, string, Exception?> _logRrequestBody =
                LoggerMessage.Define<string>(
                    LogLevel.Trace,
                    EventIds.RequestStart,
                    @"Request body:
{body}");

            private static readonly Action<ILogger, double, int, Exception?> _requestEnd = LoggerMessage.Define<double, int>(
                LogLevel.Information,
                EventIds.RequestEnd,
                "Received HTTP response headers after {ElapsedMilliseconds}ms - {StatusCode}");

            public static void RequestStart(ILogger logger, HttpRequestMessage request, Func<string, bool> shouldRedactHeaderValue)
            {
                _requestStart(logger, request.Method, request.RequestUri, null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.Log(
                        LogLevel.Trace,
                        EventIds.RequestHeader,
                        new HttpHeadersLogValue(HttpHeadersLogValue.Kind.Request, request.Headers, request.Content?.Headers, shouldRedactHeaderValue),
                        null,
                        (state, ex) => state.ToString());
                }
            }

            public static void LogRequestBody(ILogger logger, string requestBodyAsString)
            {
                _logRrequestBody(logger, requestBodyAsString, null);
            }
            public static void RequestEnd(ILogger logger, HttpResponseMessage response, TimeSpan duration, Func<string, bool> shouldRedactHeaderValue)
            {
                _requestEnd(logger, duration.TotalMilliseconds, (int)response.StatusCode, null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.Log(
                        LogLevel.Trace,
                        EventIds.ResponseHeader,
                        new HttpHeadersLogValue(HttpHeadersLogValue.Kind.Response, response.Headers, response.Content?.Headers, shouldRedactHeaderValue),
                        null,
                        (state, ex) => state.ToString());
                }
            }

            private static readonly Action<ILogger, string?, Exception?> _responseBody =
                LoggerMessage.Define<string?>(
                    LogLevel.Trace,
                    EventIds.RequestEnd,
                    @"Response body:
{body}");

            public static void ResponseBody(ILogger logger, string? responseBodyAsString)
            {
                _responseBody(logger, responseBodyAsString, null);
            }
        }
    }
}

