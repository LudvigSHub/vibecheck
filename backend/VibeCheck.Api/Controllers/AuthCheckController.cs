using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthCheckController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("You are authenticated!");
    }
}
