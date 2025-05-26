using Microsoft.AspNetCore.Identity;
namespace WebAppEF.Models;

public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? ProfilePicturePath { get; set; }
    public List<Product> Products { get; set; } = new();
    
}