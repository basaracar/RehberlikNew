using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Teacher;
using System.Linq;
using System.Threading.Tasks;

namespace RehberlikSistemi.Web.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var students = await _context.StudentProfiles
                .Include(sp => sp.User)
                .Include(sp => sp.StudyTasks)
                .Where(sp => sp.TeacherId == user.Id)
                .ToListAsync();

            var model = new TeacherDashboardViewModel
            {
                TeacherFullName = $"{user.FirstName} {user.LastName}",
                TotalStudents = students.Count,
                RecentStudents = students.Select(sp => {
                    var totalTasks = sp.StudyTasks.Count;
                    var completedTasks = sp.StudyTasks.Count(t => t.Status == Core.Enums.StudyTaskStatus.Completed);
                    var rate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;
                    
                    string status = "İyi";
                    string statusClass = "success";
                    
                    if (rate < 40) { status = "Riskli"; statusClass = "danger"; }
                    else if (rate < 70) { status = "Yavaş"; statusClass = "warning"; }

                    return new RecentStudentProgressViewModel {
                        ProfileId = sp.Id,
                        FullName = $"{sp.User.FirstName} {sp.User.LastName}",
                        GradeLevel = sp.GradeLevel,
                        ProfileImageUrl = sp.User.ProfileImageUrl,
                        CompletionRate = rate,
                        Status = status,
                        StatusClass = statusClass
                    };
                }).OrderByDescending(s => s.CompletionRate).Take(5).ToList()
            };

            // Aggregated Stats
            if (students.Any())
            {
                model.AverageCompletionRate = model.RecentStudents.Any() ? model.RecentStudents.Average(s => s.CompletionRate) : 0;
                model.AtRiskStudentsCount = model.RecentStudents.Count(s => s.Status == "Riskli");
            }

            // Monthly Goals (simplification: total weekly targets for current month)
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            model.MonthlyGoalCount = await _context.WeeklyTargets
                .CountAsync(wt => wt.Student.TeacherId == user.Id && wt.WeekStartDate >= startOfMonth);

            // Recent Activities (simulated from recent tasks)
            var recentTasks = await _context.StudyTasks
                .Include(t => t.Subject)
                .Include(t => t.Student).ThenInclude(s => s.User)
                .Where(t => t.Student.TeacherId == user.Id)
                .OrderByDescending(t => t.Id)
                .Take(4)
                .ToListAsync();

            model.RecentActivities = recentTasks.Select(t => new RecentActivityViewModel {
                Title = t.Status == Core.Enums.StudyTaskStatus.Completed ? "Çalışma Tamamlandı" : "Sınav Planlandı",
                Description = $"{t.Student.User.FirstName}, {t.Subject.Name} dersini {(t.Status == Core.Enums.StudyTaskStatus.Completed ? "bitirdi" : "çalışacak")}.",
                TimeAgo = "Bugün", // Simplified
                IconClass = t.Status == Core.Enums.StudyTaskStatus.Completed ? "bi-check-circle" : "bi-calendar-event",
                ColorClass = t.Status == Core.Enums.StudyTaskStatus.Completed ? "success" : "primary"
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> Students()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var students = await _context.StudentProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.TeacherId == user.Id)
                .Select(sp => new TeacherStudentViewModel
                {
                    ProfileId = sp.Id,
                    FullName = sp.User.FirstName + " " + sp.User.LastName,
                    GradeLevel = sp.GradeLevel,
                    TargetUniversity = sp.TargetUniversity
                }).ToListAsync();

            return View(students);
        }

        public async Task<IActionResult> UnassignedStudents()
        {
            var students = await _context.StudentProfiles
                .Include(sp => sp.User)
                .Where(sp => string.IsNullOrEmpty(sp.TeacherId))
                .Select(sp => new TeacherStudentViewModel
                {
                    ProfileId = sp.Id,
                    FullName = sp.User.FirstName + " " + sp.User.LastName,
                    GradeLevel = sp.GradeLevel,
                    TargetUniversity = sp.TargetUniversity
                }).ToListAsync();

            return View(students);
        }

        [HttpPost]
        public async Task<IActionResult> AssignStudent(int profileId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var student = await _context.StudentProfiles.FindAsync(profileId);
            if (student == null) return NotFound();

            if (string.IsNullOrEmpty(student.TeacherId))
            {
                student.TeacherId = user.Id;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Students));
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id) // ProfileId
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var studentProfile = await _context.StudentProfiles
                .Include(sp => sp.User)
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.TeacherId == user.Id);

            if (studentProfile == null) return NotFound();

            var model = new TeacherEditStudentViewModel
            {
                ProfileId = studentProfile.Id,
                FirstName = studentProfile.User.FirstName,
                LastName = studentProfile.User.LastName,
                Email = studentProfile.User.Email ?? "",
                GradeLevel = studentProfile.GradeLevel,
                TargetUniversity = studentProfile.TargetUniversity
            };

            return View("EditStudent", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(TeacherEditStudentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var studentProfile = await _context.StudentProfiles
                    .Include(sp => sp.User)
                    .FirstOrDefaultAsync(sp => sp.Id == model.ProfileId && sp.TeacherId == user.Id);

                if (studentProfile == null) return NotFound();

                // Update fields
                studentProfile.User.FirstName = model.FirstName;
                studentProfile.User.LastName = model.LastName;
                studentProfile.User.Email = model.Email;
                studentProfile.GradeLevel = model.GradeLevel;
                studentProfile.TargetUniversity = model.TargetUniversity;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Students));
            }

            return View("EditStudent", model);
        }

        public async Task<IActionResult> StudentDetail(int id, string? msg = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var student = await _context.StudentProfiles
                .Include(sp => sp.User)
                .Include(sp => sp.Availabilities)
                .Include(sp => sp.Exams).ThenInclude(e => e.Subject)
                .Include(sp => sp.WeeklyTargets).ThenInclude(w => w.Subject)
                .Include(sp => sp.StudyTasks).ThenInclude(t => t.Subject)
                .FirstOrDefaultAsync(sp => sp.Id == id && sp.TeacherId == user.Id);

            if (student == null) return NotFound();

            var today = DateTime.Today;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = today.AddDays(-1 * diff).Date;
            var endOfWeek = startOfWeek.AddDays(6).Date.AddTicks(TimeSpan.TicksPerDay - 1);

            var model = new StudentDetailViewModel
            {
                ProfileId = student.Id,
                FullName = student.User.FirstName + " " + student.User.LastName,
                GradeLevel = student.GradeLevel,
                TargetUniversity = student.TargetUniversity,
                Exams = student.Exams.OrderBy(e => e.ExamDate).ToList(),
                WeeklyTargets = student.WeeklyTargets.OrderByDescending(w => w.WeekStartDate).ToList(),
                Availabilities = student.Availabilities.ToList(),
                WeekStartDate = startOfWeek,
                WeekEndDate = endOfWeek,
                RequestSuccessMessage = msg ?? string.Empty
            };

            // Calculate Automated Targets
            var currentWeekTasks = student.StudyTasks
                .Where(t => t.ScheduledDate >= startOfWeek && t.ScheduledDate <= endOfWeek)
                .ToList();

            model.AutomatedTargets = currentWeekTasks
                .GroupBy(t => t.SubjectId)
                .Select(g => new SubjectTargetViewModel
                {
                    SubjectName = g.First().Subject.Name,
                    PlannedHours = g.Sum(t => (t.EndTime - t.StartTime).TotalHours),
                    CompletedHours = g.Where(t => t.Status == Core.Enums.StudyTaskStatus.Completed)
                                      .Sum(t => (t.EndTime - t.StartTime).TotalHours)
                })
                .OrderByDescending(t => t.PlannedHours)
                .ToList();

            ViewBag.Subjects = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Subjects.ToListAsync(), "Id", "Name");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateExam(int profileId)
        {
            var student = await _context.StudentProfiles.FindAsync(profileId);
            if (student == null) return NotFound();
            
            ViewBag.Subjects = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Subjects.ToListAsync(), "Id", "Name");
            return View(new CreateExamViewModel { ProfileId = profileId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateExam(CreateExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exam = new Exam
                {
                    StudentId = model.ProfileId,
                    SubjectId = model.SubjectId,
                    ExamDate = model.ExamDate,
                    ImportanceLevel = model.ImportanceLevel
                };
                _context.Exams.Add(exam);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId });
            }
            ViewBag.Subjects = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", model.SubjectId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SetExamScore(int id, int profileId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var exam = await _context.Exams.Include(e => e.Subject)
                .FirstOrDefaultAsync(e => e.Id == id && e.StudentId == profileId && e.Student.TeacherId == user.Id);
            
            if (exam == null) return NotFound();

            var model = new SetExamScoreViewModel
            {
                ExamId = exam.Id,
                ProfileId = profileId,
                SubjectName = exam.Subject.Name,
                Score = exam.Score
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetExamScore(SetExamScoreViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var exam = await _context.Exams.Include(e => e.Student)
                    .FirstOrDefaultAsync(e => e.Id == model.ExamId && e.StudentId == model.ProfileId && e.Student.TeacherId == user.Id);
                
                if (exam != null)
                {
                    exam.Score = model.Score;
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CreateWeeklyTarget(int profileId)
        {
            var student = await _context.StudentProfiles.FindAsync(profileId);
            if (student == null) return NotFound();
            
            ViewBag.Subjects = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Subjects.ToListAsync(), "Id", "Name");
            return View(new CreateWeeklyTargetViewModel { ProfileId = profileId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateWeeklyTarget(CreateWeeklyTargetViewModel model)
        {
            if (ModelState.IsValid)
            {
                var target = new WeeklyTarget
                {
                    StudentId = model.ProfileId,
                    SubjectId = model.SubjectId,
                    TargetHours = model.TargetHours,
                    WeekStartDate = model.WeekStartDate
                };
                _context.WeeklyTargets.Add(target);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId });
            }
            ViewBag.Subjects = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(await _context.Subjects.ToListAsync(), "Id", "Name", model.SubjectId);
            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> GetWeeklySchedule(int profileId, [FromQuery] string date)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var student = await _context.StudentProfiles
                .Include(sp => sp.Availabilities)
                .FirstOrDefaultAsync(sp => sp.Id == profileId && sp.TeacherId == user.Id);
                
            if (student == null) return NotFound();

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime targetDate))
            {
                targetDate = DateTime.Today;
            }

            int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = targetDate.AddDays(-1 * diff).Date;
            var endOfWeek = startOfWeek.AddDays(6).Date.AddTicks(TimeSpan.TicksPerDay - 1);

            var tasks = await _context.StudyTasks
                .Include(t => t.Subject)
                .Where(t => t.StudentId == profileId && t.ScheduledDate >= startOfWeek && t.ScheduledDate <= endOfWeek)
                .Select(t => new {
                    id = t.Id,
                    subjectName = t.Subject.Name,
                    date = t.ScheduledDate.ToString("yyyy-MM-dd"),
                    startTime = t.StartTime.ToString(@"hh\:mm"),
                    endTime = t.EndTime.ToString(@"hh\:mm"),
                    status = t.Status.ToString()
                })
                .ToListAsync();
                
            var exams = await _context.Exams
                .Include(e => e.Subject)
                .Where(e => e.StudentId == profileId && e.ExamDate >= startOfWeek && e.ExamDate <= endOfWeek)
                .Select(e => new {
                    id = e.Id,
                    subjectName = e.Subject.Name,
                    date = e.ExamDate.ToString("yyyy-MM-dd"),
                    time = e.ExamDate.ToString("HH:mm")
                })
                .ToListAsync();

            var avails = student.Availabilities.Select(a => new {
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



        [HttpPost]
        public async Task<IActionResult> DeleteExam(int id, int profileId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var exam = await _context.Exams.Include(e => e.Student).FirstOrDefaultAsync(e => e.Id == id && e.StudentId == profileId && e.Student.TeacherId == user.Id);
            if (exam != null)
            {
                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(StudentDetail), new { id = profileId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWeeklyTarget(int id, int profileId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var target = await _context.WeeklyTargets.Include(w => w.Student).FirstOrDefaultAsync(w => w.Id == id && w.StudentId == profileId && w.Student.TeacherId == user.Id);
            if (target != null)
            {
                _context.WeeklyTargets.Remove(target);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(StudentDetail), new { id = profileId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudyTask(StudentDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound();

                var student = await _context.StudentProfiles
                    .Include(sp => sp.Availabilities)
                    .FirstOrDefaultAsync(sp => sp.Id == model.ProfileId && sp.TeacherId == user.Id);

                if (student == null) return NotFound();

                if (model.StartTime >= model.EndTime)
                {
                    ModelState.AddModelError("", "Bitiş saati başlangıç saatinden büyük olmalıdır.");
                    return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId, msg = "Hata: Bitiş saati başlangıç saatinden büyük olmalıdır." });
                }

                var reqDay = model.ScheduledDate.DayOfWeek;
                bool isAvailable = student.Availabilities.Any(a => 
                    a.DayOfWeek == reqDay && 
                    a.IsAvailable && 
                    model.StartTime >= a.StartTime && 
                    model.EndTime <= a.EndTime);

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Öğrenci bu saat aralığında müsait değil.");
                    return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId, msg = "Hata: Öğrenci seçilen saat aralığında müsait değil." });
                }
                
                var hasCollision = await _context.StudyTasks.AnyAsync(t => 
                    t.StudentId == model.ProfileId &&
                    t.ScheduledDate.Date == model.ScheduledDate.Date &&
                    ((model.StartTime >= t.StartTime && model.StartTime < t.EndTime) ||
                     (model.EndTime > t.StartTime && model.EndTime <= t.EndTime) ||
                     (model.StartTime <= t.StartTime && model.EndTime >= t.EndTime))
                );

                if (hasCollision)
                {
                    return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId, msg = "Hata: Bu saatler arasında başka bir görev planlanmış durumda (Çakışma)." });
                }

                var task = new StudyTask
                {
                    StudentId = model.ProfileId,
                    SubjectId = model.SubjectId,
                    ScheduledDate = model.ScheduledDate,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Status = Core.Enums.StudyTaskStatus.Pending
                };

                _context.StudyTasks.Add(task);
                await _context.SaveChangesAsync();
                
                var subject = await _context.Subjects.FindAsync(model.SubjectId);
                string subjectName = subject?.Name ?? "Ders";

                return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId, msg = $"{subjectName} dersi başarıyla planlandı." });
            }

            return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId, msg = "Hata: Form doğrulanamadı." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudyTask(int id, int profileId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var task = await _context.StudyTasks.Include(t => t.Student).FirstOrDefaultAsync(t => t.Id == id && t.StudentId == profileId && t.Student.TeacherId == user.Id);
            
            if (task != null)
            {
                if (task.Status != Core.Enums.StudyTaskStatus.Pending || task.ScheduledDate.Date <= DateTime.Today)
                {
                    return RedirectToAction(nameof(StudentDetail), new { id = profileId, msg = "Hata: Sadece onaylanmamış ve gelecek tarihteki dersleri silebilirsiniz." });
                }

                _context.StudyTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(StudentDetail), new { id = profileId });
        }
        
        [HttpPost]
        public async Task<IActionResult> ClearDayTasks(DateTime date, int profileId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var tasks = await _context.StudyTasks
                .Include(t => t.Student)
                .Where(t => t.StudentId == profileId && 
                           t.Student.TeacherId == user.Id && 
                           t.ScheduledDate.Date == date.Date &&
                           t.Status == Core.Enums.StudyTaskStatus.Pending &&
                           t.ScheduledDate.Date > DateTime.Today)
                .ToListAsync();

            if (tasks.Any())
            {
                _context.StudyTasks.RemoveRange(tasks);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(StudentDetail), new { id = profileId, msg = "Günlük program temizlendi." });
        }


        [HttpGet]
        public async Task<IActionResult> PlanGenerator(int profileId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var student = await _context.StudentProfiles
                .Include(sp => sp.User)
                .Include(sp => sp.Availabilities)
                .Include(sp => sp.Exams).ThenInclude(e => e.Subject)
                .FirstOrDefaultAsync(sp => sp.Id == profileId && sp.TeacherId == user.Id);

            if (student == null) return NotFound();

            var model = new PlanGeneratorViewModel
            {
                ProfileId = profileId,
                StudentFullName = student.User.FirstName + " " + student.User.LastName
            };

            var currentDate = System.DateTime.Now;
            var next7Days = currentDate.Date.AddDays(7);
            
            // Step: Check if any plan already exists for the next week
            var existingTasks = await _context.StudyTasks
                .AnyAsync(t => t.StudentId == profileId && t.ScheduledDate >= currentDate.Date && t.ScheduledDate <= next7Days);

            if (existingTasks)
            {
                model.IsSuccessful = false;
                model.Message = "Öğrencinin önümüzdeki hafta için zaten planlanmış dersleri mevcut. Yeni bir otomatik plan oluşturmak için önce mevcut dersleri temizlemelisiniz.";
                return View(model);
            }

            if (!student.Availabilities.Any())
            {
                model.IsSuccessful = false;
                model.Message = "Öğrencinin müsaitlik (Availability) bilgisi bulunamadığı için plan oluşturulamıyor.";
                return View(model);
            }

            var allSubjects = await _context.Subjects.ToListAsync();
            var subjectPriorties = new Dictionary<int, int>();

            // Initialize all with base priority 1
            foreach (var subject in allSubjects)
                subjectPriorties[subject.Id] = 1;

            // Algorithm step: Add points based on exams
            foreach (var exam in student.Exams)
            {
                if (exam.ExamDate >= currentDate)
                {
                    // Upcoming exams
                    var daysUntil = (exam.ExamDate - currentDate).TotalDays;
                    int addedPriority = exam.ImportanceLevel; // Add their importance level
                    
                    if (daysUntil <= 7) addedPriority += 5; // Urgent
                    else if (daysUntil <= 30) addedPriority += 3; // Soon

                    subjectPriorties[exam.SubjectId] += addedPriority;
                }
                else
                {
                    // Past exams
                    if (exam.Score.HasValue)
                    {
                        if (exam.Score.Value < 50) subjectPriorties[exam.SubjectId] += 4; // Needs much work
                        else if (exam.Score.Value < 70) subjectPriorties[exam.SubjectId] += 2; // Needs some work
                    }
                }
            }

            // Create a queue weighted by priority
            var weightedSubjectsQueue = new Queue<Subject>();
            var sortedPriorities = subjectPriorties.OrderByDescending(x => x.Value).ToList();
            
            // Fill a pool where subjects appear 'Priority' times
            var subjectPool = new List<Subject>();
            foreach (var sp in sortedPriorities)
            {
                var subj = allSubjects.First(s => s.Id == sp.Key);
                for (int i = 0; i < sp.Value; i++)
                {
                    subjectPool.Add(subj);
                }
            }

            // Shuffle or just cycle through the pool sequentially (which groups high priority ones first)
            int poolIndex = 0;

            // Generate tasks for the next 7 days
            var proposedTasks = new List<StudyTask>();
            
            for (int i = 1; i <= 7; i++)
            {
                var targetDay = currentDate.Date.AddDays(i);
                var dayOfWeek = targetDay.DayOfWeek;

                var availabilities = student.Availabilities.Where(a => a.DayOfWeek == dayOfWeek && a.IsAvailable).ToList();
                
                foreach (var avail in availabilities)
                {
                    // Create task chunks of 1 hour within the availability block
                    var currentStart = avail.StartTime;
                    while (currentStart.Add(System.TimeSpan.FromHours(1)) <= avail.EndTime)
                    {
                        var taskSubject = subjectPool[poolIndex % subjectPool.Count];
                        poolIndex++;

                        proposedTasks.Add(new StudyTask
                        {
                            StudentId = profileId,
                            SubjectId = taskSubject.Id,
                            Subject = taskSubject,
                            ScheduledDate = targetDay,
                            StartTime = currentStart,
                            EndTime = currentStart.Add(System.TimeSpan.FromHours(1)),
                            Status = Core.Enums.StudyTaskStatus.Pending
                        });

                        currentStart = currentStart.Add(System.TimeSpan.FromHours(1));
                    }
                }
            }

            model.ProposedTasks = proposedTasks;
            
            // Serialize for POSTing later
            var plainTasks = proposedTasks.Select(t => new { t.SubjectId, t.ScheduledDate, t.StartTime, t.EndTime }).ToList();
            model.SerializedTasks = System.Text.Json.JsonSerializer.Serialize(plainTasks);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveGeneratedPlan(PlanGeneratorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var student = await _context.StudentProfiles.FirstOrDefaultAsync(sp => sp.Id == model.ProfileId && sp.TeacherId == user.Id);
            if (student == null) return NotFound();

            // Safety Check: Re-verify if any tasks were added in the meantime
            var currentDate = System.DateTime.Now.Date;
            var next7Days = currentDate.AddDays(7);
            var alreadyHasTasks = await _context.StudyTasks
                .AnyAsync(t => t.StudentId == model.ProfileId && t.ScheduledDate >= currentDate && t.ScheduledDate <= next7Days);
            
            if (alreadyHasTasks)
            {
                return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId, msg = "Hata: Bu haftanın planı zaten mevcut. Kaydedilemedi." });
            }

            if (!string.IsNullOrEmpty(model.SerializedTasks))
            {
                // We use a small anonymous DTO to deserialize what we passed from the preview
                var plainTasks = System.Text.Json.JsonSerializer.Deserialize<List<StudyTaskDto>>(model.SerializedTasks);
                if (plainTasks != null)
                {
                    foreach (var pt in plainTasks)
                    {
                        _context.StudyTasks.Add(new StudyTask
                        {
                            StudentId = model.ProfileId,
                            SubjectId = pt.SubjectId,
                            ScheduledDate = pt.ScheduledDate,
                            StartTime = pt.StartTime,
                            EndTime = pt.EndTime,
                            Status = Core.Enums.StudyTaskStatus.Pending
                        });
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(StudentDetail), new { id = model.ProfileId });
        }
    }
    
    public class StudyTaskDto
    {
        public int SubjectId { get; set; }
        public System.DateTime ScheduledDate { get; set; }
        public System.TimeSpan StartTime { get; set; }
        public System.TimeSpan EndTime { get; set; }
    }
}
