using System.Net.Http.Headers;

namespace MasterNet.Client.Auth;

public sealed class AuthorizationHandler(AuthTokenStore store) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await store.EnsureLoadedAsync();

        if (store.IsAuthenticated)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", store.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
