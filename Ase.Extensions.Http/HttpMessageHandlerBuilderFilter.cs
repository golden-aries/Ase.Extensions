using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ase.Extensions.Http;

public class HttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptionsMonitor<HttpClientFactoryOptions>? options;
    private readonly IJsonFormatter? jsonFormatter;

    public HttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory) 
        : this(loggerFactory, null, null)
    {
       
    }

    public HttpMessageHandlerBuilderFilter(
       ILoggerFactory loggerFactory,
       IJsonFormatter jsonFormatter):this(loggerFactory, null, jsonFormatter)
    {
    }

    public HttpMessageHandlerBuilderFilter(
       ILoggerFactory loggerFactory,
       IOptionsMonitor<HttpClientFactoryOptions> options) : this(loggerFactory, options, null)
    {
    }

    public HttpMessageHandlerBuilderFilter(
        ILoggerFactory loggerFactory,
        IOptionsMonitor<HttpClientFactoryOptions>? options,
        IJsonFormatter? jsonFormatter)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        this.options = options;
        this.jsonFormatter = jsonFormatter;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        if (next == null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        return (builder) =>
        {
            // Run other configuration first, we want to decorate.
            next(builder);

            string loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";

            ILogger logger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.{typeof(LoggingHttpMessageHandler).FullName}");
            builder.AdditionalHandlers.Add(new LoggingHttpMessageHandler(logger, options?.Get(builder.Name), jsonFormatter));
        };
    }
}
