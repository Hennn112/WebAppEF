namespace WebAppEF.Models;
public class ProductCategories
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }

    public List<Product> Products { get; set; } = new();
}
