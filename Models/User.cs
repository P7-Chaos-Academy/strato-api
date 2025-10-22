namespace stratoapi.Models;

public class User : BaseModel
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public AuthRole Role { get; set; } = AuthRole.User;
}

public enum AuthRole
{
    Admin = 1,
    SeedUser = 2,
    User = 3
}