using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;
using stratoapi.Models;
using stratoapi.Services;

namespace stratoapi.Controllers;

public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult>  Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var result = await _authService.Authenticate(request);

        if (result == null)
        {
            return Unauthorized();
        }
        
        return Ok(result);
    }

    [HttpPost("register")]
    [Authorize(Roles = $"{nameof(AuthRole.SeedUser)},{nameof(AuthRole.Admin)}")]
    public async Task<IActionResult> Register([FromBody] NewUserRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _authService.RegisterNewUser(request);
        if (!success)
        {
            return Conflict("Username already exists.");
        }

        return Ok("User registered successfully.");
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var username = User.Identity?.Name;
        var roles = User.Claims
            .Where(c => c.Type == "role")
            .Select(c => c.Value)
            .ToList();
        return Ok(new { Username = username, Roles = roles });
    }
}