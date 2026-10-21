using MasterNet.Client.Core;
using MasterNet.Client.Courses;
using Refit;

namespace MasterNet.Client.Refit;

public interface ICoursesApi
{
    [Get("/api/courses")]
    Task<PagedList<CourseResponse>> GetCoursesAsync([Query] CoursesQueryParameters parameters, CancellationToken cancellationToken = default);

    [Get("/api/courses/{id}")]
    Task<IApiResponse<CourseResponse>> GetCourseAsync(Guid id, CancellationToken cancellationToken = default);

    // La API recibe el alta como formulario (acepta foto), por eso va url-encoded.
    [Post("/api/courses")]
    Task<IApiResponse<Guid>> CreateCourseAsync([Body(BodySerializationMethod.UrlEncoded)] CreateCourseRequest request, CancellationToken cancellationToken = default);

    [Put("/api/courses/{id}")]
    Task<IApiResponse<Guid>> UpdateCourseAsync(Guid id, [Body] UpdateCourseRequest request, CancellationToken cancellationToken = default);
}
