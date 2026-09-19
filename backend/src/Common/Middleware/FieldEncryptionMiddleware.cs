using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PasswordVault.Common.Interfaces;

namespace PasswordVault.Common.Middleware;

public sealed class FieldEncryptionMiddleware
{
    private const string EncryptionHeader = "X-Encrypted-Payload";
    private readonly RequestDelegate _next;

    public FieldEncryptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        [FromKeyedServices("middleware")] ICrypto crypto)
    {
        if (!IsEncryptedPayload(context.Request))
        {
            await _next(context);
            return;
        }

        await DecryptRequestBodyAsync(context, crypto);

        var originalResponseBody = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            await EncryptResponseBodyAsync(context, crypto, originalResponseBody, responseBody);
        }
        finally
        {
            context.Response.Body = originalResponseBody;
        }
    }

    private static bool IsEncryptedPayload(HttpRequest request) =>
        request.Headers.TryGetValue(EncryptionHeader, out var value) &&
        bool.TryParse(value.FirstOrDefault(), out var enabled) && enabled;

    private static async Task DecryptRequestBodyAsync(HttpContext context, ICrypto crypto)
    {
        if (!context.Request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ?? true)
        {
            return;
        }

        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        var json = JsonNode.Parse(body);
        if (json is null)
        {
            return;
        }

        TransformStringValues(json, crypto.GetDecryptedText);
        var decryptedBody = Encoding.UTF8.GetBytes(json.ToJsonString());
        context.Request.Body = new MemoryStream(decryptedBody);
        context.Request.ContentLength = decryptedBody.Length;
    }

    private static async Task EncryptResponseBodyAsync(
        HttpContext context,
        ICrypto crypto,
        Stream originalResponseBody,
        MemoryStream responseBody)
    {
        if (!context.Response.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) ?? true)
        {
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalResponseBody);
            return;
        }

        responseBody.Position = 0;
        using var reader = new StreamReader(responseBody, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(body))
        {
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalResponseBody);
            return;
        }

        var json = JsonNode.Parse(body);
        if (json is null)
        {
            responseBody.Position = 0;
            await responseBody.CopyToAsync(originalResponseBody);
            return;
        }

        TransformStringValues(json, crypto.GetEncryptedText);
        var encryptedBody = Encoding.UTF8.GetBytes(json.ToJsonString());
        context.Response.ContentLength = encryptedBody.Length;
        await originalResponseBody.WriteAsync(encryptedBody);
    }

    private static void TransformStringValues(JsonNode node, Func<string, string> transform)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (property.Value is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var stringValue))
                {
                    jsonObject[property.Key] = TransformSafely(stringValue, transform);
                }
                else if (property.Value is not null)
                {
                    TransformStringValues(property.Value, transform);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    TransformStringValues(item, transform);
                }
            }
        }
    }

    private static string TransformSafely(string value, Func<string, string> transform)
    {
        try
        {
            return transform(value);
        }
        catch (Exception)
        {
            return value;
        }
    }
}
