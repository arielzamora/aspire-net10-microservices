using MasterNet.Client.Auth;
using Refit;

namespace MasterNet.Client.Refit;

public interface IAccountApi
{
    [Post("/api/account/login")]
    Task<IApiResponse<ProfileResponse>> LoginAsync([Body] LoginRequest request, CancellationToken cancellationToken = default);
}
