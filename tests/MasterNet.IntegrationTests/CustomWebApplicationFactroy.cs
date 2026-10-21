using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MasterNet.IntegrationTests
{
    public class CustomWebApplicationFactroy : WebApplicationFactory<Program>
    {

        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureHostConfiguration(configBuilder =>
            {
                configBuilder.AddInMemoryCollection([
                    new KeyValuePair<string, string?>("ConnectionStrings:MasterNetDb","Server=127.0.0.1,1433;User ID=sa;Password=Vaxi$2025USPass;TrustServerCertificate=true;Initial Catalog=MasterNetDB"),
                    new KeyValuePair<string, string?>("services:ratingservice:http:0","https://localhost:7160"),
                    new KeyValuePair<string, string?>("services:ratingservice:https:0", "https://localhost:7160")
                    ]);


            });
            return base.CreateHost(builder);
        }
    }
}
