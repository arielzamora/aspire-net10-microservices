using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MasterNet.IntegrationTests.AspireTesting
{
    // Enfoque Aspire.Hosting.Testing: toda la topología real (SQL, Redis, servicios), solo por HTTP.
    public class IntegrationAspireTests : IClassFixture<AppHostFixture>
    {
        private readonly AppHostFixture _fixture;

        public IntegrationAspireTests(AppHostFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetCoursesShouldReturnOk()
        {
            var response = await _fixture.ApiClient.GetAsync("/api/courses");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ListCreateEditAndViewCourse()
        {
            var api = _fixture.ApiClient;

            // listar
            var before = await GetTotalCountAsync(api);

            // iniciar sesión como el admin sembrado
            var login = await api.PostAsJsonAsync("/api/account/login", new
            {
                email = "azamora@gmail.com",
                password = _fixture.SeedAdminPassword
            });
            login.EnsureSuccessStatusCode();
            var token = (await ReadJsonAsync(login)).GetProperty("token").GetString();
            api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // crear
            var create = await api.PostAsync("/api/courses", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["title"] = "Curso de punta a punta",
                ["description"] = "Creado por el test del AppHost"
            }));
            create.EnsureSuccessStatusCode();
            var id = await create.Content.ReadFromJsonAsync<Guid>();

            // editar
            var update = await api.PutAsJsonAsync($"/api/courses/{id}", new
            {
                title = "Curso de punta a punta (editado)",
                description = "Editado por el test del AppHost"
            });
            update.EnsureSuccessStatusCode();

            // ver
            var view = await api.GetAsync($"/api/courses/{id}");
            Assert.Equal(HttpStatusCode.OK, view.StatusCode);
            Assert.Equal("Curso de punta a punta (editado)", (await ReadJsonAsync(view)).GetProperty("title").GetString());
            Assert.Equal(before + 1, await GetTotalCountAsync(api));
        }

        private static async Task<int> GetTotalCountAsync(HttpClient api)
        {
            var response = await api.GetAsync("/api/courses?pageSize=5");
            response.EnsureSuccessStatusCode();
            return (await ReadJsonAsync(response)).GetProperty("totalCount").GetInt32();
        }

        private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response)
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.Clone();
        }
    }
}
