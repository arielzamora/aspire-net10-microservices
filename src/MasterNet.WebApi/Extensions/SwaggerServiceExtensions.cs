//using Microsoft.OpenApi;
//using Microsoft.OpenApi.Models;

//namespace MasterNet.WebApi.Extensions;

//public static class SwaggerServiceExtensions
//{

//    public static IServiceCollection AddSwaggerDocumentation
//    (
//        this IServiceCollection services
//    )
//    {
//        services.AddEndpointsApiExplorer();
//        services.AddSwaggerGen(c =>
//        {
//            var securitySchema = new OpenApiSecurityScheme
//            {
//                Description = "JWT Authorization Bearer Schema",
//                Name = "Authorization",
//                In = ParameterLocation.Header,
//                Type = SecuritySchemeType.Http,
//                Scheme = "Bearer",
//                BearerFormat = "JWT",
//                //Reference = new OpenApiReference
//                //{
//                //    Type = ReferenceType.SecurityScheme,
//                //    Id = "Bearer"
//                //}
//            };
//            c.AddSecurityDefinition("Bearer", securitySchema);

//            //var securityRequirement = new OpenApiSecurityRequirement
//            //{
//            //    { securitySchema, Array.Empty<string>() }
//            //};

//            //c.AddSecurityRequirement(securityRequirement);
//            var securityRequirement = new OpenApiSecurityRequirement
//{
//    { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
//   };
//            c.AddSecurityRequirement(securityRequirement);
//        });


//        return services;
//    }

//    public static IApplicationBuilder useSwaggerDocumentation(
//        this IApplicationBuilder app
//    )
//    {

//        app.UseSwagger();
//        app.UseSwaggerUI();

//        return app;
//    }


//}