using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TeachSpark.Web.Models;

namespace TeachSpark.Web.Controllers;

/// <summary>
/// Controller for user account and profile management
/// </summary>
[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Display user dashboard - requires authentication
    /// </summary>
    /// <returns>Dashboard view</returns>
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        ViewData["Title"] = "Dashboard";
        return View(user);
    }

    /// <summary>
    /// Display user profile information
    /// </summary>
    /// <returns>Profile view</returns>
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        ViewData["Title"] = "My Profile";
        return View(user);
    }
}
