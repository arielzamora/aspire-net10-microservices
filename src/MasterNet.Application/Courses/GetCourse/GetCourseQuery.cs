using MasterNet.Application.Core;
using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;
using Core.MediatOR.Contracts;
using Core.Mappy.Interfaces;
using Core.Mappy.Extensions;

namespace MasterNet.Application.Courses.GetCourse;

public class GetCourseQuery
{

    public record GetCourseQueryRequest
    : IRequest<Result<CourseResponse>>
    {
        public Guid Id { get; set; }
    }

    internal class GetCourseQueryHandler
    : IRequestHandler<GetCourseQueryRequest, Result<CourseResponse>>
    {
        private readonly MasterNetDbContext _context;
        private readonly IMapper _mapper;

        public GetCourseQueryHandler(MasterNetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<CourseResponse>> Handle(
            GetCourseQueryRequest request,
            CancellationToken cancellationToken
        )
        {
            var course = await _context.Courses!.Where(x => x.Id == request.Id)
                            .Include(x => x.Instructors)
                            .Include(x => x.Prices)
                            .Include(x => x.Ratings)
                            .Include(x => x.Photos)
                            .ProjectTo<CourseResponse>(_mapper.ConfigurationProvider)
                            .FirstOrDefaultAsync(cancellationToken);

            // Already projected to CourseResponse, no need to Map again.
            return Result<CourseResponse>.Success(course!);
        }
    }
}
