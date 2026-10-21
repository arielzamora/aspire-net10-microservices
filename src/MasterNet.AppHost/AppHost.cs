using MasterNet.AppHost.AppHostEvents;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// En modo publicación (azd / aspire publish) se arma la topología de Azure; en local no cambia nada.
var isPublish = builder.ExecutionContext.IsPublishMode;

if (isPublish)
{
    builder.AddAzureContainerAppEnvironment("env");
}

// Secretos: nada de esto vive en el repositorio. Los generados los crea Aspire y los guarda en
// los user-secrets de este proyecto; la contraseña del admin la pide el dashboard al primer arranque.
var tokenKey = ParameterResourceBuilderExtensions.CreateGeneratedParameter(
    builder, "token-key", secret: true, new GenerateParameterDefault { MinLength = 48, Special = false });

IResourceBuilder<ParameterResource>? seedAdminPassword = null;

if (!isPublish)
{
    // Solo se usa para sembrar usuarios en Development: no se publica a Azure.
    seedAdminPassword = builder.AddParameter("seed-admin-password", secret: true)
        .WithDescription("Contraseña de los usuarios de prueba que se crean en Development (admin: azamora@gmail.com).");

    // Demos del curso (parámetro y cadena de conexión de ejemplo).
    builder.AddParameter("myParameter");
    builder.AddConnectionString("myConnectionString");
}

// SQL Server: contenedor en local, Azure SQL (serverless) al publicar.
var server = builder.AddAzureSqlServer("server")
    .RunAsContainer(container => container
        .WithHostPort(1433)
        .WithLifetime(ContainerLifetime.Persistent));

var db = server.AddDatabase("MasterNetDB");

var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

if (!isPublish)
{
    cache.WithRedisCommander();
}

var ratingService = builder.AddProject<MasterNet_RatingService>("ratingservice")
    .WithReference(cache)
    .WaitFor(cache);

var api = builder.AddProject<Projects.MasterNet_WebApi>("api")
           .WithReference(db)
           .WithReference(ratingService)
           .WithEnvironment("TokenKey", tokenKey)
           .WaitFor(db)
           .WaitFor(ratingService);

if (isPublish)
{
    // El navegador llama a la API directamente: tiene que ser accesible desde fuera.
    api.WithHttpEndpoint(name: "http")
       .WithExternalHttpEndpoints();

    // El cliente Blazor WebAssembly son archivos estáticos: se publica como contenedor nginx
    // (ver src/MasterNet.Client/Dockerfile). La URL de la API se inyecta al arrancar el contenedor.
    builder.AddDockerfile("client", "../MasterNet.Client")
        .WithHttpEndpoint(targetPort: 80)
        .WithExternalHttpEndpoints()
        .WithEnvironment("API_URL", api.GetEndpoint("http"))
        .WaitFor(api);
}
else
{
    // Puerto fijo en local (el cliente apunta a http://localhost:5001). En Azure el ingress lo define Container Apps.
    api.WithHttpEndpoint(port: 5001)
       .WithEnvironment("SeedAdmin__Password", seedAdminPassword!);

    builder.AddProject<Projects.MasterNet_Client>("client")
        .WithReference(api)
        .WaitFor(api)
        .WithExternalHttpEndpoints();

    //builder.AddProject<MasterNet_MigrationService>("migration")
    //.WithReference(db)
    //.WaitFor(db);

    var migrationSql = builder.AddProject<MasterNet_MigrationSqlService>("migrationSql")
        .WithReference(db)
        .WaitFor(db)
        .WithParentRelationship(server);

    // La API necesita el esquema y los roles antes de arrancar (seed de usuarios en Development).
    // En Azure las migraciones son un paso del pipeline, no un worker.
    api.WaitForCompletion(migrationSql);
}

//builder.SubscribeToAppHostEvents();

builder.SubscribeToResourcetEvents(api.Resource);

builder.Build().Run();
