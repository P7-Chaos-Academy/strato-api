using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using stratoapi.Dtos;
using stratoapi.Models;
using stratoapi.Services;

namespace stratoapi.Controllers;

public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("[AuthController] POST /login - Username: {Username}", request.Username);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AuthController] Login validation failed for {Username}: {Errors}",
                request.Username, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        var result = await _authService.Authenticate(request);

        if (result == null)
        {
            _logger.LogWarning("[AuthController] Login unauthorized for {Username}", request.Username);
            return Unauthorized();
        }

        _logger.LogInformation("[AuthController] Login successful for {Username}", request.Username);
        return Ok(result);
    }

    [HttpPost("register")]
    [Authorize(Roles = $"{nameof(AuthRole.SeedUser)},{nameof(AuthRole.Admin)}")]
    public async Task<IActionResult> Register([FromBody] NewUserRequestDto request)
    {
        var currentUser = User.Identity?.Name ?? "unknown";
        _logger.LogInformation("[AuthController] POST /register - New user: {Username}, Requested by: {CurrentUser}",
            request.Username, currentUser);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("[AuthController] Register validation failed for {Username}: {Errors}",
                request.Username, string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        var success = await _authService.RegisterNewUser(request);
        if (!success)
        {
            _logger.LogWarning("[AuthController] Registration failed - Username already exists: {Username}", request.Username);
            return Conflict("Username already exists.");
        }

        _logger.LogInformation("[AuthController] User registered successfully: {Username}", request.Username);
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

        _logger.LogDebug("[AuthController] GET /me - User: {Username}, Roles: [{Roles}]",
            username, string.Join(", ", roles));

        return Ok(new { Username = username, Roles = roles });
    }
}