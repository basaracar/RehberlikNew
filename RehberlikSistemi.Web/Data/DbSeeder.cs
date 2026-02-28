using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;
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
            string[] roles = { "Admin", "Teacher", "Student" };
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
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }

            // 3. Örnek Öğretmen Ekleme
            var teacherUser = await userManager.FindByEmailAsync("teacher@rehberlik.com");
            if (teacherUser == null)
            {
                var newTeacher = new ApplicationUser
                {
                    UserName = "teacher@rehberlik.com",
                    Email = "teacher@rehberlik.com",
                    FirstName = "Örnek",
                    LastName = "Öğretmen",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(newTeacher, "Teacher123!");
                await userManager.AddToRoleAsync(newTeacher, "Teacher");
                teacherUser = newTeacher;
            }

            // 4. Örnek Öğrenci Ekleme
            var studentUser = await userManager.FindByEmailAsync("student@rehberlik.com");
            if (studentUser == null)
            {
                var newStudent = new ApplicationUser
                {
                    UserName = "student@rehberlik.com",
                    Email = "student@rehberlik.com",
                    FirstName = "Örnek",
                    LastName = "Öğrenci",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(newStudent, "Student123!");
                await userManager.AddToRoleAsync(newStudent, "Student");
                studentUser = newStudent;
            }

            var studentProfile = context.StudentProfiles.FirstOrDefault(sp => sp.UserId == studentUser.Id);
            if (studentProfile == null)
            {
                studentProfile = new StudentProfile
                {
                    UserId = studentUser.Id,
                    TeacherId = teacherUser?.Id,
                    GradeLevel = "12",
                    TargetUniversity = "Boğaziçi Üniversitesi / Bilgisayar Mühendisliği"
                };
                context.StudentProfiles.Add(studentProfile);
                await context.SaveChangesAsync();
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

            // 6. Örnek Verilerin (Availability, Exam, vs.) Eklenmesi
            var mathSubject = context.Subjects.FirstOrDefault(s => s.Name == "Matematik");
            var physicsSubject = context.Subjects.FirstOrDefault(s => s.Name == "Fizik");

            if (studentProfile != null && !context.Availabilities.Any(a => a.StudentId == studentProfile.Id))
            {
                context.Availabilities.AddRange(
                    new Availability { StudentId = studentProfile.Id, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(22, 0, 0) },
                    new Availability { StudentId = studentProfile.Id, DayOfWeek = DayOfWeek.Wednesday, StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(23, 0, 0) },
                    new Availability { StudentId = studentProfile.Id, DayOfWeek = DayOfWeek.Saturday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(15, 0, 0) }
                );
                await context.SaveChangesAsync();
            }

            if (studentProfile != null && mathSubject != null && physicsSubject != null && !context.Exams.Any(e => e.StudentId == studentProfile.Id))
            {
                context.Exams.AddRange(
                    new Exam { StudentId = studentProfile.Id, SubjectId = mathSubject.Id, ExamDate = DateTime.Now.AddDays(15), ImportanceLevel = 5 },
                    new Exam { StudentId = studentProfile.Id, SubjectId = physicsSubject.Id, ExamDate = DateTime.Now.AddDays(20), ImportanceLevel = 4 }
                );
                await context.SaveChangesAsync();
            }

            if (studentProfile != null && mathSubject != null && physicsSubject != null && !context.WeeklyTargets.Any(wt => wt.StudentId == studentProfile.Id))
            {
                var monday = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);
                context.WeeklyTargets.AddRange(
                    new WeeklyTarget { StudentId = studentProfile.Id, SubjectId = mathSubject.Id, TargetHours = 10, WeekStartDate = monday },
                    new WeeklyTarget { StudentId = studentProfile.Id, SubjectId = physicsSubject.Id, TargetHours = 6, WeekStartDate = monday }
                );
                await context.SaveChangesAsync();
            }

            if (studentProfile != null && mathSubject != null && physicsSubject != null && !context.StudyTasks.Any(st => st.StudentId == studentProfile.Id))
            {
                context.StudyTasks.AddRange(
                    new StudyTask { StudentId = studentProfile.Id, SubjectId = mathSubject.Id, ScheduledDate = DateTime.Now.Date.AddDays(1), StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(20, 0, 0), Status = Core.Enums.StudyTaskStatus.Pending },
                    new StudyTask { StudentId = studentProfile.Id, SubjectId = physicsSubject.Id, ScheduledDate = DateTime.Now.Date.AddDays(2), StartTime = new TimeSpan(19, 0, 0), EndTime = new TimeSpan(21, 0, 0), Status = Core.Enums.StudyTaskStatus.Completed, CompletedDurationMinutes = 120 }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
