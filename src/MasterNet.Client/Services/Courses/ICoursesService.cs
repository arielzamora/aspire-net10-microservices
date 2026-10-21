using MasterNet.Client.Core;
using MasterNet.Client.Courses;

namespace MasterNet.Client.Services.Courses;

public interface ICoursesService
{
    Task<PagedList<CourseResponse>> GetAsync(CoursesQueryParameters? parameters = null, CancellationToken cancellationToken = default);
}