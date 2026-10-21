namespace MasterNet.Client.Courses;

public sealed record class CreateCourseRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }

    // ISO yyyy-MM-dd: evita que el formato dependa de la cultura del navegador.
    public string? PublishedAt { get; init; }
}

public sealed record class UpdateCourseRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? PublishedAt { get; init; }
}

public sealed record class CourseOperationResult(bool Success, Guid? Id = null, string? Error = null);
