using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
using RehberlikSistemi.Web.Core.Constants;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Data;

namespace RehberlikSistemi.Web.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Rollere Ekleme
            string[] roles = { AppRoles.Admin, AppRoles.Teacher, AppRoles.Student };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Örnek Admin Ekleme
            var adminUser = await userManager.FindByEmailAsync("admin@rehberlik.com");
            if (adminUser == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = "admin@rehberlik.com",
                    Email = "admin@rehberlik.com",
                    FirstName = "Sistem",
                    LastName = "Yöneticisi",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(newAdmin, "Admin123!");
                await userManager.AddToRoleAsync(newAdmin, AppRoles.Admin);
            }

            // 3. Örnek Öğretmenler (3 adet)
            var teacherData = new[]
            {
                new { Email = "ayse.yilmaz@rehberlik.com", FirstName = "Ayşe", LastName = "Yılmaz" },
                new { Email = "mehmet.kaya@rehberlik.com", FirstName = "Mehmet", LastName = "Kaya" },
                new { Email = "fatma.demir@rehberlik.com", FirstName = "Fatma", LastName = "Demir" }
            };

            ApplicationUser? firstTeacher = null;
            foreach (var t in teacherData)
            {
                var existing = await userManager.FindByEmailAsync(t.Email);
                if (existing == null)
                {
                    var newTeacher = new ApplicationUser
                    {
                        UserName = t.Email,
                        Email = t.Email,
                        FirstName = t.FirstName,
                        LastName = t.LastName,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newTeacher, "Teacher123!");
                    await userManager.AddToRoleAsync(newTeacher, AppRoles.Teacher);
                    firstTeacher ??= newTeacher;
                }
                else
                {
                    firstTeacher ??= existing;
                }
            }

            // İkinci ve üçüncü öğretmenleri bul
            var secondTeacher = await userManager.FindByEmailAsync("mehmet.kaya@rehberlik.com");
            var thirdTeacher = await userManager.FindByEmailAsync("fatma.demir@rehberlik.com");

            // 4. Örnek Öğrenciler (5 adet)
            var studentData = new[]
            {
                new { Email = "elif.ozturk@rehberlik.com", FirstName = "Elif", LastName = "Öztürk", Grade = "12", Target = "Boğaziçi Üniversitesi / Bilgisayar Mühendisliği", TeacherId = firstTeacher?.Id },
                new { Email = "cem.arslan@rehberlik.com", FirstName = "Cem", LastName = "Arslan", Grade = "11", Target = "İTÜ / Elektrik-Elektronik Mühendisliği", TeacherId = firstTeacher?.Id },
                new { Email = "zeynep.celik@rehberlik.com", FirstName = "Zeynep", LastName = "Çelik", Grade = "12", Target = "Hacettepe Üniversitesi / Tıp Fakültesi", TeacherId = secondTeacher?.Id },
                new { Email = "burak.sahin@rehberlik.com", FirstName = "Burak", LastName = "Şahin", Grade = "10", Target = "ODTÜ / Makine Mühendisliği", TeacherId = secondTeacher?.Id },
                new { Email = "selin.korkmaz@rehberlik.com", FirstName = "Selin", LastName = "Korkmaz", Grade = "11", Target = "Ankara Üniversitesi / Hukuk Fakültesi", TeacherId = thirdTeacher?.Id }
            };

            StudentProfile? firstStudentProfile = null;
            foreach (var s in studentData)
            {
                var existing = await userManager.FindByEmailAsync(s.Email);
                if (existing == null)
                {
                    var newStudent = new ApplicationUser
                    {
                        UserName = s.Email,
                        Email = s.Email,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(newStudent, "Student123!");
                    await userManager.AddToRoleAsync(newStudent, AppRoles.Student);
                    existing = newStudent;
                }

                var profile = context.StudentProfiles.FirstOrDefault(sp => sp.UserId == existing.Id);
                if (profile == null)
                {
                    profile = new StudentProfile
                    {
                        UserId = existing.Id,
                        TeacherId = s.TeacherId,
                        GradeLevel = s.Grade,
                        TargetUniversity = s.Target
                    };
                    context.StudentProfiles.Add(profile);
                    await context.SaveChangesAsync();
                }
                firstStudentProfile ??= profile;
            }

            // 5. Derslerin Eklenmesi (Subjects)
            if (!context.Subjects.Any())
            {
                context.Subjects.AddRange(
                    new Subject { Name = "Matematik", SubjectType = Core.Enums.SubjectType.Sayisal },
                    new Subject { Name = "Fizik", SubjectType = Core.Enums.SubjectType.Sayisal },
                    new Subject { Name = "Kimya", SubjectType = Core.Enums.SubjectType.Sayisal },
                    new Subject { Name = "Biyoloji", SubjectType = Core.Enums.SubjectType.Sayisal },
                    new Subject { Name = "Türkçe", SubjectType = Core.Enums.SubjectType.Sozel },
                    new Subject { Name = "Tarih", SubjectType = Core.Enums.SubjectType.Sozel },
                    new Subject { Name = "Coğrafya", SubjectType = Core.Enums.SubjectType.Sozel }
                );
                await context.SaveChangesAsync();
            }

            // 6. Örnek Verilerin (Availability, Exam, vs.) Eklenmesi — İlk öğrenci üzerinden
            var mathSubject = context.Subjects.FirstOrDefault(s => s.Name == "Matematik");
            var physicsSubject = context.Subjects.FirstOrDefault(s => s.Name == "Fizik");

            if (firstStudentProfile != null && !context.Availabilities.Any(a => a.StudentId == firstStudentProfile.Id))
            {
                context.Availabilities.AddRange(
                    new Availability { StudentId = firstStudentProfile.Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(22, 0, 0) },
                    new Availability { StudentId = firstStudentProfile.Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(23, 0, 0) },
                    new Availability { StudentId = firstStudentProfile.Id, DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(15, 0, 0) }
                );
                await context.SaveChangesAsync();
            }

            if (firstStudentProfile != null && mathSubject != null && physicsSubject != null && !context.Exams.Any(e => e.StudentId == firstStudentProfile.Id))
            {
                context.Exams.AddRange(
                    new Exam { StudentId = firstStudentProfile.Id, SubjectId = mathSubject.Id, ExamDate = DateTime.Now.AddDays(15), ImportanceLevel = 5 },
                    new Exam { StudentId = firstStudentProfile.Id, SubjectId = physicsSubject.Id, ExamDate = DateTime.Now.AddDays(20), ImportanceLevel = 4 }
                );
                await context.SaveChangesAsync();
            }

            if (firstStudentProfile != null && mathSubject != null && physicsSubject != null && !context.WeeklyTargets.Any(wt => wt.StudentId == firstStudentProfile.Id))
            {
                var monday = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);
                context.WeeklyTargets.AddRange(
                    new WeeklyTarget { StudentId = firstStudentProfile.Id, SubjectId = mathSubject.Id, TargetHours = 10, WeekStartDate = monday },
                    new WeeklyTarget { StudentId = firstStudentProfile.Id, SubjectId = physicsSubject.Id, TargetHours = 6, WeekStartDate = monday }
                );
                await context.SaveChangesAsync();
            }

            if (firstStudentProfile != null && mathSubject != null && physicsSubject != null && !context.StudyTasks.Any(st => st.StudentId == firstStudentProfile.Id))
            {
                context.StudyTasks.AddRange(
                    new StudyTask { StudentId = firstStudentProfile.Id, SubjectId = mathSubject.Id, ScheduledDate = DateTime.Now.Date.AddDays(1), StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(20, 0, 0), Status = Core.Enums.StudyTaskStatus.Pending },
                    new StudyTask { StudentId = firstStudentProfile.Id, SubjectId = physicsSubject.Id, ScheduledDate = DateTime.Now.Date.AddDays(2), StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(21, 0, 0), Status = Core.Enums.StudyTaskStatus.Completed, CompletedDurationMinutes = 120 }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
