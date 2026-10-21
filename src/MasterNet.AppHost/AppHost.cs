using MasterNet.AppHost.AppHostEvents;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);


// Secretos: nada de esto vive en el repositorio. Los generados los crea Aspire y los guarda en
// los user-secrets de este proyecto; la contraseña del admin la pide el dashboard al primer arranque.
var tokenKey = ParameterResourceBuilderExtensions.CreateGeneratedParameter(
    builder, "token-key", secret: true, new GenerateParameterDefault { MinLength = 48, Special = false });

var seedAdminPassword = builder.AddParameter("seed-admin-password", secret: true)
    .WithDescription("Contraseña de los usuarios de prueba que se crean en Development (admin: azamora@gmail.com).");

var myParameter = builder.AddParameter("myParameter");

var myConnectionString = builder.AddConnectionString("myConnectionString");


var server = builder.AddSqlServer("server", port: 1433)
    .WithLifetime(ContainerLifetime.Persistent);

var db =server.AddDatabase("MasterNetDB");

var cache = builder.AddRedis("cache")
    .WithRedisCommander()
    .WithLifetime(ContainerLifetime.Persistent);

var ratingService = builder.AddProject<MasterNet_RatingService>("ratingservice")
    .WithReference(cache)
    .WaitFor(cache);



var api = builder.AddProject<Projects.MasterNet_WebApi>("api")
           .WithHttpEndpoint(port: 5001)
           .WithReference(db)
           .WithReference(ratingService)
           .WithEnvironment("TokenKey", tokenKey)
           .WithEnvironment("SeedAdmin__Password", seedAdminPassword)
           .WaitFor(db)
           .WaitFor(ratingService);
          
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
api.WaitForCompletion(migrationSql);

//builder.SubscribeToAppHostEvents();

builder.SubscribeToResourcetEvents(api.Resource);

builder.Build().Run();
