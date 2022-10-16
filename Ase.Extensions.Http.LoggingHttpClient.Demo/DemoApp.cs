using Microsoft.Extensions.Logging;

namespace Ase.Extensions.Http.LoggingHttpClient.Demo;

public class DemoApp
{
    private readonly ILogger<DemoApp> logger;
    private readonly IHttpClientFactory httpClientFactory;
    internal static string HttpClientName = "zenquotes";
    public DemoApp(ILogger<DemoApp> logger, IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<string> RunAsync()
    {
        logger.LogInformation("Application {applicationEvent} at {dateTime}", "Started", DateTime.Now);
        var request = new HttpRequestMessage(HttpMethod.Get, "api/random");
        request.Headers.Add("logTest", "logTest");
        var client = httpClientFactory.CreateClient(HttpClientName);
        var response = await client.SendAsync(request);
        logger.LogInformation("Application {applicationEvent} at {dateTime} ", "Ended", DateTime.Now);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            return $"StatusCode: {response.StatusCode}";
        }
    }
}
