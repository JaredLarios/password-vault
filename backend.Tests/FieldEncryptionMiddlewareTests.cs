using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PasswordVault.Common.Middleware;
using PasswordVault.Common.Repositories;

namespace PasswordVaultAPI.Tests;

public class FieldEncryptionMiddlewareTests
{
    private const string MiddlewareKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    [Fact]
    public async Task EncryptedPayload_DecryptsRequestAndEncryptsResponse()
    {
        var crypto = new FernetRepository(MiddlewareKey);
        var encryptedRequest = JsonSerializer.Serialize(new
        {
            username = crypto.GetEncryptedText("fabian@example.com"),
            name = crypto.GetEncryptedText("Fabian")
        });
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Headers["X-Encrypted-Payload"] = "true";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(encryptedRequest));
        context.Response.Body = new MemoryStream();

        var middleware = new FieldEncryptionMiddleware(async httpContext =>
        {
            using var reader = new StreamReader(httpContext.Request.Body);
            var requestBody = await reader.ReadToEndAsync();
            var requestJson = JsonDocument.Parse(requestBody);

            Assert.Equal("fabian@example.com", requestJson.RootElement.GetProperty("username").GetString());
            Assert.Equal("Fabian", requestJson.RootElement.GetProperty("name").GetString());

            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync("{\"message\":\"accepted\"}");
        });

        await middleware.InvokeAsync(context, crypto);

        context.Response.Body.Position = 0;
        using var responseReader = new StreamReader(context.Response.Body);
        var responseBody = await responseReader.ReadToEndAsync();
        var responseJson = JsonDocument.Parse(responseBody);
        var encryptedMessage = responseJson.RootElement.GetProperty("message").GetString();

        Assert.NotEqual("accepted", encryptedMessage);
        Assert.Equal("accepted", crypto.GetDecryptedText(encryptedMessage!));
    }

    [Fact]
    public async Task PlainPayload_BypassesEncryption()
    {
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"name\":\"Fabian\"}"));
        context.Response.Body = new MemoryStream();

        var middleware = new FieldEncryptionMiddleware(async httpContext =>
        {
            using var reader = new StreamReader(httpContext.Request.Body);
            Assert.Contains("Fabian", await reader.ReadToEndAsync());
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync("{\"message\":\"accepted\"}");
        });

        await middleware.InvokeAsync(context, new FernetRepository(MiddlewareKey));

        context.Response.Body.Position = 0;
        using var responseReader = new StreamReader(context.Response.Body);
        Assert.Equal("{\"message\":\"accepted\"}", await responseReader.ReadToEndAsync());
    }
}
