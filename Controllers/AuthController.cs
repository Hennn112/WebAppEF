using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAppEF.Models;

namespace SendingEmails;

public class AuthController : Controller
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailSender _emailSender;

    public AuthController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        this._emailSender = emailSender;
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
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
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

    [HttpPost]
    [Route("verification")]
    public async Task<IActionResult> Verification(string email)
    {
        var subject = "Email Verification";
        var code = new Random().Next(1000, 9999).ToString();
        HttpContext.Session.SetString("VerificationCode", code);
        var message = $"Berikut ada kode verifikasi untuk akun Anda: {code}.";
        await _emailSender.SendEmailAsync(email, subject, message);
        return View();
    }

    [HttpPost]
    [Route("verify-code")]
    public async Task<IActionResult> VerifyCode(string code1, string code2, string code3, string code4)
    {
        var inputCode = $"{code1}{code2}{code3}{code4}";
        var savedCode = HttpContext.Session.GetString("VerificationCode");

        if (inputCode == savedCode)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "User tidak ditemukan.";
                return RedirectToAction("Verification");
            }

            user.Verified = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                TempData["Success"] = "Email berhasil diverifikasi.";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                TempData["Error"] = "Gagal memperbarui status verifikasi.";
                return RedirectToAction("Verification");
            }
        }

        TempData["Error"] = "Kode verifikasi salah. Silakan coba lagi.";
        return RedirectToAction("Verification");
    }
}
