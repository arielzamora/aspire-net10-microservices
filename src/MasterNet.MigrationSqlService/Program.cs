
using MasterNet.MigrationSqlService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MasterNet.Persistence;


var builder = Host.CreateApplicationBuilder(args);  

builder.Services.AddHostedService<Worker>();


builder.AddServiceDefaults();

builder.AddSqlServerDbContext<MasterNetDbContext>(connectionName:"MasterNetDB");

var host = builder.Build();
host.Run(); 

