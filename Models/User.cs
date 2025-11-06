namespace stratoapi.Models;
using System.ComponentModel.DataAnnotations;

public class User : BaseModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public byte[] PasswordHash { get; set; }
    
    [Required]
    public byte[] PasswordSalt { get; set; }
    public AuthRole Role { get; set; } = AuthRole.User;
}

public enum AuthRole
{
    Admin = 1,
    SeedUser = 2,
    User = 3
}