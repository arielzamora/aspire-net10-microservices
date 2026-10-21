using MasterNet.Application.Core;
using MasterNet.Application.Courses.GetCourse;
using MasterNet.Application.Courses.GetCourses;
using MasterNet.Persistence;
using MasterNet.Persistence.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using static MasterNet.Application.Courses.GetCourses.GetCoursesQuery;

namespace MasterNet.WebApi.Controllers;


[ApiController]
[Route("api/devices")]
public class DeviceController(MasterNetDbContext context) : ControllerBase
{
    private readonly MasterNetDbContext _context = context;


    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Get(
    [FromQuery] string request,
    CancellationToken cancellationToken
    )
    {
        var nameSpec = new DeviceByNameContainsSpec(request);

        var devices = await _context
                        .Devices!
                        .Where(nameSpec.Criteria)
                        .ToListAsync();

        return Ok(devices);
    }
}
