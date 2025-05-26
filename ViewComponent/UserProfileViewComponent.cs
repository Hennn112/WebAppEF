using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebAppEF.Models; // ganti sesuai namespace kamu

public class UserProfileViewComponent : ViewComponent
{
    private readonly UserManager<AppUser> _userManager;

    public UserProfileViewComponent(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        return View(user);
    }
}
