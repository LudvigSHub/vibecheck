using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Route("api/admin/words")]
[Authorize(Roles = "admin")]
public class AdminWordsController : ControllerBase
{
    private readonly AdminWordService _adminWordService;

    public AdminWordsController(AdminWordService adminWordService)
    {
        _adminWordService = adminWordService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminWordListItemDTO>>> GetAll()
    {
        var words = await _adminWordService.GetAllAsync();

        return Ok(words);
    }

    [HttpPost]
    public async Task<ActionResult<AdminWordListItemDTO>> Create(
    AdminCreateWordDTO request)
    {
        try
        {
            var createdWord = await _adminWordService.CreateAsync(request);

            return StatusCode(StatusCodes.Status201Created, createdWord);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}