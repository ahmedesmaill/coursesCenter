using coursesCenter.Models;
using coursesCenter.Models.entities;
using coursesCenter.Repository;
using coursesCenter.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace coursesCenter.Controllers
{
    [Authorize(Roles = "Manager")]
    public class TraineeAccountController : Controller
    {
        private readonly IDepartmentRepositorty DepartmentRepo;
        private readonly ITraineRepository TraineeRepo;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole<int>> roleManager;
        public TraineeAccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole<int>> roleManager, IDepartmentRepositorty DepartmentRepo, ITraineRepository TraineeRepo)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.DepartmentRepo= DepartmentRepo;
            this.TraineeRepo= TraineeRepo;
        }
        [HttpGet]
        public IActionResult RegisterTrainee()
        {
            ViewBag.department = DepartmentRepo.GetAll();
            return View("RegisterTrainee");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterTrainee(RegisterTraineViewModel TraineeRegiste)
        {
            if(ModelState.IsValid)
            {
                if (TraineeRegiste.DepartmentId == 0)
                {
                    ModelState.AddModelError("", "you should choose department");
                }
                else
                {
                    ApplicationUser user = new ApplicationUser();
                    user.Email = TraineeRegiste.EmailAddress;
                    user.UserName = TraineeRegiste.Name;
                    IdentityResult result = await userManager.CreateAsync(user, TraineeRegiste.Password);
                    if (result.Succeeded)
                    {
                        Traine trainee = new Traine();
                        trainee.Name = TraineeRegiste.Name;
                        trainee.Address = TraineeRegiste.Address;
                        trainee.Level = TraineeRegiste.Level;
                        trainee.DepartmentId = TraineeRegiste.DepartmentId;
                        trainee.ApplicationUserId = user.Id;
                        TraineeRepo.Save(trainee);
                        await userManager.AddToRoleAsync(user, "Trainee");
                        await signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        foreach (var item in result.Errors)
                        {
                            ModelState.AddModelError("", item.Description);
                        }
                    }
                }
            }
            ViewBag.department = DepartmentRepo.GetAll();
            return View("RegisterTrainee", TraineeRegiste);
        }
    }
}
