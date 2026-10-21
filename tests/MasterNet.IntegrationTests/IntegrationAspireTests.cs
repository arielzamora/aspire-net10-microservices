using Aspire.Hosting.Testing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MasterNet.IntegrationTests
{
    public class IntegrationAspireTests
    {
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(180);

        [Fact]
        public async Task GetCoursesShouldReturnOk()
        {
            // El parámetro secreto no tiene valor por defecto (en local lo pide el dashboard): el test aporta uno propio.
            var seedAdminPassword = $"Aa1{Guid.NewGuid():N}";
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MasterNet_AppHost>(
                [$"Parameters:seed-admin-password={seedAdminPassword}"]);
            await using var app = await appHost.BuildAsync().WaitAsync(_defaultTimeout);
            await app.StartAsync().WaitAsync(_defaultTimeout);

            await app.ResourceNotifications.WaitForResourceHealthyAsync("api").WaitAsync(_defaultTimeout);

            var httpClient = app.CreateHttpClient("api");
            var response = await httpClient.GetAsync("/api/courses");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        }
    }
}
