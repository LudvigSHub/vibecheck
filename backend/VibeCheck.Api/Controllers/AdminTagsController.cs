using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Route("api/admin/tags")]
[Authorize(Roles = "admin")]
public class AdminTagsController : ControllerBase
{
    private readonly AdminTagService _adminTagService;

    public AdminTagsController(AdminTagService adminTagService)
    {
        _adminTagService = adminTagService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminTagListItemDTO>>> GetAll()
    {
        var tags = await _adminTagService.GetAllAsync();

        return Ok(tags);
    }
}