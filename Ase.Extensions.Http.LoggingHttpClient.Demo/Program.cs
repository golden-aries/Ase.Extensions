using Ase.Extensions.Http.LoggingHttpClient.Demo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

var host = new HostBuilder()
    .ConfigureBuilder()
    .Build();

using (var serviceScope = host.Services.CreateScope())
{
    var sp = serviceScope.ServiceProvider;
    try
    {
        var myService = sp.GetRequiredService<DemoApp>();
        var result = await myService.RunAsync();
        var arr = JsonSerializer.Deserialize<IEnumerable<Zen>>(
            result,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        var zen = arr!.First();
        Console.WriteLine(zen.Q);
        Console.WriteLine(zen.A);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error Occured: " + ex.Message);
    }
}