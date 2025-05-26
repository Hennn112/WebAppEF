using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppEF.Models;

namespace WebAppEF.Controllers;

public class ProductCategoriesController : Controller
{
    private readonly Data.AppDbContext _context;
    private readonly ILogger<ProductCategoriesController> _logger;
    private readonly UserManager<AppUser> _userManager;

    public ProductCategoriesController(Data.AppDbContext context, ILogger<ProductCategoriesController> logger, UserManager<AppUser> userManager)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
    }

    [Route("product-categories")]
    [HttpGet]
    public IActionResult Index()
    {
        var categories = _context.ProductCategories
        .ToList();
        return View(categories);
    }

    [HttpGet]
    [Route("product-categories/create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Route("product-categories/create")]
    public async Task<IActionResult> Create(ProductCategories category, IFormFile? ImageFile)
    {
        var UserId = _userManager.GetUserId(User);
        if (UserId == null)
        {
            ModelState.Clear();
            ModelState.AddModelError("Unauthorized", "Anda harus login untuk menambah kategori produk");
            return View(category);
        }
        var existingProduct = await _context.ProductCategories
            .FirstOrDefaultAsync(p => p.Name == category.Name && !p.IsDeleted);
        if (existingProduct != null)
        {
            ModelState.AddModelError("Name", "Produk dengan nama ini sudah ada");
            ViewBag.Categories = _context.ProductCategories.Where(c => !c.IsDeleted).ToList();
            return View(category);
        }

        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("product-categories/edit/{id}")]
    public IActionResult Edit(int id)
    {
        var category = _context.ProductCategories.Find(id);
        if (category == null) return NotFound();

        return View(category);
    }

    [HttpPost]
    [Route("product-categories/edit/{id}")]
    public async Task<IActionResult> Edit(ProductCategories category, IFormFile? ImageFile)
    {
        var UserId = _userManager.GetUserId(User);
        if (UserId == null)
        {
            ModelState.Clear();
            ModelState.AddModelError("Unauthorized", "Anda harus login untuk mengedit kategori produk");
            return View(category);
        }
        var existing = await _context.ProductCategories
            .FirstOrDefaultAsync(p => p.Id == category.Id);
        if (existing == null) return NotFound();

        var existingProduct = await _context.ProductCategories
            .FirstOrDefaultAsync(p => p.Name == category.Name && !p.IsDeleted);
        if (existingProduct != null)
        {
            ModelState.AddModelError("Name", "Produk dengan nama ini sudah ada");
            ViewBag.Categories = _context.ProductCategories.Where(c => !c.IsDeleted).ToList();
            return View(category);
        }

        existing.Name = category.Name;

        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "ProductCategories");
    }

    [HttpPost]
    public IActionResult SoftDelete(int id)
    {
        var category = _context.ProductCategories.FirstOrDefault(p => p.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        category.IsDeleted = true;
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
