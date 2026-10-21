using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MasterNet.WebApi.Controllers
{
    [Route("")]
    [ApiController]
    public class HealthToogleController : ControllerBase
    {
        [HttpGet("make-healthy")]
        public IActionResult MakeHealthy()
        {
            MyHealtCheck.Healthy = true;
            return Ok(new
            {
                message = "Health check set to healthy."
            });
        }

        [HttpGet("make-unhealthy")]
        public IActionResult MakeUnHealthy()
        {
            MyHealtCheck.Healthy = false;
            return Ok(new
            {
                message = "Health check set to unhealthy."
            });
        }

    }
}
