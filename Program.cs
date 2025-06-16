using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendingEmails;
using WebAppEF.Data;
using WebAppEF.Models;

var builder = WebApplication.CreateBuilder(args);

// Setup DbContext pakai SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));

builder.Services.AddSession();

builder.Services.AddTransient<IEmailSender, EmailSender>();
// Setup Identity dengan AppUser dan Role
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseSession();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Migrasi database otomatis
    db.Database.Migrate();

    // Seed Roles
    string[] roles = new[] { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed Admin User
    string adminUserName = "admin";
    string adminEmail = "admin@gmail.com";
    string adminPassword = "Admin_123";

    var adminUser = await userManager.FindByNameAsync(adminUserName);
    if (adminUser == null)
    {
        adminUser = new AppUser
        {
            UserName = adminUserName,
            Email = adminEmail,
            FullName = "Administrator",
            EmailConfirmed = true,
        };

        var createUserResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createUserResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("Admin user created successfully.");
        }
        else
        {
            foreach (var error in createUserResult.Errors)
            {
                Console.WriteLine($"Error creating admin user: {error.Description}");
            }
        }
    }

    // Seed ProductCategory & Product jika belum ada
    if (!db.ProductCategories.Any())
    {
        var cat1 = new ProductCategories { Name = "Electronics" };
        var cat2 = new ProductCategories { Name = "Books" };

        // Simpan kategori dulu agar ID-nya terisi
        db.ProductCategories.AddRange(cat1, cat2);
        await db.SaveChangesAsync();

        // Ambil ulang dari database (untuk memastikan tracking dan ID valid)
        var electronics = await db.ProductCategories.FirstOrDefaultAsync(c => c.Name == "Electronics");
        var books = await db.ProductCategories.FirstOrDefaultAsync(c => c.Name == "Books");

        // Buat produk setelah kategori tersimpan
        var prod1 = new Product
        {
            Name = "Smartphone",
            ProductCategoryId = electronics!.Id,
            Category = electronics,
            Image = "images/smartphone.jpg",
            UserId = adminUser.Id,
        };
        var prod2 = new Product
        {
            Name = "Laptop",
            ProductCategoryId = electronics.Id,
            Category = electronics,
            Image = "images/laptop.jpg",
            UserId = adminUser.Id
        };
        var prod3 = new Product
        {
            Name = "ASP.NET Core Book",
            ProductCategoryId = books!.Id,
            Category = books,
            Image = "images/book.jpg",
            UserId = adminUser.Id
        };

        db.Products.AddRange(prod1, prod2, prod3);
        await db.SaveChangesAsync();

        Console.WriteLine("Seeded ProductCategories and Products with correct relations.");
    }

}


// Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
