using Core.Mappy.Extensions;
using Core.Mappy.Interfaces;
using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Domain.Ratings;
using MasterNet.Persistence;
using System.Linq.Expressions;

namespace MasterNet.Application.Ratings.GetRatings;

public class GetRatingsQuery
{

    public record GetRatingsQueryRequest
    : IRequest<Result<PagedList<RatingResponse>>>
    {
        public GetRatingsRequest? RatingsRequest { get; set; }
    }

    internal class GetRatingsQueryHandler
    : IRequestHandler<GetRatingsQueryRequest, Result<PagedList<RatingResponse>>>
    {
        private readonly MasterNetDbContext _context;
        private readonly IMapper _mapper;

        public GetRatingsQueryHandler(MasterNetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedList<RatingResponse>>> Handle(GetRatingsQueryRequest request, CancellationToken cancellationToken)
        {

            IQueryable<Rating> queryable = _context.Ratings!;


            var predicate = ExpressionBuilder.New<Rating>();
            if (!string.IsNullOrEmpty(request.RatingsRequest!.Student))
            {
                predicate = predicate
                .And(y => y.Student!.Contains(request.RatingsRequest.Student));
            }

            if (request.RatingsRequest.CourseId is not null)
            {
                predicate = predicate
                .And(y => y.CourseId == request.RatingsRequest.CourseId);
            }

            if (!string.IsNullOrEmpty(request.RatingsRequest.OrderBy))
            {
                Expression<Func<Rating, object>>? orderBySelector =
                    request.RatingsRequest.OrderBy.ToLower() switch
                    {
                        "student" => x => x.Student!,
                        "course" => x => x.CourseId!,
                        _ => x => x.Student!
                    };

                bool orderBy = request.RatingsRequest.OrderAsc.HasValue
                                ? request.RatingsRequest.OrderAsc.Value
                                : true;

                queryable = orderBy
                            ? queryable.OrderBy(orderBySelector)
                            : queryable.OrderByDescending(orderBySelector);
            }

            queryable = queryable.Where(predicate);

            var ratingQuery = queryable
                                    .ProjectTo<RatingResponse>(_mapper.ConfigurationProvider)
                                    .AsQueryable();

            var pagination = await PagedList<RatingResponse>
                    .CreateAsync(
                        ratingQuery,
                        request.RatingsRequest.PageNumber,
                        request.RatingsRequest.PageSize
                    );


            return Result<PagedList<RatingResponse>>.Success(pagination);
        }
    }

}


public record RatingResponse(
    string? Student,
    int? Score,
    string? Comment,
    string? CourseName
)
{
    public RatingResponse() : this(null, null, null, null)
    {
    }
}
