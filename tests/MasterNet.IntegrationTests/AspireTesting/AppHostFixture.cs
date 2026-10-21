using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace MasterNet.IntegrationTests.AspireTesting
{
    // Levanta la topología completa del AppHost una sola vez para todos los tests de la clase.
    public class AppHostFixture : IAsyncLifetime
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(180);

        private DistributedApplication? _app;

        // El parámetro secreto no tiene valor por defecto (en local lo pide el dashboard): el test aporta uno propio.
        public string SeedAdminPassword { get; } = $"Aa1{Guid.NewGuid():N}";

        public HttpClient ApiClient { get; private set; } = default!;

        public async Task InitializeAsync()
        {
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MasterNet_AppHost>(
                [$"Parameters:seed-admin-password={SeedAdminPassword}"]);

            _app = await appHost.BuildAsync().WaitAsync(DefaultTimeout);
            await _app.StartAsync().WaitAsync(DefaultTimeout);
            await _app.ResourceNotifications.WaitForResourceHealthyAsync("api").WaitAsync(DefaultTimeout);

            ApiClient = _app.CreateHttpClient("api");
        }

        public async Task DisposeAsync()
        {
            if (_app is not null)
            {
                await _app.DisposeAsync();
            }
        }

        // Si una llamada falla, el mensaje incluye el cuerpo de la respuesta (en Development la API devuelve el detalle
        // del error): en CI no hay otra forma de ver qué pasó dentro de la API.
        public async Task<HttpResponseMessage> EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var body = await response.Content.ReadAsStringAsync();
            throw new Xunit.Sdk.XunitException(
                $"HTTP {(int)response.StatusCode} en {response.RequestMessage?.Method} {response.RequestMessage?.RequestUri}{Environment.NewLine}" +
                $"Cuerpo: {body}");
        }
    }
}
