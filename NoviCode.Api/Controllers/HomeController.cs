using Microsoft.AspNetCore.Mvc;

namespace NoviCode.Api.Controllers;

[ApiController]
public sealed class HomeController : ControllerBase
{
    [HttpGet("/")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Get() =>
        Ok(new { service = "NoviCode.Api", status = "running" });
}
