using MasterNet.Client.Core;
using MasterNet.Client.Courses;
using Refit;

namespace MasterNet.Client.Refit;

public interface ICoursesApi
{
    [Get("/api/courses")]
    Task<PagedList<CourseResponse>> GetCoursesAsync([Query] CoursesQueryParameters parameters, CancellationToken cancellationToken = default);
}
