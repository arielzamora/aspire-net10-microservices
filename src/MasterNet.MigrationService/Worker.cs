using MasterNet.Domain.Courses;
using MasterNet.Domain.Instructors;
using MasterNet.Domain.Prices;
using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace MasterNet.MigrationService;

public class Worker(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime
    ) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = ActivitySource
                              .StartActivity("Migrando Database", ActivityKind.Client);
        try
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MasterNetDbContext>();

            await EnsureDatabaseAsync(context, stoppingToken);
            await RunMigrationAsync(context, stoppingToken);
            await SeedDataAsync(context, stoppingToken);


        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }


        hostApplicationLifetime.StopApplication();

    }

    private static async Task EnsureDatabaseAsync(
        MasterNetDbContext dbContext,
        CancellationToken cancellationToken
        )
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }

        });
    }


    private static async Task RunMigrationAsync(
        MasterNetDbContext context,
        CancellationToken cancellationToken
    )
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database
                                                .BeginTransactionAsync(cancellationToken);

            await context.Database.MigrateAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

    }


    private static async Task SeedDataAsync(
        MasterNetDbContext context,
        CancellationToken cancellationToken
        )
    {
        if (await context.Courses!.AnyAsync(cancellationToken))
        {
            return;
        }

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => {
            await using var transaction = await context.Database
                                             .BeginTransactionAsync(cancellationToken);


            // Instructors
            var instructors = new List<Instructor>
            {
                new() { Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"), FirstName = "John", LastName = "Smith", Degree = "PhD in Computer Science" },
                new() { Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"), FirstName = "Maria", LastName = "Garcia", Degree = "Master in Software Engineering" },
                new() { Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"), FirstName = "David", LastName = "Chen", Degree = "PhD in Artificial Intelligence" },
                new() { Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"), FirstName = "Sarah", LastName = "Johnson", Degree = "Master in Data Science" },
                new() { Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"), FirstName = "Michael", LastName = "Brown", Degree = "PhD in Cybersecurity" },
            };

            context.Instructors!.AddRange(instructors);
            await context.SaveChangesAsync(cancellationToken);

            // Prices
            var prices = new List<Price>
            {
                new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Free", CurrentPrice = 0.00m, PromotionPrice = 0.00m },
                new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Basic", CurrentPrice = 29.99m, PromotionPrice = 19.99m },
                new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Standard", CurrentPrice = 49.99m, PromotionPrice = 39.99m },
                new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Premium", CurrentPrice = 99.99m, PromotionPrice = 79.99m },
                new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Enterprise", CurrentPrice = 199.99m, PromotionPrice = 149.99m },
            };

            context.Prices!.AddRange(prices);
            await context.SaveChangesAsync(cancellationToken);

            // Courses
            var courses = new List<Course>
            {
                new() { Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"), Title = "ASP.NET Core Fundamentals", Description = "Learn the basics of ASP.NET Core web development", PublishedAt = new DateTime(2024, 1, 15) },
                new() { Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"), Title = "Advanced C# Programming", Description = "Master advanced C# concepts and patterns", PublishedAt = new DateTime(2024, 2, 1) },
                new() { Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"), Title = "Blazor WebAssembly Complete Guide", Description = "Build modern web applications with Blazor", PublishedAt = new DateTime(2024, 3, 10) },
                new() { Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"), Title = "Entity Framework Core Mastery", Description = "Deep dive into EF Core and database design", PublishedAt = new DateTime(2024, 4, 5) },
                new() { Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"), Title = "Microservices with .NET", Description = "Build scalable microservices architecture", PublishedAt = new DateTime(2024, 5, 20) },
                new() { Id = Guid.Parse("c6666666-6666-6666-6666-666666666666"), Title = "Azure DevOps and CI/CD", Description = "Implement continuous integration and deployment", PublishedAt = new DateTime(2024, 6, 15) },
                new() { Id = Guid.Parse("c7777777-7777-7777-7777-777777777777"), Title = "Clean Architecture in .NET", Description = "Learn clean code principles and architecture", PublishedAt = new DateTime(2024, 7, 1) },
                new() { Id = Guid.Parse("c8888888-8888-8888-8888-888888888888"), Title = "Docker and Kubernetes for .NET", Description = "Containerize and orchestrate .NET applications", PublishedAt = new DateTime(2024, 8, 10) },
            };

            context.Courses!.AddRange(courses);
            await context.SaveChangesAsync(cancellationToken);


            // Courses-Instructors
            var courseInstructors = new List<CourseInstructor>
            {
                new() { CourseId = Guid.Parse("c1111111-1111-1111-1111-111111111111"), InstructorId = Guid.Parse("a1111111-1111-1111-1111-111111111111") },
                new() { CourseId = Guid.Parse("c1111111-1111-1111-1111-111111111111"), InstructorId = Guid.Parse("a2222222-2222-2222-2222-222222222222") },
                new() { CourseId = Guid.Parse("c2222222-2222-2222-2222-222222222222"), InstructorId = Guid.Parse("a1111111-1111-1111-1111-111111111111") },
                new() { CourseId = Guid.Parse("c3333333-3333-3333-3333-333333333333"), InstructorId = Guid.Parse("a2222222-2222-2222-2222-222222222222") },
                new() { CourseId = Guid.Parse("c4444444-4444-4444-4444-444444444444"), InstructorId = Guid.Parse("a3333333-3333-3333-3333-333333333333") },
                new() { CourseId = Guid.Parse("c5555555-5555-5555-5555-555555555555"), InstructorId = Guid.Parse("a4444444-4444-4444-4444-444444444444") },
                new() { CourseId = Guid.Parse("c6666666-6666-6666-6666-666666666666"), InstructorId = Guid.Parse("a5555555-5555-5555-5555-555555555555") },
                new() { CourseId = Guid.Parse("c7777777-7777-7777-7777-777777777777"), InstructorId = Guid.Parse("a1111111-1111-1111-1111-111111111111") },
                new() { CourseId = Guid.Parse("c8888888-8888-8888-8888-888888888888"), InstructorId = Guid.Parse("a5555555-5555-5555-5555-555555555555") },
            };

            context.Set<CourseInstructor>().AddRange(courseInstructors);
            await context.SaveChangesAsync(cancellationToken);

            // Courses-Prices
            var coursePrices = new List<CoursePrice>
            {
                new() { CourseId = Guid.Parse("c1111111-1111-1111-1111-111111111111"), PriceId = Guid.Parse("22222222-2222-2222-2222-222222222222") },
                new() { CourseId = Guid.Parse("c2222222-2222-2222-2222-222222222222"), PriceId = Guid.Parse("33333333-3333-3333-3333-333333333333") },
                new() { CourseId = Guid.Parse("c3333333-3333-3333-3333-333333333333"), PriceId = Guid.Parse("33333333-3333-3333-3333-333333333333") },
                new() { CourseId = Guid.Parse("c4444444-4444-4444-4444-444444444444"), PriceId = Guid.Parse("44444444-4444-4444-4444-444444444444") },
                new() { CourseId = Guid.Parse("c5555555-5555-5555-5555-555555555555"), PriceId = Guid.Parse("44444444-4444-4444-4444-444444444444") },
                new() { CourseId = Guid.Parse("c6666666-6666-6666-6666-666666666666"), PriceId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
                new() { CourseId = Guid.Parse("c7777777-7777-7777-7777-777777777777"), PriceId = Guid.Parse("33333333-3333-3333-3333-333333333333") },
                new() { CourseId = Guid.Parse("c8888888-8888-8888-8888-888888888888"), PriceId = Guid.Parse("55555555-5555-5555-5555-555555555555") },
            };

            context.Set<CoursePrice>().AddRange(coursePrices);
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

        });
    }

}
