using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace GatewayAPI.Helpers;

[ExcludeFromCodeCoverage]
public static class ProxyHelper
{
    public static async Task<IActionResult> ProxyResponse(HttpResponseMessage response)
    {
        if (response.Content == null)
            return new StatusCodeResult((int)response.StatusCode);

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
        var contentLength = response.Content.Headers.ContentLength;

        if (contentLength == 0)
            return new StatusCodeResult((int)response.StatusCode);

        if (contentType.Contains("application/json"))
        {
            var body = await response.Content.ReadFromJsonAsync<object>();
            return new ObjectResult(body) { StatusCode = (int)response.StatusCode };
        }

        var text = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(text))
            return new StatusCodeResult((int)response.StatusCode);

        return new ObjectResult(text) { StatusCode = (int)response.StatusCode };
    }
}