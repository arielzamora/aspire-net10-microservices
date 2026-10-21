using MasterNet.Domain.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MasterNet.IntegrationTests
{
    public class CoursesEndpointsTests : IClassFixture<CustomWebApplicationFactroy>
    {
        private static readonly Guid MissingId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private readonly HttpClient _client;

        public CoursesEndpointsTests(CustomWebApplicationFactroy factory)
        {
            _client = factory.CreateClient();
        }

        // ---------- POST /api/courses ----------

        [Fact]
        public async Task Create_WithValidData_Returns200AndPersistsCourse()
        {
            var response = await PostCourseAsync("Curso nuevo", "Descripcion", WriterToken());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var id = await response.Content.ReadFromJsonAsync<Guid>();

            var created = await _client.GetAsync($"/api/courses/{id}");
            Assert.Equal(HttpStatusCode.OK, created.StatusCode);
            Assert.Equal("Curso nuevo", await ReadTitleAsync(created));
        }

        [Fact]
        public async Task Create_WithoutTitleAndDescription_Returns400()
        {
            var response = await PostCourseAsync("", "", WriterToken());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errors = await ReadErrorsAsync(response);
            Assert.Contains("Title", errors);
            Assert.Contains("Description", errors);
        }

        [Fact]
        public async Task Create_WithoutToken_Returns401()
        {
            var response = await PostCourseAsync("Curso", "Descripcion", token: null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutWritePolicy_Returns403()
        {
            var response = await PostCourseAsync("Curso", "Descripcion", CreateToken(PolicyMaster.COURSE_READ));

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ---------- PUT /api/courses/{id} ----------

        [Fact]
        public async Task Update_ExistingCourse_Returns200AndPersistsChanges()
        {
            var id = await CreateCourseAsync("Original", "Descripcion");

            var response = await PutCourseAsync(id, "Editado", "Descripcion nueva", UpdaterToken());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await _client.GetAsync($"/api/courses/{id}");
            Assert.Equal("Editado", await ReadTitleAsync(updated));
        }

        [Fact]
        public async Task Update_MissingCourse_Returns404()
        {
            var response = await PutCourseAsync(MissingId, "Editado", "Descripcion", UpdaterToken());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithoutTitle_Returns400()
        {
            var id = await CreateCourseAsync("Original", "Descripcion");

            var response = await PutCourseAsync(id, "", "Descripcion", UpdaterToken());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("Title", await ReadErrorsAsync(response));
        }

        [Fact]
        public async Task Update_WithoutToken_Returns401()
        {
            var response = await PutCourseAsync(MissingId, "Editado", "Descripcion", token: null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Update_WithoutUpdatePolicy_Returns403()
        {
            var response = await PutCourseAsync(MissingId, "Editado", "Descripcion", WriterToken());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ---------- DELETE /api/courses/{id} ----------

        [Fact]
        public async Task Delete_ExistingCourse_Returns200AndRemovesIt()
        {
            var id = await CreateCourseAsync("A borrar", "Descripcion");

            var response = await DeleteCourseAsync(id, CreateToken(PolicyMaster.COURSE_DELETE));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var list = await _client.GetStringAsync("/api/courses?pageSize=100");
            Assert.DoesNotContain(id.ToString(), list);
        }

        [Fact]
        public async Task Delete_WithoutDeletePolicy_Returns403()
        {
            var response = await DeleteCourseAsync(MissingId, WriterToken());

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // ---------- helpers ----------

        private static string WriterToken() => CreateToken(PolicyMaster.COURSE_WRITE);

        private static string UpdaterToken() => CreateToken(PolicyMaster.COURSE_UPDATE);

        private static string CreateToken(params string[] policies)
        {
            var claims = policies.Select(p => new Claim(CustomClaims.POLICIES, p)).ToList();
            claims.Add(new Claim(ClaimTypes.Name, "tester"));

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CustomWebApplicationFactroy.TestTokenKey)),
                SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddHours(1), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        private Task<HttpResponseMessage> PostCourseAsync(string title, string description, string? token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/courses")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["title"] = title,
                    ["description"] = description
                })
            };
            Authorize(request, token);
            return _client.SendAsync(request);
        }

        private Task<HttpResponseMessage> PutCourseAsync(Guid id, string title, string description, string? token)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/courses/{id}")
            {
                Content = JsonContent.Create(new { title, description })
            };
            Authorize(request, token);
            return _client.SendAsync(request);
        }

        private Task<HttpResponseMessage> DeleteCourseAsync(Guid id, string? token)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/courses/{id}");
            Authorize(request, token);
            return _client.SendAsync(request);
        }

        private async Task<Guid> CreateCourseAsync(string title, string description)
        {
            var response = await PostCourseAsync(title, description, WriterToken());
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        private static void Authorize(HttpRequestMessage request, string? token)
        {
            if (token is not null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private static async Task<string?> ReadTitleAsync(HttpResponseMessage response)
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("title").GetString();
        }

        private static async Task<string> ReadErrorsAsync(HttpResponseMessage response)
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return string.Join(",", doc.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name));
        }
    }
}
