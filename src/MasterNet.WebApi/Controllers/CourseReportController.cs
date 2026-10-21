using MasterNet.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace MasterNet.WebApi.Controllers;

[Route("odata/courses")]
public class CourseReportController : ODataController
{
    private readonly MasterNetDbContext _dbContext;

    public CourseReportController(MasterNetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [EnableQuery]
    [HttpGet]
    public IActionResult Get() => Ok(_dbContext.Courses!.AsQueryable());
}
