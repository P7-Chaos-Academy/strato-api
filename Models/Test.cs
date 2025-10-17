using System.ComponentModel.DataAnnotations;

namespace stratoapi.Models;

public class Test
{
    [Key]
    public int Id { get; set; }
        
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
}