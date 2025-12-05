using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly ILogger<AuthService> _logger;

    public readonly struct PasswordHashAndSalt
    {
        public byte[] PasswordHash { get; }
        public byte[] PasswordSalt { get; }
        public PasswordHashAndSalt(byte[] passwordHash, byte[] passwordSalt)
        {
            PasswordHash = passwordHash;
            PasswordSalt = passwordSalt;
        }
    }

    public AuthService(IConfiguration configuration, ApplicationDbContext context, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    // Admin authentication with username and password
    public async Task<LoginResponseDto?> Authenticate(LoginRequestDto loginDto)
    {
        _logger.LogInformation("[AuthService] Login attempt for user: {Username}", loginDto.Username);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

        if (user == null)
        {
            _logger.LogWarning("[AuthService] Login failed - User not found: {Username}", loginDto.Username);
            return null;
        }

        if (!VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
        {
            _logger.LogWarning("[AuthService] Login failed - Invalid password for user: {Username}", loginDto.Username);
            return null;
        }

        _logger.LogInformation("[AuthService] Login successful for user: {Username} (ID: {UserId}, Role: {Role})",
            user.Username, user.Id, user.Role);

        var token = GenerateToken(user);
        return new LoginResponseDto
        {
            Token = token,
        };
    }

    // Register new admin user
    public async Task<Boolean> RegisterNewUser(NewUserRequestDto registrationDto)
    {
        _logger.LogInformation("[AuthService] Registration attempt for username: {Username}", registrationDto.Username);

        // Check if username already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == registrationDto.Username);

        if (existingUser != null)
        {
            _logger.LogWarning("[AuthService] Registration failed - Username already exists: {Username}", registrationDto.Username);
            return false; // Username already taken
        }

        PasswordHashAndSalt hashAndSalt = HashPassword(registrationDto.Password);

        var newUser = new User
        {
            Username = registrationDto.Username,
            PasswordHash = hashAndSalt.PasswordHash,
            PasswordSalt = hashAndSalt.PasswordSalt
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("[AuthService] User registered successfully: {Username} (ID: {UserId})",
            newUser.Username, newUser.Id);
        return true;
    }
    
    public string GenerateToken(User user)
    {
        _logger.LogDebug("[AuthService] Generating JWT token for user: {Username} (ID: {UserId})", user.Username, user.Id);

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

        var expiresAt = DateTime.UtcNow.AddHours(24);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        _logger.LogDebug("[AuthService] JWT token generated - Issuer: {Issuer}, Audience: {Audience}, Expires: {Expires}",
            issuer, audience, expiresAt);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public PasswordHashAndSalt HashPassword(string password)
    {
        HMACSHA512 hmac = new HMACSHA512();
        byte[] saltPassword = hmac.Key;
        byte[] hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return new PasswordHashAndSalt(hashedPassword, saltPassword);
    }
    
    public bool VerifyPassword(string password, byte[] storedHashedPassword, byte[] storedSalt)
    {
        HMACSHA512 hmac = new HMACSHA512(storedSalt);
        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(storedHashedPassword);
    }
}