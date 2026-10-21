using MasterNet.Application.Courses.CourseUpdate;
using static MasterNet.Application.Courses.CourseCreate.CreateCourseCommand;
using static MasterNet.Application.Courses.CourseUpdate.CourseUpdateCommand;

namespace MasterNet.Application.UnitTests.Cursos;

public class CourseValidatorsTests
{
    [Fact]
    public void Create_WithTitleAndDescription_HasNoErrors()
    {
        var request = new CreateCourseCommandRequest("Titulo", "Descripcion", null, null);

        var errors = new CreateCourseCommandRequestValidator().Validate(request);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null, "Descripcion", "Title")]
    [InlineData("  ", "Descripcion", "Title")]
    [InlineData("Titulo", null, "Description")]
    [InlineData("Titulo", "", "Description")]
    public void Create_WithMissingField_ReportsThatField(string? title, string? description, string expectedField)
    {
        var request = new CreateCourseCommandRequest(title, description, null, null);

        var errors = new CreateCourseCommandRequestValidator().Validate(request).ToList();

        Assert.Single(errors);
        Assert.Equal(expectedField, errors[0].PropertyName);
    }

    [Fact]
    public void Update_WithTitleAndDescription_HasNoErrors()
    {
        var request = new CourseUpdateCommandRequest(
            new CourseUpdateRequest { Title = "Titulo", Description = "Descripcion" }, Guid.NewGuid());

        var errors = new CourseUpdateCommandRequestValidator().Validate(request);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null, "Descripcion", "Title")]
    [InlineData("", "Descripcion", "Title")]
    [InlineData("Titulo", " ", "Description")]
    public void Update_WithMissingField_ReportsThatField(string? title, string? description, string expectedField)
    {
        var request = new CourseUpdateCommandRequest(
            new CourseUpdateRequest { Title = title, Description = description }, Guid.NewGuid());

        var errors = new CourseUpdateCommandRequestValidator().Validate(request).ToList();

        Assert.Single(errors);
        Assert.Equal(expectedField, errors[0].PropertyName);
    }
}
