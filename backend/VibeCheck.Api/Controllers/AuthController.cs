using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VibeCheck.Api.DTOs;
using VibeCheck.Api.Services;

namespace VibeCheck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }



    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDTO>> Register(
   RegisterRequestDTO request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result.Response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login(
    LoginRequestDTO request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Succeeded)
        {
            return Unauthorized(result.Errors);
        }

        return Ok(result.Response);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserDTO> Me()
    {
        var userName = User.Identity?.Name;

        if (userName is null)
        {
            return Unauthorized();
        }

        var roles = User
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value);

        return Ok(new CurrentUserDTO
        {
            UserName = userName,
            Roles = roles
        });
    }


}

   
