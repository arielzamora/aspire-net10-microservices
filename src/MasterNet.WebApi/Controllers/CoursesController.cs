using Core.MediatOR.Contracts;
using MasterNet.Application.Core;
using MasterNet.Application.Courses.CourseUpdate;
using MasterNet.Application.Courses.GetCourse;
using MasterNet.Application.Courses.GetCourses;
using MasterNet.Domain.Security;
using MasterNet.WebApi.Extensions;
using MasterNet.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static MasterNet.Application.Courses.CourseCreate.CreateCourseCommand;
using static MasterNet.Application.Courses.CourseDelete.CourseDeleteCommand;
using static MasterNet.Application.Courses.CourseReport.GetCoursesReportQuery;
using static MasterNet.Application.Courses.CourseUpdate.CourseUpdateCommand;
using static MasterNet.Application.Courses.GetCourse.GetCourseQuery;
using static MasterNet.Application.Courses.GetCourses.GetCoursesQuery;

namespace MasterNet.WebApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly IMediatOR _sender;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(IMediatOR sender, ILogger<CoursesController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<PagedList<CourseResponse>>> PaginationCourses(
        [FromQuery] GetCoursesRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCoursesQueryRequest { CoursesRequest = request };
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound();
    }


    [Authorize(Policy = PolicyMaster.COURSE_WRITE)]
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CourseCreate(
        [FromForm] CreateCourseRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateCourseCommandRequest(request.Title, request.Description, request.PriceId, request.InstructorId, request.PublishedAt);

        if (request.Photo is not null)
        {
            using var ms = new MemoryStream();
            await request.Photo.CopyToAsync(ms, cancellationToken);
            command = command with { Photo = ms.ToArray() };
        }

        var result = await _sender.Send(command, cancellationToken);

        return this.FromResult(result);
    }

    [Authorize(Policy = PolicyMaster.COURSE_UPDATE)]
    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CourseUpdate(
        [FromBody] CourseUpdateRequest request,
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var command = new CourseUpdateCommandRequest(request, id);
        var result = await _sender.Send(command, cancellationToken);
        return this.FromResult(result);
    }

    [Authorize(Policy = PolicyMaster.COURSE_DELETE)]
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CourseDelete(
        Guid id,
        CancellationToken cancellationToken
        )
    {
        var command = new CourseDeleteCommandRequest(id);
        var result = await _sender.Send(command, cancellationToken);
        return this.FromResult(result);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CourseGet(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCourseQueryRequest { Id = id };
        var result = await _sender.Send(query, cancellationToken);
        return this.FromResult(result);
    }

    [AllowAnonymous]
    [HttpGet("report")]
    public async Task<IActionResult> ReportCSV(CancellationToken cancellationToken)
    {
        var query = new GetCoursesReportQueryRequest();

        try
        {
            // Pass the request cancellation token through to MediatR/handlers
            var bytes = await _sender.Send(query, cancellationToken);

            if (bytes is null || bytes.Length == 0)
            {
                return NoContent();
            }

            return File(bytes, "text/csv", "courses.csv");
        }
        catch (OperationCanceledException)
        {
            // Client disconnected or request was cancelled
            _logger.LogInformation("Report generation cancelled by client or token.");
            // 499 Client Closed Request is non-standard; return 204/202 or similar as appropriate.
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (IOException ioEx)
        {
            // Transport-level I/O error while streaming response
            _logger.LogWarning(ioEx, "I/O error while producing report (possible client disconnect).");
            return StatusCode(StatusCodes.Status500InternalServerError, "I/O error while producing report.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while generating report.");
            return StatusCode(StatusCodes.Status500InternalServerError, "Unexpected error while generating report.");
        }
    }


}
