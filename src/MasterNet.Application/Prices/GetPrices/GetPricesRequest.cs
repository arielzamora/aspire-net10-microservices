using MasterNet.Application.Core;

namespace MasterNet.Application.Prices.GetPrices;

public class GetPricesRequest : PagingParams
{
    public string? Name { get; set; }

}
