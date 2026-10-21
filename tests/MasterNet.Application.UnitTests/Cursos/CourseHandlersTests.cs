using MasterNet.Application.Courses.CourseDelete;
using MasterNet.Application.Courses.CourseUpdate;
using MasterNet.Application.Interfaces;
using MasterNet.Domain.Abstractions;
using MasterNet.Domain.Courses;
using MasterNet.Domain.Instructors;
using MasterNet.Domain.Prices;
using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using static MasterNet.Application.Courses.CourseCreate.CreateCourseCommand;
using static MasterNet.Application.Courses.CourseDelete.CourseDeleteCommand;
using static MasterNet.Application.Courses.CourseUpdate.CourseUpdateCommand;

namespace MasterNet.Application.UnitTests.Cursos;

public class CourseHandlersTests
{
    // ---------- alta (usa IUnitOfWork, se mockea) ----------

    [Fact]
    public async Task Create_WithValidData_AddsCourseAndReturnsItsId()
    {
        var courses = new Mock<IGenericRepository<Course>>();
        var unitOfWork = UnitOfWorkWith(courses);
        unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(true);
        var handler = new CreateCourseCommandHandler(unitOfWork.Object, Mock.Of<IPhotoService>());

        var result = await handler.Handle(new CreateCourseCommandRequest("Titulo", "Descripcion", null, null), default);

        Assert.True(result.IsSuccess);
        courses.Verify(r => r.AddAsync(It.Is<Course>(c => c.Id == result.Value && c.Title == "Titulo")), Times.Once);
    }

    [Fact]
    public async Task Create_WithUnknownInstructor_FailsWithoutSaving()
    {
        var instructors = new Mock<IGenericRepository<Instructor>>();
        instructors.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Instructor?)null);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<Instructor>()).Returns(instructors.Object);
        var handler = new CreateCourseCommandHandler(unitOfWork.Object, Mock.Of<IPhotoService>());

        var result = await handler.Handle(new CreateCourseCommandRequest("Titulo", "Descripcion", null, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Instructor not found", result.Error);
        unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Create_WithUnknownPrice_FailsWithoutSaving()
    {
        var prices = new Mock<IGenericRepository<Price>>();
        prices.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Price?)null);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<Price>()).Returns(prices.Object);
        var handler = new CreateCourseCommandHandler(unitOfWork.Object, Mock.Of<IPhotoService>());

        var result = await handler.Handle(new CreateCourseCommandRequest("Titulo", "Descripcion", Guid.NewGuid(), null), default);

        Assert.False(result.IsSuccess);
        Assert.Equal("Price not found", result.Error);
        unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Create_WhenSaveFails_ReturnsFailure()
    {
        var unitOfWork = UnitOfWorkWith(new Mock<IGenericRepository<Course>>());
        unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(false);
        var handler = new CreateCourseCommandHandler(unitOfWork.Object, Mock.Of<IPhotoService>());

        var result = await handler.Handle(new CreateCourseCommandRequest("Titulo", "Descripcion", null, null), default);

        Assert.False(result.IsSuccess);
    }

    // ---------- actualización y baja (usan el DbContext, se prueban con EF InMemory) ----------

    [Fact]
    public async Task Update_ExistingCourse_ChangesItsFields()
    {
        await using var context = NewContext();
        var course = await AddCourseAsync(context, "Original");
        var handler = new CourseUpdateCommandHandler(context);
        var request = new CourseUpdateRequest { Title = "Editado", Description = "Nueva", PublishedAt = new DateTime(2026, 1, 2) };

        var result = await handler.Handle(new CourseUpdateCommandRequest(request, course.Id), default);

        Assert.True(result.IsSuccess);
        var saved = await context.Courses!.SingleAsync(c => c.Id == course.Id);
        Assert.Equal("Editado", saved.Title);
        Assert.Equal("Nueva", saved.Description);
        Assert.Equal(new DateTime(2026, 1, 2), saved.PublishedAt);
    }

    [Fact]
    public async Task Update_MissingCourse_ReturnsNotFound()
    {
        await using var context = NewContext();
        var handler = new CourseUpdateCommandHandler(context);
        var request = new CourseUpdateRequest { Title = "Editado", Description = "Nueva" };

        var result = await handler.Handle(new CourseUpdateCommandRequest(request, Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Delete_ExistingCourse_RemovesIt()
    {
        await using var context = NewContext();
        var course = await AddCourseAsync(context, "A borrar");
        var handler = new CourseDeleteCommandHandler(context);

        var result = await handler.Handle(new CourseDeleteCommandRequest(course.Id), default);

        Assert.True(result.IsSuccess);
        Assert.False(await context.Courses!.AnyAsync(c => c.Id == course.Id));
    }

    [Fact]
    public async Task Delete_MissingCourse_Fails()
    {
        await using var context = NewContext();
        var handler = new CourseDeleteCommandHandler(context);

        var result = await handler.Handle(new CourseDeleteCommandRequest(Guid.NewGuid()), default);

        Assert.False(result.IsSuccess);
    }

    // ---------- helpers ----------

    private static Mock<IUnitOfWork> UnitOfWorkWith(Mock<IGenericRepository<Course>> courses)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Repository<Course>()).Returns(courses.Object);
        return unitOfWork;
    }

    private static MasterNetDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<MasterNetDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new MasterNetDbContext(options);
    }

    private static async Task<Course> AddCourseAsync(MasterNetDbContext context, string title)
    {
        var course = new Course { Id = Guid.NewGuid(), Title = title, Description = "Descripcion" };
        context.Courses!.Add(course);
        await context.SaveChangesAsync();
        return course;
    }
}
