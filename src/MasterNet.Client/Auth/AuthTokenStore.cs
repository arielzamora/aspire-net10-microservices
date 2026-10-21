using System.Text.Json;
using Microsoft.JSInterop;

namespace MasterNet.Client.Auth;

// Singleton: el handler de HttpClientFactory vive en otro scope que los componentes.
public sealed class AuthTokenStore(IJSRuntime js)
{
    private const string StorageKey = "masternet.auth";

    private bool _loaded;

    public string? Token { get; private set; }
    public string? Email { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public event Action? Changed;

    public async Task EnsureLoadedAsync()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;

        try
        {
            var json = await js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
            var stored = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<StoredSession>(json);
            Token = stored?.Token;
            Email = stored?.Email;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }

        Changed?.Invoke();
    }

    public async Task SignInAsync(string token, string? email)
    {
        _loaded = true;
        Token = token;
        Email = email;
        await js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, JsonSerializer.Serialize(new StoredSession(token, email)));
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        Token = null;
        Email = null;
        await js.InvokeVoidAsync("sessionStorage.removeItem", StorageKey);
        Changed?.Invoke();
    }

    private sealed record StoredSession(string? Token, string? Email);
}
