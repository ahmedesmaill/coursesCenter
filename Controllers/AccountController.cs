using coursesCenter.Models;
using coursesCenter.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Diagnostics.Eventing.Reader;
using Microsoft.IdentityModel.Tokens;
using coursesCenter.Utility;

namespace coursesCenter.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole<int>> roleManager;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole<int>> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
        }
        public async Task<IActionResult> RegisterAsync()
        {
            if (roleManager.Roles.IsNullOrEmpty())
            {
                await roleManager.CreateAsync(new(SD.adminRole));
                await roleManager.CreateAsync(new(SD.companyRole));
                await roleManager.CreateAsync(new(SD.CustomerRole));
            }


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(ApplicationUserVM userVm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser applicationUser = new ApplicationUser()
                {
                    UserName = userVm.Name,
                    Email = userVm.Email,

                };

                var result = await userManager.CreateAsync(applicationUser, userVm.Password);
                if (result.Succeeded)
                {
                    // Ok
                    // Assign role to user


                    await userManager.AddToRoleAsync(applicationUser, SD.CustomerRole);
                    await signInManager.SignInAsync(applicationUser, false);


                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("Password", "Invalid Password");
            }

            return View(userVm);
        }

        [HttpGet]
        public ActionResult LogIn()
        {
            return View("LogIn");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogIn(logInViewModel logInUser)
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                ApplicationUser user = await userManager.FindByEmailAsync(logInUser.EmailAddress);
                if (user != null)
                {
                    // Check if the password is correct
                    bool found = await userManager.CheckPasswordAsync(user, logInUser.Password);
                    if (found)
                    {
                        var roles = await userManager.GetRolesAsync(user);
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
                        };
                        foreach (var role in roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        // Create a ClaimsIdentity
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        // Create a ClaimsPrincipal
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                        // Sign in the user with the claimsPrincipal which includes the user ID in the cookie
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal, new AuthenticationProperties
                        {
                            IsPersistent = logInUser.RememberMe
                        });

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            // If login failed, show error message
            ModelState.AddModelError("", "Email or password is not valid");
            return View("LogIn", logInUser);
        }

        [HttpGet]
        public async Task<ActionResult> LogOut() 
        { 
            await signInManager.SignOutAsync();
            return RedirectToAction("LogIn","Account");
        }
    }
}
