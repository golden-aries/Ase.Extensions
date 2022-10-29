namespace Ase.Extensions.Http.Utils
{
    public static class RequestUtils
    {
        public static HashSet<string> JsonContentTypes { get; set; } = new(2, StringComparer.OrdinalIgnoreCase)
        {
            System.Net.Mime.MediaTypeNames.Application.Json,
            MediaTypeNamesEx.Applicaion.JoseJson
        };
    }
}
