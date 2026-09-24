using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

using PasswordVault.Common.Interfaces;

namespace PasswordVault.Common.Middleware;

public class SecurityMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICrypto _cryptoService;

    public SecurityMiddleware(
        RequestDelegate next,
        [FromKeyedServices("middleware")] ICrypto cryptoService)
    {
        _next = next;
        _cryptoService = cryptoService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await DecryptRequestAsync(context);

        var originalResponseBody = context.Response.Body;

        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        context.Response.Body = originalResponseBody;

        await EncryptResponseAsync(
            context,
            responseBody);
    }

    private async Task DecryptRequestAsync(
        HttpContext context)
    {
        if (!ShouldProcessRequest(context))
        {
            return;
        }

        context.Request.EnableBuffering();

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        using var document = JsonDocument.Parse(body);

        var decryptedJson = TransformJson(
            document.RootElement,
            _cryptoService.GetDecryptedText);

        var bytes = Encoding.UTF8.GetBytes(decryptedJson);

        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentLength = bytes.Length;
    }

    private async Task EncryptResponseAsync(
        HttpContext context,
        MemoryStream responseBody)
    {
        responseBody.Position = 0;

        using var reader = new StreamReader(responseBody);

        var body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        if (!IsJsonResponse(context))
        {
            await context.Response.Body.WriteAsync(
                Encoding.UTF8.GetBytes(body));

            return;
        }

        using var document = JsonDocument.Parse(body);

        var encryptedJson = TransformJson(
            document.RootElement,
            _cryptoService.GetEncryptedText);

        var bytes = Encoding.UTF8.GetBytes(encryptedJson);

        context.Response.ContentLength = bytes.Length;

        await context.Response.Body.WriteAsync(bytes);
    }

    private static string TransformJson(
        JsonElement element,
        Func<string, string> transform)
    {
        using var stream = new MemoryStream();

        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteJson(
                writer,
                element,
                transform);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteJson(
        Utf8JsonWriter writer,
        JsonElement element,
        Func<string, string> transform)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:

                writer.WriteStartObject();

                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);

                    if (IsCredentialProperty(property.Name) &&
                        property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value =
                            property.Value.GetString() ?? string.Empty;

                        writer.WriteStringValue(
                            transform(value));

                        continue;
                    }

                    WriteJson(
                        writer,
                        property.Value,
                        transform);
                }

                writer.WriteEndObject();

                break;

            case JsonValueKind.Array:

                writer.WriteStartArray();

                foreach (var item in element.EnumerateArray())
                {
                    WriteJson(
                        writer,
                        item,
                        transform);
                }

                writer.WriteEndArray();

                break;

            default:

                element.WriteTo(writer);

                break;
        }
    }

    private static bool IsCredentialProperty(
        string propertyName)
    {
        return IsCredentialField(
                   propertyName,
                   "Username")
               || IsCredentialField(
                   propertyName,
                   "Password");
    }

    private static bool IsCredentialField(
        string propertyName,
        string credentialName)
    {
        return propertyName.Equals(
                   credentialName,
                   StringComparison.OrdinalIgnoreCase)
               || propertyName.StartsWith(
                   credentialName,
                   StringComparison.OrdinalIgnoreCase)
               || propertyName.EndsWith(
                   credentialName,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldProcessRequest(
        HttpContext context)
    {
        var method = context.Request.Method;

        var isWriteRequest =
            HttpMethods.IsPost(method) ||
            HttpMethods.IsPut(method) ||
            HttpMethods.IsPatch(method);

        if (!isWriteRequest)
        {
            return false;
        }

        return context.Request.ContentType?
                   .Contains(
                       "application/json",
                       StringComparison.OrdinalIgnoreCase)
               == true;
    }

    private static bool IsJsonResponse(
        HttpContext context)
    {
        return context.Response.ContentType?
                   .Contains(
                       "application/json",
                       StringComparison.OrdinalIgnoreCase)
               == true;
    }
}
