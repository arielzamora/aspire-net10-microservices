using Core.MediatOR.Contracts;
using MasterNet.Application.Instructors.GetInstructors;
using MasterNet.Application.Instructors.InstructorCreate;
using MasterNet.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static MasterNet.Application.Instructors.GetInstructors.GetInstructorsQuery;

namespace MasterNet.WebApi.Controllers;

[ApiController]
[Route("api/instructors")]
public class InstructorsController : ControllerBase
{
    private readonly IMediatOR _sender;

    public InstructorsController(IMediatOR sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> PaginationInstructor
    (
        [FromQuery] GetInstructorsRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = new GetInstructorsQueryRequest
        {
            InstructorRequest = request
        };
        var results = await _sender.Send(query, cancellationToken);
        return this.FromResult(results);
    }

    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<Guid>> CreateInstructor
    (
        [FromBody] InstructorCreateCommand.InstructorCreateCommandRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _sender.Send(request, cancellationToken);

        if (result != Guid.Empty)
        {
            return CreatedAtAction(nameof(CreateInstructor), new { id = result }, result);
        }

        return BadRequest("Error creating instructor");
    }
}
