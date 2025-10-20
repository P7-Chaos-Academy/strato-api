using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using stratoapi.Data;
using stratoapi.Dtos;
using stratoapi.Models;

namespace stratoapi.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;


    public AuthService(IConfiguration configuration, ApplicationDbContext context)
    {
        _context = context;
        _configuration = configuration;

    }
    
    // Admin authentication with username and password
    public async Task<LoginResponseDto?> Authenticate(LoginRequestDto loginDto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            
        if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
        {
            return null;
        }
        
        var token = GenerateToken(user);
        return new LoginResponseDto
        {
            Token = token,
        };
    }
    
    // Register new admin user
    public async Task<Boolean> RegisterNewUser(NewUserRequestDto registrationDto)
    {
        // Check if username already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == registrationDto.Username);
            
        if (existingUser != null)
        {
            return false; // Username already taken
        }
        
        var newUser = new User
        {
            Username = registrationDto.Username,
            PasswordHash = HashPassword(registrationDto.PasswordHash)
        };
        
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "Backend";
        var audience = jwtSettings["Audience"] ?? "Users";
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string HashPassword(string password)
    {
        // Store password as plaintext (no hashing)
        return password;
    }
    
    public bool VerifyPassword(string password, string storedPassword)
    {
        return password == storedPassword;
    }
}