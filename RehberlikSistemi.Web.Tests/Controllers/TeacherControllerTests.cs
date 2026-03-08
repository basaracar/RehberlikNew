using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Core.Enums;
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Teacher;
using Xunit;

namespace RehberlikSistemi.Web.Tests.Controllers
{
    public class TeacherControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly TeacherController _controller;

        public TeacherControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new TeacherController(_context, _mockUserManager.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task Dashboard_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Dashboard();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Dashboard_ValidUser_ReturnsCorrectViewModel()
        {
            // Arrange
            var teacherId = "teacher1";
            var teacherUser = new ApplicationUser { Id = teacherId, FirstName = "Teacher", LastName = "One" };
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(teacherUser);

            var subject1 = new Subject { Id = 1, Name = "Math" };
            _context.Subjects.Add(subject1);

            var studentUser1 = new ApplicationUser { Id = "student1", FirstName = "John", LastName = "Doe" };
            var studentProfile1 = new StudentProfile { Id = 1, UserId = "student1", User = studentUser1, TeacherId = teacherId, GradeLevel = "10" };

            var studentUser2 = new ApplicationUser { Id = "student2", FirstName = "Jane", LastName = "Smith" };
            var studentProfile2 = new StudentProfile { Id = 2, UserId = "student2", User = studentUser2, TeacherId = teacherId, GradeLevel = "11" };

            var studentUser3 = new ApplicationUser { Id = "student3", FirstName = "Other", LastName = "Student" };
            var studentProfile3 = new StudentProfile { Id = 3, UserId = "student3", User = studentUser3, TeacherId = "otherTeacherId", GradeLevel = "12" };

            _context.StudentProfiles.AddRange(studentProfile1, studentProfile2, studentProfile3);

            // Add some study tasks for John Doe (1 Completed out of 2 = 50% rate = Yavaş status warning)
            var task1 = new StudyTask { Id = 1, StudentId = 1, SubjectId = 1, Status = StudyTaskStatus.Completed, ScheduledDate = DateTime.Today };
            var task2 = new StudyTask { Id = 2, StudentId = 1, SubjectId = 1, Status = StudyTaskStatus.Pending, ScheduledDate = DateTime.Today.AddDays(1) };

            // Add study tasks for Jane Smith (0 Completed out of 1 = 0% rate = Riskli status danger)
            var task3 = new StudyTask { Id = 3, StudentId = 2, SubjectId = 1, Status = StudyTaskStatus.Pending, ScheduledDate = DateTime.Today };

            _context.StudyTasks.AddRange(task1, task2, task3);

            // Add weekly targets in the current month
            var target1 = new WeeklyTarget { Id = 1, StudentId = 1, SubjectId = 1, TargetHours = 10, WeekStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 5) };
            _context.WeeklyTargets.Add(target1);

            await _context.SaveChangesAsync();

            // Setup Controller Context for User
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, teacherId),
            }, "mock"));
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await _controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<TeacherDashboardViewModel>(viewResult.Model);

            Assert.Equal("Teacher One", model.TeacherFullName);
            Assert.Equal(2, model.TotalStudents); // Only student1 and student2

            // Average Completion Rate = (50% + 0%) / 2 = 25%
            Assert.Equal(25, model.AverageCompletionRate);

            // AtRiskStudentsCount (CompletionRate < 40 is Riskli) -> Jane Smith is 0%
            Assert.Equal(1, model.AtRiskStudentsCount);

            Assert.Equal(1, model.MonthlyGoalCount);

            Assert.Equal(2, model.RecentStudents.Count);

            var jane = model.RecentStudents.First(s => s.FullName == "Jane Smith");
            Assert.Equal(0, jane.CompletionRate);
            Assert.Equal("Riskli", jane.Status);
            Assert.Equal("danger", jane.StatusClass);

            var john = model.RecentStudents.First(s => s.FullName == "John Doe");
            Assert.Equal(50, john.CompletionRate);
            Assert.Equal("Yavaş", john.Status);
            Assert.Equal("warning", john.StatusClass);

            Assert.Equal(3, model.RecentActivities.Count);
        }
    }
}
