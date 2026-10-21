using Microsoft.AspNetCore.Mvc;

namespace MasterNet.WebApi.Controllers;

[ApiController]
[Route("Demo")]
public class DemoController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public DemoController(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    [HttpGet("getstring")]
    public string GetName()
    {
        return "netuniversity.org";
    }

    [HttpGet("Environment")]
    public IActionResult GetEnvironment()
    {
        var message = _configuration.GetValue<string>("MyVariable");
        var environment = _environment.EnvironmentName;

        return Ok(new { Environment = environment, Mensaje = message });
    }

}