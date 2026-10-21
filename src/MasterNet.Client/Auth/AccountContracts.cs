namespace MasterNet.Client.Auth;

public sealed record class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public sealed record class ProfileResponse
{
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public string? Token { get; init; }
    public string? Username { get; init; }
}
