using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAppEF.Models;

public class AuthController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpGet]
    [Route("signup")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [Route("signup")]
    public async Task<IActionResult> Register(string email, string password, string fullName)
    {
        var user = new AppUser
        {
            UserName = email.Split('@')[0],
            Email = email,
            FullName = fullName
        };

        // Coba untuk membuat user baru
        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            // Jika berhasil, login user
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Product");
        }

        // Jika gagal, tampilkan error
        ViewBag.Error = string.Join(", ", result.Errors.Select(e => e.Description));
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Cari user berdasarkan username/email
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            ViewBag.Error = "Password atau Username salah";
            return View();
        }

        // Cek password dengan SignInManager
        var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return RedirectToAction("Index", "Product");
        }
        else
        {
            ViewBag.Error = "Password atau Username salah";
            return View();
        }
    }

    [HttpGet]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }
}
