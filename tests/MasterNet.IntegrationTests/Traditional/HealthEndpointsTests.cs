using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MasterNet.IntegrationTests.Traditional
{
    // ADR 0003: fuera de Development los endpoints de salud no se exponen salvo que se habilite HealthChecks:Expose.
    // La factory usa el entorno "Testing", es decir, un entorno que no es Development.
    public class HealthEndpointsTests : IClassFixture<CustomWebApplicationFactroy>
    {
        private readonly CustomWebApplicationFactroy _factory;

        public HealthEndpointsTests(CustomWebApplicationFactroy factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/alive")]
        [InlineData("/health")]
        public async Task HealthEndpoints_AreNotExposed_ByDefault(string path)
        {
            var response = await _factory.CreateClient().GetAsync(path);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Alive_WhenExposed_ReturnsOnlyTheStatus()
        {
            var client = _factory
                .WithWebHostBuilder(builder => builder.UseSetting("HealthChecks:Expose", "true"))
                .CreateClient();

            var response = await client.GetAsync("/alive");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("/make-healthy")]
        [InlineData("/make-unhealthy")]
        public async Task HealthToggleDemo_IsOnlyAvailableInDevelopment(string path)
        {
            var response = await _factory.CreateClient().GetAsync(path);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
