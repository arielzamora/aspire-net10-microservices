using MasterNet.Application.Core;

namespace MasterNet.Application.Courses.GetCourses;

public class GetCoursesRequest : PagingParams
{

    public string? Title { get; set; }
    public string? Description { get; set; }

}