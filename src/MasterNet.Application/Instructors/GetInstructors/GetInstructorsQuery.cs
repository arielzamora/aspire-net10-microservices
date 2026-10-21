using Core.Mappy.Extensions;
using Core.Mappy.Interfaces;
using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Domain.Instructors;
using MasterNet.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MasterNet.Application.Instructors.GetInstructors;

public class GetInstructorsQuery
{

    public record GetInstructorsQueryRequest : IRequest<Result<PagedList<InstructorResponse>>>
    {
        public GetInstructorsRequest? InstructorRequest { get; set; }
    }

    internal class GetInstructorsQueryHandler
    : IRequestHandler<GetInstructorsQueryRequest, Result<PagedList<InstructorResponse>>>
    {
        private readonly MasterNetDbContext _context;
        private readonly IMapper _mapper;

        public GetInstructorsQueryHandler(MasterNetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedList<InstructorResponse>>> Handle(
            GetInstructorsQueryRequest request,
            CancellationToken cancellationToken
        )
        {

            IQueryable<Instructor> queryable = _context.Instructors!;


            var predicate = ExpressionBuilder.New<Instructor>();
            if (!string.IsNullOrEmpty(request.InstructorRequest!.FirstName))
            {
                predicate = predicate
                .And(y => y.FirstName!.Contains(request.InstructorRequest.FirstName));
            }

            if (!string.IsNullOrEmpty(request.InstructorRequest!.OrderBy))
            {
                Expression<Func<Instructor, object>>? orderBySelector =
                                request.InstructorRequest.OrderBy!.ToLower() switch
                                {
                                    "firstname" => instructor => instructor.FirstName!,
                                    _ => instructor => instructor.FirstName!
                                };

                bool orderBy = request.InstructorRequest.OrderAsc.HasValue
                            ? request.InstructorRequest.OrderAsc.Value
                            : true;

                queryable = orderBy
                            ? queryable.OrderBy(orderBySelector)
                            : queryable.OrderByDescending(orderBySelector);
            }

            queryable = queryable.Where(predicate);

            var instructorsQuery = queryable
                        .ProjectTo<InstructorResponse>(_mapper.ConfigurationProvider)
                        .AsQueryable();

            var pagination = await PagedList<InstructorResponse>
                .CreateAsync(instructorsQuery,
                request.InstructorRequest.PageNumber,
                request.InstructorRequest.PageSize
                );

            return Result<PagedList<InstructorResponse>>.Success(pagination);
        }
    }
}


public record InstructorResponse(
    Guid? Id,
    string? FirstName,
    string? LastName,
    string? Degree
)
{
    public InstructorResponse() : this(null, null, null, null)
    {
    }
}
