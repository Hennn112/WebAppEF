using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAppEF.Models;

namespace WebAppEF.Controllers;
public class ProfileController : Controller
{
    private readonly Data.AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IWebHostEnvironment _env;


    public ProfileController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, Data.AppDbContext context, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _env = env;
    }

    [HttpGet]
    [Route("profile")]
    public async Task<IActionResult> Index()
    {
        var UserId = _userManager.GetUserId(User);
        if (UserId == null)
        {
            return RedirectToAction("Login","Auth");
        }
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        return View("~/Views/Auth/Profile.cshtml",user);
    }

     [HttpPost]
    public async Task<IActionResult> Update(AppUser model, IFormFile? ImageFile)
    {
        var user = await _userManager.FindByIdAsync(model.Id.ToString());
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;

        if (!string.IsNullOrWhiteSpace(model.PasswordHash))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.PasswordHash);
            if (!result.Succeeded)
            {
                ViewBag.Error = "Gagal mengganti password";
                return View("~/Views/Auth/Profile.cshtml", model);
            }
        }

        if (ImageFile != null && ImageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}_{ImageFile.FileName}";
            var path = Path.Combine(_env.WebRootPath, "Images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await ImageFile.CopyToAsync(stream);
            }

            user.ProfilePicturePath = $"Images/{fileName}";
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            ViewBag.Error = "Gagal memperbarui profil";
            return View("~/Views/Auth/Profile.cshtml", model);
        }

        ViewBag.Message = "Profil berhasil diperbarui";
        return View("~/Views/Auth/Profile.cshtml", user);
    }

}