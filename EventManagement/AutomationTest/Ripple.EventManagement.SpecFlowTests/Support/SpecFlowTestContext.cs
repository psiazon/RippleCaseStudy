using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TechTalk.SpecFlow;

namespace Ripple.EventManagement.SpecFlowTests.Support;

[Binding]
public sealed class SpecFlowTestContext : IDisposable
{
    public TestWebApplicationFactory Factory { get; } = new();
    public HttpClient Client { get; }
    public HttpResponseMessage? LastResponse { get; set; }
    public Guid LastEventId { get; set; }
    public JsonElement LastJson { get; set; }

    public SpecFlowTestContext()
    {
        Client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public void AuthenticateAs(string role)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestJwtTokenFactory.CreateToken(role));
    }

    public void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    public async Task CaptureJsonAsync()
    {
        if (LastResponse is null || LastResponse.Content.Headers.ContentLength == 0)
        {
            LastJson = default;
            return;
        }

        var text = await LastResponse.Content.ReadAsStringAsync();
        LastJson = string.IsNullOrWhiteSpace(text) ? default : JsonDocument.Parse(text).RootElement.Clone();
    }

    public void Dispose()
    {
        LastResponse?.Dispose();
        Client.Dispose();
        Factory.Dispose();
    }
}
