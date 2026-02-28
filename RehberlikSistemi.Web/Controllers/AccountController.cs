using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RehberlikSistemi.Web.Core.Entities;
using System.Threading.Tasks;

namespace RehberlikSistemi.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToDashboard();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToDashboard();
                }
                ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        private IActionResult RedirectToDashboard()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }
            else if (User.IsInRole("Teacher"))
            {
                return RedirectToAction("Dashboard", "Teacher");
            }
            else if (User.IsInRole("Student"))
            {
                return RedirectToAction("Dashboard", "Student");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
