namespace MasterNet.Client.Courses;


public sealed record class CourseResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public int Score { get; init; }
    public DateTime? PublishedAt { get; init; }
    public List<InstructorResponse> Instructors { get; init; } = [];
    public List<RatingResponse> Ratings { get; init; } = [];
    public List<PriceResponse> Prices { get; init; } = [];
    public List<PhotoResponse> Photos { get; init; } = [];
}

public sealed record class InstructorResponse
{
    public Guid? Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Degree { get; init; }
}

public sealed record class RatingResponse
{
    public string? Student { get; init; }
    public int? Score { get; init; }
    public string? Comment { get; init; }
    public string? CourseName { get; init; }
}

public sealed record class PriceResponse
{
    public Guid? Id { get; init; }
    public string? Name { get; init; }
    public decimal? CurrentPrice { get; init; }
    public decimal? PromotionPrice { get; init; }
}

public sealed record class PhotoResponse
{
    public Guid? Id { get; init; }
    public string? Url { get; init; }
    public Guid? CourseId { get; init; }
}
