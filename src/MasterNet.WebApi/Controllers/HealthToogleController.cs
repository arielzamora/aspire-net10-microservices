using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MasterNet.WebApi.Controllers
{
    // Demo del curso: solo existe en Development, porque permite poner la app "no sana" a cualquiera que llame al endpoint.
    [Route("")]
    [ApiController]
    public class HealthToogleController(IWebHostEnvironment environment) : ControllerBase
    {
        [HttpGet("make-healthy")]
        public IActionResult MakeHealthy()
        {
            if (!environment.IsDevelopment())
            {
                return NotFound();
            }

            MyHealtCheck.Healthy = true;
            return Ok(new
            {
                message = "Health check set to healthy."
            });
        }

        [HttpGet("make-unhealthy")]
        public IActionResult MakeUnHealthy()
        {
            if (!environment.IsDevelopment())
            {
                return NotFound();
            }

            MyHealtCheck.Healthy = false;
            return Ok(new
            {
                message = "Health check set to unhealthy."
            });
        }

    }
}
