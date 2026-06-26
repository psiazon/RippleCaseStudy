using System.Net;

namespace Ripple.EventManagement.SpecFlowTests.Support;

public sealed class SuccessfulTicketInventoryHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("{}")
        };

        return Task.FromResult(response);
    }
}
