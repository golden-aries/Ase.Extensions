using Ase.Extensions.Http;
using Ase.Extensions.Http.LoggingHttpClient.Demo;
using Ase.Extensions.Http.StJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class HostBuilderEx
    {
        internal static IHostBuilder ConfigureBuilder(this IHostBuilder builder)
        {
            builder
                .ConfigureAppConfiguration(cd =>
                {
                    cd.AddJsonFile("appsettings.json");
                })
                .ConfigureLogging((hostBuilderContext, loggingBuiilder) =>
                {
                    loggingBuiilder
                    .AddConfiguration(hostBuilderContext.Configuration.GetSection("Logging"));
                    loggingBuiilder.AddConsole();//.SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient(DemoApp.HttpClientName, c =>
                    {
                        c.BaseAddress = new Uri("https://zenquotes.io/");
                        //c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                        c.DefaultRequestHeaders.Add("X-API-KEY", "Zen");
                        c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36 Edg/105.0.1343.53");

                    });
                    services.AddOptions<HttpClientFactoryOptions>();
                    services.AddSingleton<IJsonFormatter, JsonFormatter>();

                    // this replaces Microsoft provided IHttpMessageHandlerBuilderFilter implementaion
                    services.Replace(
                        ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HttpMessageHandlerBuilderFilter>());
                    services.AddTransient<DemoApp>();
                }).UseConsoleLifetime();
            return builder;
        }
    }
}
