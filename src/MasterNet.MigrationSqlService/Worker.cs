using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MasterNet.MigrationSqlService;

public class Worker(
    IConfiguration configuration,
    IHostApplicationLifetime applicationLifetime,
    ILogger<Worker> logger
    ) : BackgroundService
{

    public const string ActivitySourceName = "SQLMigrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource
                           .StartActivity("Ejecutando SQL Scripts", ActivityKind.Client);

        try
        {
            var connectionString = configuration.GetConnectionString("MasterNetDB");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    "No existe la cadena de conexion a la db"
                );
            }


            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            var sqlFilePath = Path.Combine(AppContext.BaseDirectory,
                                "Scripts",
                                "CreateDatabaseAndSeed.sql"
                        );

            if (!File.Exists(sqlFilePath))
            {
                throw new FileNotFoundException(
                    $"No se encontro el sql file: {sqlFilePath}"
                 );
            }

            logger.LogInformation("El archivo SQL fue encontrado {FilePath}", sqlFilePath);

            var sqlScript = await File.ReadAllTextAsync(sqlFilePath, stoppingToken);

            var batches = Regex.Split(
                sqlScript,
                @"^\s*GO\s*$",
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            await using var connection = new SqlConnection(builder.ConnectionString);
            await connection.OpenAsync(stoppingToken);

            logger.LogInformation("Leyendo los batchs: {BatchCount}", batches.Length);

            foreach (var batch in batches)
            {
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    await using var command = new SqlCommand(batch, connection);
                    command.CommandTimeout = 60;
                    await command.ExecuteNonQueryAsync(stoppingToken);
                }
            }


            logger.LogInformation("El SQL Script se ejecuto con exito");
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            logger.LogError(ex, "Error ejecutando SQL Scripts");
            throw;
        }

        applicationLifetime.StopApplication();
    }
}
