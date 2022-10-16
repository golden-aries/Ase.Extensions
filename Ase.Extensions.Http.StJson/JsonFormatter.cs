using System.Text.Json;
using System.Text.Json.Nodes;

namespace Ase.Extensions.Http.StJson;

public class JsonFormatter : IJsonFormatter
{
    public  JsonSerializerOptions Options { get; }

    public JsonFormatter()
        : this(new JsonSerializerOptions { WriteIndented = true })
    { 

    }

    public JsonFormatter(JsonSerializerOptions jsonSerializerOptions)
    {
        this.Options = jsonSerializerOptions;
    }

    public string? Format(string json)
    {
        return JsonNode.Parse(json)?.ToJsonString(Options);
    }
}
