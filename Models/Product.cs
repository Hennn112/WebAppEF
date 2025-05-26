namespace WebAppEF.Models;
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public int ProductCategoryId { get; set; }
    public ProductCategories? Category { get; set; }
    public string UserId { get; set; }
    public AppUser User { get; set; }
    public bool IsDeleted { get; set; } 
}
