using MasterNet.Client.Core;
using MasterNet.Client.Courses;

namespace MasterNet.Client.Services.Courses;

public interface ICoursesService
{
    Task<PagedList<CourseResponse>> GetAsync(CoursesQueryParameters? parameters = null, CancellationToken cancellationToken = default);

    Task<CourseResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CourseOperationResult> CreateAsync(CourseFormModel model, CancellationToken cancellationToken = default);

    Task<CourseOperationResult> UpdateAsync(Guid id, CourseFormModel model, CancellationToken cancellationToken = default);

    Task<CourseOperationResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
