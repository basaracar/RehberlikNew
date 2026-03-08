using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace RehberlikSistemi.Web.Tests.Controllers
{
    public class TeacherControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            return mgr;
        }

        private TeacherController GetController(ApplicationDbContext context, Mock<UserManager<ApplicationUser>> mockUserManager, ApplicationUser? currentUser = null)
        {
            if (currentUser != null)
            {
                mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(currentUser);
            }
            else
            {
                mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null!);
            }

            var controller = new TeacherController(context, mockUserManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            return controller;
        }

        [Fact]
        public async Task PlanGenerator_UserIsNull_ReturnsNotFound()
        {
            // Arrange
            using var context = GetDbContext();
            var mockUserManager = GetMockUserManager();
            var controller = GetController(context, mockUserManager, null);

            // Act
            var result = await controller.PlanGenerator(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PlanGenerator_StudentNotFound_ReturnsNotFound()
        {
            // Arrange
            using var context = GetDbContext();
            var teacher = new ApplicationUser { Id = "teacher1" };
            var mockUserManager = GetMockUserManager();
            var controller = GetController(context, mockUserManager, teacher);

            // Act
            var result = await controller.PlanGenerator(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PlanGenerator_StudentTeacherMismatch_ReturnsNotFound()
        {
            // Arrange
            using var context = GetDbContext();
            var teacher = new ApplicationUser { Id = "teacher1" };
            var studentUser = new ApplicationUser { Id = "student1", FirstName = "John", LastName = "Doe" };

            var studentProfile = new StudentProfile
            {
                Id = 1,
                UserId = "student1",
                User = studentUser,
                TeacherId = "otherTeacher" // Mismatch
            };

            context.StudentProfiles.Add(studentProfile);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = GetController(context, mockUserManager, teacher);

            // Act
            var result = await controller.PlanGenerator(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PlanGenerator_HasExistingTasksNext7Days_ReturnsUnsuccessful()
        {
            // Arrange
            using var context = GetDbContext();
            var teacher = new ApplicationUser { Id = "teacher1" };
            var studentUser = new ApplicationUser { Id = "student1", FirstName = "John", LastName = "Doe" };

            var studentProfile = new StudentProfile
            {
                Id = 1,
                UserId = "student1",
                User = studentUser,
                TeacherId = "teacher1",
                Availabilities = new List<Availability>()
            };

            var taskDate = DateTime.Now.Date.AddDays(2);
            context.StudyTasks.Add(new StudyTask
            {
                StudentId = 1,
                ScheduledDate = taskDate,
                SubjectId = 1
            });

            context.StudentProfiles.Add(studentProfile);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = GetController(context, mockUserManager, teacher);

            // Act
            var result = await controller.PlanGenerator(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<PlanGeneratorViewModel>(result.Model);
            Assert.False(model.IsSuccessful);
            Assert.Contains("zaten planlanmış dersleri mevcut", model.Message);
        }

        [Fact]
        public async Task PlanGenerator_NoAvailabilities_ReturnsUnsuccessful()
        {
            // Arrange
            using var context = GetDbContext();
            var teacher = new ApplicationUser { Id = "teacher1" };
            var studentUser = new ApplicationUser { Id = "student1", FirstName = "John", LastName = "Doe" };

            var studentProfile = new StudentProfile
            {
                Id = 1,
                UserId = "student1",
                User = studentUser,
                TeacherId = "teacher1",
                Availabilities = new List<Availability>() // Empty
            };

            context.StudentProfiles.Add(studentProfile);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = GetController(context, mockUserManager, teacher);

            // Act
            var result = await controller.PlanGenerator(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<PlanGeneratorViewModel>(result.Model);
            Assert.False(model.IsSuccessful);
            Assert.Contains("müsaitlik (Availability) bilgisi bulunamadığı", model.Message);
        }

        [Fact]
        public async Task PlanGenerator_GeneratesTasksBasedOnPriority()
        {
            // Arrange
            using var context = GetDbContext();
            var teacher = new ApplicationUser { Id = "teacher1" };
            var studentUser = new ApplicationUser { Id = "student1", FirstName = "John", LastName = "Doe" };

            var subject1 = new Subject { Id = 1, Name = "Math" };
            var subject2 = new Subject { Id = 2, Name = "Physics" };
            var subject3 = new Subject { Id = 3, Name = "Chemistry" };
            context.Subjects.AddRange(subject1, subject2, subject3);

            var currentDate = DateTime.Now;
            var targetDay = currentDate.Date.AddDays(1);
            var dayOfWeek = targetDay.DayOfWeek;

            var studentProfile = new StudentProfile
            {
                Id = 1,
                UserId = "student1",
                User = studentUser,
                TeacherId = "teacher1",
                Availabilities = new List<Availability>
                {
                    // 5 hours of availability next day
                    new Availability
                    {
                        DayOfWeek = dayOfWeek,
                        IsAvailable = true,
                        StartTime = new TimeSpan(10, 0, 0),
                        EndTime = new TimeSpan(15, 0, 0)
                    }
                },
                Exams = new List<Exam>
                {
                    // Upcoming Exam < 7 days (+5) + ImportanceLevel (5) = +10 priority for Subject 1. Base is 1. Total: 11
                    new Exam { SubjectId = 1, ExamDate = currentDate.AddDays(3), ImportanceLevel = 5 },
                    // Past Exam with Score < 50 (+4). Base is 1. Total: 5 priority for Subject 2
                    new Exam { SubjectId = 2, ExamDate = currentDate.AddDays(-10), Score = 40 }
                }
            };
            // Subject 3 has only base priority: 1
            // Total pool size: 11 + 5 + 1 = 17.
            // When we pick 5 hours, Math (Subj 1) will be picked the most (11 times in the pool).
            // Queue is ordered by priority descending: Math, Physics, Chemistry.

            context.StudentProfiles.Add(studentProfile);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = GetController(context, mockUserManager, teacher);

            // Act
            var result = await controller.PlanGenerator(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<PlanGeneratorViewModel>(result.Model);
            Assert.True(model.IsSuccessful);
            Assert.NotEmpty(model.ProposedTasks);

            // Should have 5 tasks generated for the next day
            Assert.Equal(5, model.ProposedTasks.Count);

            // Because priority order is Subject1 (11), Subject2 (5), Subject3 (1)
            // The pool looks like [Subj1... (11x), Subj2... (5x), Subj3]
            // We take first 5 from the pool, they should all be Subject 1!
            Assert.All(model.ProposedTasks, t => Assert.Equal(1, t.SubjectId));
        }

        [Fact]
        public async Task PlanGenerator_PastExamBetween50And70_IncreasesPriority()
        {
            // Arrange
            using var context = GetDbContext();
            var teacher = new ApplicationUser { Id = "teacher1" };
            var studentUser = new ApplicationUser { Id = "student1", FirstName = "John", LastName = "Doe" };

            var subject1 = new Subject { Id = 1, Name = "Math" };
            var subject2 = new Subject { Id = 2, Name = "Physics" };
            context.Subjects.AddRange(subject1, subject2);

            var currentDate = DateTime.Now;
            var targetDay = currentDate.Date.AddDays(1);
            var dayOfWeek = targetDay.DayOfWeek;

            var studentProfile = new StudentProfile
            {
                Id = 1,
                UserId = "student1",
                User = studentUser,
                TeacherId = "teacher1",
                Availabilities = new List<Availability>
                {
                    new Availability
                    {
                        DayOfWeek = dayOfWeek,
                        IsAvailable = true,
                        StartTime = new TimeSpan(10, 0, 0),
                        EndTime = new TimeSpan(13, 0, 0) // 3 hours
                    }
                },
                Exams = new List<Exam>
                {
                    // Past Exam with Score < 70 (+2). Base is 1. Total: 3 priority for Subject 2
                    new Exam { SubjectId = 2, ExamDate = currentDate.AddDays(-10), Score = 60 }
                }
            };
            // Subject 1 has base priority: 1
            // Pool: Subject 2 (x3), Subject 1 (x1) -> [Subj2, Subj2, Subj2, Subj1]
            // Picking 3 tasks, all should be Subject 2

            context.StudentProfiles.Add(studentProfile);
            await context.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var controller = GetController(context, mockUserManager, teacher);

            // Act
            var result = await controller.PlanGenerator(1) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<PlanGeneratorViewModel>(result.Model);
            Assert.True(model.IsSuccessful);
            Assert.Equal(3, model.ProposedTasks.Count);

            Assert.All(model.ProposedTasks, t => Assert.Equal(2, t.SubjectId));
        }
    }
}
