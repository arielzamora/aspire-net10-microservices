using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Application.Prices.GetPrices;
using MasterNet.Domain.Prices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static MasterNet.Application.Prices.GetPrices.GetPricesQuery;

namespace MasterNet.WebApi.Controllers;

[ApiController]
[Route("api/prices")]
public class PricesController : ControllerBase
{
    private readonly IMediatOR _sender;

    public PricesController(IMediatOR sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<PagedList<Price>>> PaginationPrice
    (
        [FromQuery] GetPricesRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = new GetPricesQueryRequest
        {
            PricesRequest = request
        };
        var results = await _sender.Send(query, cancellationToken);
        return results.IsSuccess ? Ok(results.Value) : NotFound();
    }

}
