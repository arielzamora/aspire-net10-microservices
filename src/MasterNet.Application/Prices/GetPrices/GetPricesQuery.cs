using Core.Mappy.Extensions;
using Core.Mappy.Interfaces;
using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Domain.Prices;
using MasterNet.Persistence;
using System.Linq.Expressions;

namespace MasterNet.Application.Prices.GetPrices;

public class GetPricesQuery
{

    public record GetPricesQueryRequest
    : IRequest<Result<PagedList<PriceResponse>>>
    {
        public GetPricesRequest? PricesRequest { get; set; }
    }

    internal class GetPricesQueryHandler :
    IRequestHandler<GetPricesQueryRequest, Result<PagedList<PriceResponse>>>
    {
        private readonly MasterNetDbContext _context;
        private readonly IMapper _mapper;

        public GetPricesQueryHandler(MasterNetDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<PagedList<PriceResponse>>> Handle(
            GetPricesQueryRequest request,
            CancellationToken cancellationToken
        )
        {

            IQueryable<Price> queryable = _context.Prices!;

            var predicate = ExpressionBuilder.New<Price>();

            if (!string.IsNullOrEmpty(request.PricesRequest!.Name))
            {
                predicate = predicate
                .And(y => y.Name!.Contains(request.PricesRequest!.Name));
            }

            if (!string.IsNullOrEmpty(request.PricesRequest!.OrderBy))
            {
                Expression<Func<Price, object>>? orderSelector =
                    request.PricesRequest.OrderBy.ToLower() switch
                    {
                        "name" => x => x.Name!,
                        "price" => x => x.CurrentPrice,
                        _ => x => x.Name!
                    };

                bool orderBy = request.PricesRequest.OrderAsc.HasValue
                    ? request.PricesRequest.OrderAsc.Value
                    : true;

                queryable = orderBy
                            ? queryable.OrderBy(orderSelector)
                            : queryable.OrderByDescending(orderSelector);
            }

            queryable = queryable.Where(predicate);

            var pricesQuery = queryable
                    .ProjectTo<PriceResponse>(_mapper.ConfigurationProvider)
                    .AsQueryable();


            var pagination = await PagedList<PriceResponse>
             .CreateAsync(pricesQuery,
                 request.PricesRequest.PageNumber,
                 request.PricesRequest.PageSize
            );

            return Result<PagedList<PriceResponse>>.Success(pagination);
        }
    }
}


public record PriceResponse(
    Guid? Id,
    string? Name,
    decimal? CurrentPrice,
    decimal? PromotionPrice
)
{
    public PriceResponse() : this(null, null, null, null)
    {
    }
}
