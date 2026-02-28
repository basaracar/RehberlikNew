using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Student;
using System.Linq;
using System.Threading.Tasks;

namespace RehberlikSistemi.Web.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var studentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == user.Id);
            if (studentProfile == null) return NotFound();

            // Calculate current week's start (Monday) and end (Sunday)
            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-1 * diff).Date;
            var endOfWeek = startOfWeek.AddDays(6).Date.AddDays(1).AddTicks(-1);

            // Fetch tasks for this week
            var weekTasks = await _context.StudyTasks
                .Include(t => t.Subject)
                .Where(t => t.StudentId == studentProfile.Id && t.ScheduledDate >= startOfWeek && t.ScheduledDate <= endOfWeek)
                .OrderBy(t => t.ScheduledDate).ThenBy(t => t.StartTime)
                .ToListAsync();

            var model = new StudentDashboardViewModel
            {
                StudentName = user.FirstName ?? user.Email ?? "Öğrenci",
                WeekStartDate = startOfWeek,
                WeekEndDate = endOfWeek,
                CurrentWeekTasks = weekTasks,
                TotalTasksCount = weekTasks.Count,
                CompletedTasksCount = weekTasks.Count(t => t.Status == Core.Enums.StudyTaskStatus.Completed)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MarkTaskComplete(int taskId, [FromQuery] bool ajax = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var task = await _context.StudyTasks
                .Include(t => t.Student)
                .FirstOrDefaultAsync(t => t.Id == taskId && t.Student.UserId == user.Id);

            if (task != null)
            {
                task.Status = Core.Enums.StudyTaskStatus.Completed;
                await _context.SaveChangesAsync();
                
                if (ajax)
                    return Json(new { success = true });
            }

            if (ajax)
                return Json(new { success = false, message = "Görev bulunamadı veya yetkiniz yok." });

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> GetMyWeeklySchedule([FromQuery] string date)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var studentProfile = await _context.StudentProfiles
                .Include(sp => sp.Availabilities)
                .Include(sp => sp.Exams).ThenInclude(e => e.Subject)
                .Include(sp => sp.StudyTasks).ThenInclude(t => t.Subject)
                .FirstOrDefaultAsync(sp => sp.UserId == user.Id);

            if (studentProfile == null) return NotFound();

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
            {
                targetDate = DateTime.Today;
            }

            int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = targetDate.AddDays(-1 * diff).Date;
            var endOfWeek = startOfWeek.AddDays(6).Date.AddTicks(TimeSpan.TicksPerDay - 1);

            var tasks = await _context.StudyTasks
                .Include(t => t.Subject)
                .Where(t => t.StudentId == studentProfile.Id && t.ScheduledDate >= startOfWeek && t.ScheduledDate <= endOfWeek)
                .Select(t => new {
                    id = t.Id,
                    subjectName = t.Subject.Name,
                    date = t.ScheduledDate.ToString("yyyy-MM-dd"),
                    startTime = t.StartTime.ToString(@"hh\:mm"),
                    endTime = t.EndTime.ToString(@"hh\:mm"),
                    status = t.Status.ToString() // 0 Pending, 1 Completed filan gelir
                })
                .ToListAsync();
                
            var exams = await _context.Exams
                .Include(e => e.Subject)
                .Where(e => e.StudentId == studentProfile.Id && e.ExamDate >= startOfWeek && e.ExamDate <= endOfWeek)
                .Select(e => new {
                    id = e.Id,
                    subjectName = e.Subject.Name,
                    date = e.ExamDate.ToString("yyyy-MM-dd"),
                    time = e.ExamDate.ToString("HH:mm")
                })
                .ToListAsync();

            var avails = studentProfile.Availabilities.Select(a => new {
                dayOfWeek = (int)a.DayOfWeek,
                isAvailable = a.IsAvailable,
                startTime = a.StartTime.ToString(@"hh\:mm"),
                endTime = a.EndTime.ToString(@"hh\:mm")
            });

            return Json(new {
                weekStartDate = startOfWeek.ToString("yyyy-MM-dd"),
                weekEndDate = endOfWeek.ToString("yyyy-MM-dd"),
                tasks = tasks,
                exams = exams,
                availabilities = avails
            });
        }

        [HttpGet]
        public async Task<IActionResult> Availability()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var studentProfile = await _context.StudentProfiles
                .Include(sp => sp.Availabilities)
                .FirstOrDefaultAsync(sp => sp.UserId == user.Id);

            if (studentProfile == null) return NotFound();

            var model = new StudentAvailabilityViewModel
            {
                Availabilities = studentProfile.Availabilities.OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddAvailability(StudentAvailabilityViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var studentProfile = await _context.StudentProfiles.FirstOrDefaultAsync(sp => sp.UserId == user.Id);
            if (studentProfile == null) return NotFound();

            var availability = new Availability
            {
                StudentId = studentProfile.Id,
                DayOfWeek = model.NewDayOfWeek,
                StartTime = model.NewStartTime,
                EndTime = model.NewEndTime,
                IsAvailable = true
            };

            _context.Availabilities.Add(availability);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Availability));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var availability = await _context.Availabilities
                .Include(a => a.Student)
                .FirstOrDefaultAsync(a => a.Id == id && a.Student.UserId == user.Id);

            if (availability != null)
            {
                _context.Availabilities.Remove(availability);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Availability));
        }
    }
}
