using Refit;

namespace MasterNet.Client.Courses;

public class CoursesQueryParameters
{
    [AliasAs("title")]
    public string? Title { get; set; }

    [AliasAs("description")]
    public string? Description { get; set; }

    [AliasAs("pageNumber")]
    public int PageNumber { get; set; } = 1;

    [AliasAs("pageSize")]
    public int PageSize { get; set; } = 10;

    [AliasAs("orderBy")]
    public string? OrderBy { get; set; }

    [AliasAs("orderAsc")]
    public bool? OrderAsc { get; set; } = true;
}