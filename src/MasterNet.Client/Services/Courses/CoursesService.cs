using MasterNet.Client.Core;
using MasterNet.Client.Courses;
using MasterNet.Client.Refit;

namespace MasterNet.Client.Services.Courses;

public sealed class CoursesService : ICoursesService
{
    private readonly ICoursesApi _coursesApi;

    public CoursesService(ICoursesApi coursesApi) => _coursesApi = coursesApi;

    public Task<PagedList<CourseResponse>> GetAsync(CoursesQueryParameters? parameters = null, CancellationToken cancellationToken = default)
    {
        var effectiveParameters = parameters ?? new CoursesQueryParameters();
        return _coursesApi.GetCoursesAsync(effectiveParameters, cancellationToken);
    }
}
