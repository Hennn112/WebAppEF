using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using WebAppEF.Models;

namespace WebAppEF.Controllers;

public class ProductController : Controller
{
    private readonly Data.AppDbContext _context;
    private readonly ILogger<ProductController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<AppUser> _userManager;

    public ProductController(Data.AppDbContext context, ILogger<ProductController> logger, IWebHostEnvironment env, UserManager<AppUser> userManager)
    {
        _context = context;
        _logger = logger;
        _env = env;
        _userManager = userManager;
    }

    [Route("products")]
    [HttpGet]
    public IActionResult Index()
    {
        var products = _context.Products
        .Include(p => p.Category)
        .Include(p => p.User)
        .Where(p => !p.IsDeleted)
        .ToList();

        return View(products);
    }

    [HttpGet]
    [Route("products/create")]
    public IActionResult Create()
    {
        ViewBag.Categories = _context.ProductCategories
                                    .Where(c => !c.IsDeleted)
                                    .ToList();
        return View(new Product());
    }

    [HttpPost]
    [Route("products/create")]
    public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
    {
        // Validasi ID kategori
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            ModelState.Clear();
            ModelState.AddModelError("Unauthorized", "Anda harus login untuk membuat produk");
            ViewBag.Categories = _context.ProductCategories.Where(c => !c.IsDeleted).ToList();
            return View(product);
        }
        product.UserId = userId;
        product.User = await _userManager.GetUserAsync(User);
        var category = await _context.ProductCategories.FindAsync(product.ProductCategoryId);
        if (category == null)
        {
            ModelState.AddModelError("", "Kategori tidak ditemukan");
            return View(product);
        }
        product.Category = category;
        
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Name == product.Name && !p.IsDeleted);
        if (existingProduct != null)
        {
            ModelState.AddModelError("Name", "Produk dengan nama ini sudah ada");
            ViewBag.Categories = _context.ProductCategories.Where(c => !c.IsDeleted).ToList();
            return View(product);
        }

        if (ImageFile != null && ImageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
            var path = Path.Combine(_env.WebRootPath, "Images", fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await ImageFile.CopyToAsync(stream);
            product.Image = $"Images/{fileName}";
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }


    [HttpGet]
    [Route("products/edit/{id}")]
    public IActionResult Edit(int id)
    {
        var product = _context.Products.Find(id);

        if (product == null || product.IsDeleted) return NotFound();

        ViewBag.Categories = _context.ProductCategories.Where(c => !c.IsDeleted).ToList();
        return View(product);
    }

    [HttpPost]
    [Route("products/edit/{id}")]
    public async Task<IActionResult> Edit(Product product, IFormFile? ImageFile)
    {
        var UserId = _userManager.GetUserId(User);
        if (UserId == null)
        {
            ModelState.Clear();
            ModelState.AddModelError("Unauthorized", "Anda harus login untuk mengedit produk");
            ViewBag.Categories = _context.ProductCategories.Where(c => !c.IsDeleted).ToList();
            return View(product);
        }
        product.UserId = UserId;
        product.User = await _userManager.GetUserAsync(User);
        var category = await _context.ProductCategories.FindAsync(product.ProductCategoryId);
        if (category == null)
        {
            ModelState.AddModelError("", "Kategori tidak ditemukan");
        }

        var existing = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        if (existing == null) return NotFound();

        var duplicate = await _context.Products
            .FirstOrDefaultAsync(p => p.Name == product.Name && p.Name != existing.Name && !p.IsDeleted);
        if (duplicate != null)
        {
            ModelState.AddModelError("Name", "Produk dengan nama ini sudah ada");
        }

        ModelState.Remove(nameof(Product.Image));
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _context.ProductCategories.Where(c => !c.IsDeleted).ToList();
            return View(product);
        }
        existing.Name = product.Name;
        existing.ProductCategoryId = product.ProductCategoryId;
        existing.Category = category;

        if (ImageFile != null && ImageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
            var path = Path.Combine(_env.WebRootPath, "Images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            existing.Image = $"Images/{fileName}";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }




    [HttpPost]
    public IActionResult SoftDelete(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        product.IsDeleted = true;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
