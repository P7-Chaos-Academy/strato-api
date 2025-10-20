using stratoapi.Models;

namespace stratoapi.Dtos;

public class NewUserRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public AuthRole Role { get; set; } = AuthRole.User;
}