using Bogus;
using MasterNet.Domain.Courses;
using MasterNet.Domain.Devices;
using MasterNet.Domain.Instructors;
using MasterNet.Domain.Photos;
using MasterNet.Domain.Prices;
using MasterNet.Domain.Ratings;
using MasterNet.Persistence.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.Persistence;

public class MasterNetDbContext : IdentityDbContext<AppUser>
{
    public MasterNetDbContext(DbContextOptions<MasterNetDbContext> options) : base(options)
    {
    }
    public DbSet<Device>? Devices { get; set; }
    public DbSet<Course>? Courses { get; set; }
    public DbSet<Instructor>? Instructors { get; set; }
    public DbSet<Price>? Prices { get; set; }
    public DbSet<Rating>? Ratings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Device>(cfg =>
        {
            cfg.ToTable("devices");
            cfg.HasKey(x => x.Id);
            cfg.OwnsOne(x => x.DeviceName, nb =>
            {
                nb.Property(p => p.Value)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();
                nb.WithOwner();

            });
        });

        modelBuilder.Entity<Course>().ToTable("courses");
        modelBuilder.Entity<Instructor>().ToTable("instructors");
        modelBuilder.Entity<CourseInstructor>().ToTable("courses_instructors");
        modelBuilder.Entity<Price>().ToTable("prices");
        modelBuilder.Entity<CoursePrice>().ToTable("courses_prices");
        modelBuilder.Entity<Rating>().ToTable("ratings");
        modelBuilder.Entity<Photo>().ToTable("images");

        modelBuilder.Entity<Price>()
            .Property(b => b.CurrentPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Price>()
            .Property(b => b.PromotionPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Price>()
            .Property(b => b.Name)
            .HasColumnType("VARCHAR")
            .HasMaxLength(250);


        modelBuilder.Entity<Course>()
            .HasMany(m => m.Photos)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Course>()
            .HasMany(m => m.Ratings)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasMany(m => m.Prices)
            .WithMany(m => m.Courses)
            .UsingEntity<CoursePrice>(
                j => j
                    .HasOne(p => p.Price)
                    .WithMany(p => p.CoursePrices)
                    .HasForeignKey(p => p.PriceId)

                    ,
                j => j
                    .HasOne(p => p.Course)
                    .WithMany(p => p.CoursePrices)
                    .HasForeignKey(p => p.CourseId)

                    ,
                j =>
                {
                    j.HasKey(t => new { t.PriceId, t.CourseId });
                }

            );


        modelBuilder.Entity<Course>()
        .HasMany(m => m.Instructors)
        .WithMany(m => m.Courses)
        .UsingEntity<CourseInstructor>(
            j => j
                .HasOne(p => p.Instructor)
                .WithMany(p => p.CourseInstructors)
                .HasForeignKey(p => p.InstructorId)

                ,
            j => j
                .HasOne(p => p.Course)
                .WithMany(p => p.CourseInstructors)
                .HasForeignKey(p => p.CourseId)
                ,
            j =>
            {
                j.HasKey(t => new { t.InstructorId, t.CourseId });
            }
        );
    }



}
