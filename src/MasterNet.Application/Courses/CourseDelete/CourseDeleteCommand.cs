using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MasterNet.Application.Courses.CourseDelete;

public class CourseDeleteCommand
{

    public record CourseDeleteCommandRequest(Guid? CourseId)
    : IRequest<Result<bool>>, ICommandBase;

    internal class CourseDeleteCommandHandler
    : IRequestHandler<CourseDeleteCommandRequest, Result<bool>>
    {
        private readonly MasterNetDbContext _context;

        public CourseDeleteCommandHandler(MasterNetDbContext context)
        {
            _context = context;
        }

        public async Task<Result<bool>> Handle(
            CourseDeleteCommandRequest request,
            CancellationToken cancellationToken
        )
        {
            var course = await _context.Courses!
            .Include(x => x.Instructors)
            .Include(x => x.Prices)
            .Include(x => x.Ratings)
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.Id == request.CourseId);

            if (course is null)
            {
                return Result<bool>.Failure("The course does not exist");
            }

            _context.Courses!.Remove(course);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            return result
                            ? Result<bool>.Success(true)
                            : Result<bool>.Failure("Transaction error");

        }
    }
}
