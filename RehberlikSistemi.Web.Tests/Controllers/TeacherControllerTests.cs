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
        private readonly ApplicationUser _teacherUser;
        private readonly StudentProfile _student;

        public TeacherControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _teacherUser = new ApplicationUser { Id = "teacher1", UserName = "teacher@test.com" };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _teacherUser.Id) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller = new TeacherController(_context, _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(_teacherUser);

            // Seed student
            _student = new StudentProfile
            {
                Id = 1,
                UserId = "student1",
                TeacherId = _teacherUser.Id,
                GradeLevel = "12",
                TargetUniversity = "MIT",
                Availabilities = new List<Availability>
                {
                    new Availability
                    {
                        DayOfWeek = DayOfWeek.Monday,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0),
                        IsAvailable = true
                    }
                }
            };
            _context.StudentProfiles.Add(_student);

            // Seed subject
            _context.Subjects.Add(new Subject { Id = 1, Name = "Math" });

            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateStudyTask_ValidModel_ReturnsRedirectToStudentDetail()
        {
            // Arrange
            var model = new StudentDetailViewModel
            {
                ProfileId = _student.Id,
                SubjectId = 1,
                ScheduledDate = new DateTime(2023, 10, 2), // Monday
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(11, 0, 0)
            };

            // Act
            var result = await _controller.CreateStudyTask(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("StudentDetail", redirectResult.ActionName);
            Assert.Equal(_student.Id, redirectResult.RouteValues?["id"]);
            Assert.NotNull(redirectResult.RouteValues?["msg"]);
            Assert.Contains("başarıyla", redirectResult.RouteValues?["msg"]?.ToString());

            var createdTask = await _context.StudyTasks.FirstOrDefaultAsync();
            Assert.NotNull(createdTask);
            Assert.Equal(model.ProfileId, createdTask.StudentId);
            Assert.Equal(model.SubjectId, createdTask.SubjectId);
            Assert.Equal(model.ScheduledDate, createdTask.ScheduledDate);
            Assert.Equal(model.StartTime, createdTask.StartTime);
            Assert.Equal(model.EndTime, createdTask.EndTime);
        }

        [Theory]
        [InlineData(10, 11, 10, 11, true)] // Exact match
        [InlineData(10, 12, 11, 13, true)] // Overlaps at end
        [InlineData(11, 13, 10, 12, true)] // Overlaps at start
        [InlineData(10, 14, 11, 12, true)] // Contains existing task
        [InlineData(11, 12, 10, 14, true)] // Is contained by existing task
        [InlineData(9, 10, 10, 11, false)] // Ends when existing starts
        [InlineData(11, 12, 10, 11, false)] // Starts when existing ends
        public async Task CreateStudyTask_CollisionLogic_ValidatesCorrectly(int existingStart, int existingEnd, int newStart, int newEnd, bool expectCollision)
        {
            // Arrange
            var date = new DateTime(2023, 10, 2); // Monday
            var existingTask = new StudyTask
            {
                StudentId = _student.Id,
                SubjectId = 1,
                ScheduledDate = date,
                StartTime = new TimeSpan(existingStart, 0, 0),
                EndTime = new TimeSpan(existingEnd, 0, 0)
            };
            _context.StudyTasks.Add(existingTask);
            await _context.SaveChangesAsync();

            var model = new StudentDetailViewModel
            {
                ProfileId = _student.Id,
                SubjectId = 1,
                ScheduledDate = date,
                StartTime = new TimeSpan(newStart, 0, 0),
                EndTime = new TimeSpan(newEnd, 0, 0)
            };

            // Act
            var result = await _controller.CreateStudyTask(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("StudentDetail", redirectResult.ActionName);
            Assert.Equal(_student.Id, redirectResult.RouteValues?["id"]);

            if (expectCollision)
            {
                Assert.NotNull(redirectResult.RouteValues?["msg"]);
                Assert.Contains("Çakışma", redirectResult.RouteValues?["msg"]?.ToString());
                Assert.Equal(1, await _context.StudyTasks.CountAsync()); // Only existing task
            }
            else
            {
                Assert.NotNull(redirectResult.RouteValues?["msg"]);
                Assert.Contains("başarıyla", redirectResult.RouteValues?["msg"]?.ToString());
                Assert.Equal(2, await _context.StudyTasks.CountAsync()); // Both tasks
            }
        }

        [Fact]
        public async Task CreateStudyTask_InvalidModelState_ReturnsRedirectWithError()
        {
            // Arrange
            var model = new StudentDetailViewModel { ProfileId = _student.Id };
            _controller.ModelState.AddModelError("Error", "Sample Error");

            // Act
            var result = await _controller.CreateStudyTask(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("StudentDetail", redirectResult.ActionName);
            Assert.Equal(_student.Id, redirectResult.RouteValues?["id"]);
            Assert.Contains("Form doğrulanamadı", redirectResult.RouteValues?["msg"]?.ToString());
        }

        [Fact]
        public async Task CreateStudyTask_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null!);
            var model = new StudentDetailViewModel();

            // Act
            var result = await _controller.CreateStudyTask(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateStudyTask_StudentNotFoundOrNotOwned_ReturnsNotFound()
        {
            // Arrange
            var model = new StudentDetailViewModel { ProfileId = 999 }; // Non-existent student

            // Act
            var result = await _controller.CreateStudyTask(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateStudyTask_StartTimeGreaterThanEndTime_ReturnsRedirectWithError()
        {
            // Arrange
            var model = new StudentDetailViewModel
            {
                ProfileId = _student.Id,
                SubjectId = 1,
                ScheduledDate = new DateTime(2023, 10, 2),
                StartTime = new TimeSpan(12, 0, 0),
                EndTime = new TimeSpan(11, 0, 0) // Invalid time
            };

            // Act
            var result = await _controller.CreateStudyTask(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("StudentDetail", redirectResult.ActionName);
            Assert.Equal(_student.Id, redirectResult.RouteValues?["id"]);
            Assert.Contains("Bitiş saati başlangıç saatinden büyük olmalıdır", redirectResult.RouteValues?["msg"]?.ToString());
            Assert.False(_controller.ModelState.IsValid);
        }

        [Theory]
        [InlineData(2023, 10, 3, 10, 11)] // Tuesday - Not available at all
        [InlineData(2023, 10, 2, 8, 10)]  // Monday - Starts before available
        [InlineData(2023, 10, 2, 16, 18)] // Monday - Ends after available
        public async Task CreateStudyTask_StudentNotAvailable_ReturnsRedirectWithError(int year, int month, int day, int startHour, int endHour)
        {
            // Arrange
            var model = new StudentDetailViewModel
            {
                ProfileId = _student.Id,
                SubjectId = 1,
                ScheduledDate = new DateTime(year, month, day),
                StartTime = new TimeSpan(startHour, 0, 0),
                EndTime = new TimeSpan(endHour, 0, 0)
            };

            // Act
            var result = await _controller.CreateStudyTask(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("StudentDetail", redirectResult.ActionName);
            Assert.Equal(_student.Id, redirectResult.RouteValues?["id"]);
            Assert.Contains("Öğrenci seçilen saat aralığında müsait değil", redirectResult.RouteValues?["msg"]?.ToString());
            Assert.False(_controller.ModelState.IsValid);
        }
    }
}
