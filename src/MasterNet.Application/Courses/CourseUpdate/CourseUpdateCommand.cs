using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.Application.Courses.CourseUpdate;

public class CourseUpdateCommand
{

    public record CourseUpdateCommandRequest(
        CourseUpdateRequest CourseUpdateRequest,
        Guid? CourseId
    ) : IRequest<Result<Guid>>, ICommandBase;

    internal class CourseUpdateCommandHandler
    : IRequestHandler<CourseUpdateCommandRequest, Result<Guid>>
    {
        private readonly MasterNetDbContext _context;

        public CourseUpdateCommandHandler(MasterNetDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(
            CourseUpdateCommandRequest request,
            CancellationToken cancellationToken
        )
        {
            var courseId = request.CourseId;

            var course = await _context.Courses!
            .FirstOrDefaultAsync(x => x.Id == courseId);

            if (course is null)
            {
                return Result<Guid>.Failure("The course does not exist");
            }

            course.Description = request.CourseUpdateRequest.Description;
            course.Title = request.CourseUpdateRequest.Title;
            course.PublishedAt = request.CourseUpdateRequest.PublishedAt;

            _context.Entry(course).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync() > 0;

            return result
                        ? Result<Guid>.Success(course.Id)
                        : Result<Guid>.Failure("Errors updating the course");

        }
    }

}
