extern alias WebApi;

using System.Security.Cryptography;
using MasterNet.Application.Contracts;
using MasterNet.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MasterNet.IntegrationTests
{
    // Prueba un solo servicio in-process: la base y el servicio de ratings se reemplazan,
    // el resto de la API (controllers, validacion, autorizacion) es el real.
    public class CustomWebApplicationFactroy : WebApplicationFactory<WebApi::Program>
    {
        // Clave aleatoria por ejecución: nada de claves fijas en el repositorio.
        public static readonly string TestTokenKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.UseSetting("TokenKey", TestTokenKey);
            // AddSqlServerDbContext exige que exista la cadena, aunque despues se reemplace el proveedor.
            builder.UseSetting("ConnectionStrings:MasterNetDB", "Server=127.0.0.1,1433;Database=MasterNetDB;TrustServerCertificate=true");
            builder.UseSetting("services:ratingservice:http:0", "http://localhost:5054");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions));
                services.RemoveAll(typeof(MasterNetDbContext));
                var dbContextDescriptors = services
                    .Where(d => d.ServiceType.IsGenericType
                        && d.ServiceType.GenericTypeArguments.Contains(typeof(MasterNetDbContext)))
                    .ToList();

                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<MasterNetDbContext>(options => options.UseInMemoryDatabase(_databaseName));

                services.RemoveAll<IRatingServiceClient>();
                services.AddSingleton<IRatingServiceClient, FakeRatingServiceClient>();
            });
        }

        private sealed class FakeRatingServiceClient : IRatingServiceClient
        {
            public Task<int> GetRating(string id) => Task.FromResult(0);
            public Task SendRating(string id, int rating) => Task.CompletedTask;
        }
    }
}
