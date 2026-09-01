using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Identity.Client.NativeInterop;
using VibeCheck.Api.DTOs;
using VibeCheck.Data.Models;

namespace VibeCheck.Api.Services;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly TokenService _tokenService;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResultDTO> RegisterAsync(RegisterRequestDTO request)
    {
        var user = new User
        {
            UserName = request.UserName,
            Email = request.Email
        };

        var createResult =
            await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return new AuthResultDTO
            {
                Succeeded = false,
                Errors = createResult.Errors
                    .Select(error => error.Description)
            };
        }

        var roleResult =
            await _userManager.AddToRoleAsync(user, "user");

        if (!roleResult.Succeeded)
        {
            return new AuthResultDTO
            {
                Succeeded = false,
                Errors = roleResult.Errors
                    .Select(error => error.Description)
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = _tokenService.CreateToken(
            user.Id.ToString(),
            user.UserName!,
            roles);

        return new AuthResultDTO
        {
            Succeeded = true,
            Response = new AuthResponseDTO
            {
                Token = token,
                UserName = user.UserName!,
                Roles = roles
            }
        };
    }

    public async Task<AuthResultDTO> LoginAsync(LoginRequestDTO request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is null)
        {
            return new AuthResultDTO
            {
                Succeeded = false,
                Errors = ["Invalid username or password."]
            };
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: false);

        if (!signInResult.Succeeded)
        {
            return new AuthResultDTO
            {
                Succeeded = false,
                Errors = ["Invalid username or password."]
            };
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = _tokenService.CreateToken(
            user.Id.ToString(),
            user.UserName!,
            roles);

        return new AuthResultDTO
        {
            Succeeded = true,
            Response = new AuthResponseDTO
            {
                Token = token,
                UserName = user.UserName!,
                Roles = roles
            }
        };
    }
}