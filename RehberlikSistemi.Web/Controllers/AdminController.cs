using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Admin;
using System.Linq;
using System.Threading.Tasks;

namespace RehberlikSistemi.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        #region Teachers
        public async Task<IActionResult> Teachers()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            return View(teachers);
        }

        [HttpGet]
        public IActionResult CreateTeacher()
        {
            return View(new TeacherViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeacher(TeacherViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password ?? "Ogretmen123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Teacher");
                    return RedirectToAction(nameof(Teachers));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        #endregion

        #region Students
        public async Task<IActionResult> Students()
        {
            var students = await _context.StudentProfiles
                .Include(sp => sp.User)
                .Include(sp => sp.Teacher)
                .Select(sp => new StudentViewModel
                {
                    Id = sp.UserId,
                    ProfileId = sp.Id,
                    FirstName = sp.User.FirstName,
                    LastName = sp.User.LastName,
                    Email = sp.User.Email ?? "",
                    GradeLevel = sp.GradeLevel,
                    TargetUniversity = sp.TargetUniversity,
                    TeacherName = sp.Teacher != null ? sp.Teacher.FirstName + " " + sp.Teacher.LastName : "Atanmadı"
                }).ToListAsync();

            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStudent()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FirstName");
            return View(new StudentViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent(StudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password ?? "Ogrenci123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Student");

                    var studentProfile = new StudentProfile
                    {
                        UserId = user.Id,
                        TeacherId = model.TeacherId,
                        GradeLevel = model.GradeLevel,
                        TargetUniversity = model.TargetUniversity
                    };

                    _context.StudentProfiles.Add(studentProfile);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Students));
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FirstName", model.TeacherId);
            return View(model);
        }
        
        [HttpGet]
        public async Task<IActionResult> EditStudent(int id) // ProfileId
        {
            var profile = await _context.StudentProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null) return NotFound();

            var model = new StudentViewModel
            {
                Id = profile.UserId,
                ProfileId = profile.Id,
                FirstName = profile.User.FirstName,
                LastName = profile.User.LastName,
                Email = profile.User.Email ?? "",
                GradeLevel = profile.GradeLevel,
                TargetUniversity = profile.TargetUniversity,
                TeacherId = profile.TeacherId
            };

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FirstName", profile.TeacherId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(StudentViewModel model)
        {
            var profile = await _context.StudentProfiles.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == model.ProfileId);
            if (profile == null) return NotFound();

            profile.User.FirstName = model.FirstName;
            profile.User.LastName = model.LastName;
            profile.User.Email = model.Email;
            
            profile.GradeLevel = model.GradeLevel;
            profile.TargetUniversity = model.TargetUniversity;
            profile.TeacherId = model.TeacherId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Students));
        }
        #endregion

        #region Subjects
        public async Task<IActionResult> Subjects()
        {
            var subjects = await _context.Subjects.ToListAsync();
            return View(subjects);
        }

        [HttpGet]
        public IActionResult CreateSubject()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubject(Subject subject)
        {
            if (ModelState.IsValid)
            {
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Subjects));
            }
            return View(subject);
        }
        #endregion
    }
}
