using Core.Mappy.Extensions;
using Core.Mappy.Interfaces;
using Core.MediatOR.Contracts;
using MasterNet.Application.Contracts;
using MasterNet.Application.Core;
using MasterNet.Application.Courses.GetCourse;
using MasterNet.Application.Ratings.GetRatings;
using MasterNet.Domain.Courses;
using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Linq.Expressions;

namespace MasterNet.Application.Courses.GetCourses;

public class GetCoursesQuery
{

    public record GetCoursesQueryRequest : IRequest<Result<PagedList<CourseResponse>>>
    {
        public GetCoursesRequest? CoursesRequest { get; set; }
    }

    internal class GetCoursesQueryHandler
    : IRequestHandler<GetCoursesQueryRequest, Result<PagedList<CourseResponse>>>
    {
        private readonly MasterNetDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRatingServiceClient _ratingServiceClient;

        private static readonly ActivitySource _activitySource = new ActivitySource("MasterNet.Course");

        public GetCoursesQueryHandler(MasterNetDbContext context, IMapper mapper, IRatingServiceClient ratingServiceClient)
        {
            _context = context;
            _mapper = mapper;
            _ratingServiceClient = ratingServiceClient;
        }

        public async Task<Result<PagedList<CourseResponse>>> Handle(
            GetCoursesQueryRequest request,
            CancellationToken cancellationToken
        )
        {

            IQueryable<Course> queryable = _context.Courses!
                                            .Include(x => x.Instructors)
                                            .Include(x => x.Ratings)
                                            .Include(x => x.Prices)
                                            .Include(x => x.Photos);

            var predicate = ExpressionBuilder.New<Course>();
            if (!string.IsNullOrEmpty(request.CoursesRequest!.Title))
            {
                predicate = predicate
                .And(y => y.Title!.ToLower()
                .Contains(request.CoursesRequest.Title.ToLower()));
            }


            if (!string.IsNullOrEmpty(request.CoursesRequest!.Description))
            {
                predicate = predicate
                .And(y => y.Description!.ToLower()
                .Contains(request.CoursesRequest.Description.ToLower()));
            }

            if (!string.IsNullOrEmpty(request.CoursesRequest!.OrderBy))
            {
                Expression<Func<Course, object>>? orderBySelector =
                                request.CoursesRequest.OrderBy!.ToLower() switch
                                {
                                    "title" => course => course.Title!,
                                    "description" => course => course.Description!,
                                    _ => course => course.Title!
                                };

                bool orderBy = request.CoursesRequest.OrderAsc.HasValue
                            ? request.CoursesRequest.OrderAsc.Value
                            : true;

                queryable = orderBy
                            ? queryable.OrderBy(orderBySelector)
                            : queryable.OrderByDescending(orderBySelector);
            }

            queryable = queryable.Where(predicate);

            var coursesQuery = queryable
            .ProjectTo<CourseResponse>(_mapper.ConfigurationProvider)
            .AsQueryable();

            var pagination = await PagedList<CourseResponse>.CreateAsync(
                coursesQuery,
                request.CoursesRequest.PageNumber,
                request.CoursesRequest.PageSize
            );

            using (var activity = _activitySource.StartActivity("Obtengo las Calificaciones")
                )
            {
                for (int i = 0; i < pagination.Items.Count; i++)
                {
                    var course = pagination.Items[i];
                    var rating = await _ratingServiceClient.GetRating(course.Id.ToString());
                    pagination.Items[i] = course with
                    {
                        Score = rating
                    };
                }
                activity?.SetTag("Count", pagination.Items.Count());
            }


            return Result<PagedList<CourseResponse>>.Success(pagination);

        }
    }


}