using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class UserController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return Redirect("/Identity/Account/Register");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return Redirect("/Identity/Account/Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}