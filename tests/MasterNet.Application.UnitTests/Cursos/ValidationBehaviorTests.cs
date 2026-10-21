using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using static MasterNet.Application.Courses.CourseCreate.CreateCourseCommand;

namespace MasterNet.Application.UnitTests.Cursos;

public class ValidationBehaviorTests
{
    private static readonly IValidator<CreateCourseCommandRequest>[] Validators =
        [new CreateCourseCommandRequestValidator()];

    [Fact]
    public async Task Handle_WithInvalidRequest_ReturnsFailureAndSkipsTheHandler()
    {
        var behavior = new ValidationBehavior<CreateCourseCommandRequest, Result<Guid>>(Validators);
        var nextCalled = false;

        var result = await behavior.Handle(
            new CreateCourseCommandRequest("", "", null, null),
            default,
            () =>
            {
                nextCalled = true;
                return Task.FromResult(Result<Guid>.Success(Guid.NewGuid()));
            });

        Assert.False(nextCalled);
        Assert.False(result.IsSuccess);
        Assert.Equal(["Title", "Description"], result.ValidationErrors!.Select(e => e.PropertyName));
    }

    [Fact]
    public async Task Handle_WithValidRequest_CallsTheHandler()
    {
        var behavior = new ValidationBehavior<CreateCourseCommandRequest, Result<Guid>>(Validators);
        var expected = Guid.NewGuid();

        var result = await behavior.Handle(
            new CreateCourseCommandRequest("Titulo", "Descripcion", null, null),
            default,
            () => Task.FromResult(Result<Guid>.Success(expected)));

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }
}
