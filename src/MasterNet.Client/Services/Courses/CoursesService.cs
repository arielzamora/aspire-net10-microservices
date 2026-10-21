using MasterNet.Client.Core;
using MasterNet.Client.Courses;
using MasterNet.Client.Refit;
using System.Net;

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

    public async Task<CourseResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _coursesApi.GetCourseAsync(id, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            throw response.Error!;
        }

        return response.Content;
    }

    public async Task<SaveCourseResult> CreateAsync(CourseFormModel model, CancellationToken cancellationToken = default)
    {
        var request = new CreateCourseRequest
        {
            Title = model.Title,
            Description = model.Description,
            PublishedAt = model.PublishedAt?.ToString("yyyy-MM-dd")
        };

        using var response = await _coursesApi.CreateCourseAsync(request, cancellationToken);
        return ToResult(response);
    }

    public async Task<SaveCourseResult> UpdateAsync(Guid id, CourseFormModel model, CancellationToken cancellationToken = default)
    {
        var request = new UpdateCourseRequest
        {
            Title = model.Title,
            Description = model.Description,
            PublishedAt = model.PublishedAt
        };

        using var response = await _coursesApi.UpdateCourseAsync(id, request, cancellationToken);
        return ToResult(response);
    }

    private static SaveCourseResult ToResult(global::Refit.IApiResponse<Guid> response)
        => response.IsSuccessStatusCode
            ? new SaveCourseResult(true, response.Content)
            : new SaveCourseResult(false, Error: ApiErrors.Describe(response.StatusCode, response.Error?.Content));
}
