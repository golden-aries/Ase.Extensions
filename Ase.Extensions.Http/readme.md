# Ase.Extensions.Http
## Purpose
This package contains a LoggingHttpMessageHandler and HttpMessageHandlerBuilderFilter 
that can be used to :

- Replace handler [Logging Handler aka Client Handler](https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Http/src/Logging/LoggingHttpMessageHandler.cs)

- Log HttpRequestMessage.Content if log level is Trace and
`HttpRequestContent is  System.Net.Http.ByteArrayContent` (e.g. StringContent)
and `request.Content?.Headers.ContentType?.MediaType == MediaTypeNames.Application.Json`

Logs HttpResponseMessage.Body if log level is Trace and 
`response.Content.Headers.ContentType?.MediaType == MediaTypeNames.Application.Json`

if IJsonFormatter is provided in DI container it is used to format json content

The usage example is in:
Ase.Extensions.Http.LoggingHttpClient.Demo