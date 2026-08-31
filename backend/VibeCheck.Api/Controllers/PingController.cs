using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase //Service bara för att testa att flödet fungerar som det ska
                                             //Inte för att använda för själva applikationen sen...
{
    private readonly PingService _pingService;

    public PingController(PingService pingService)
    {
        _pingService = pingService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var word = await _pingService.GetFirstWordAsync();

        return Ok(new
        {
            message = "Backend svarar!",
            wordFromDb = word
        });
    }
}
