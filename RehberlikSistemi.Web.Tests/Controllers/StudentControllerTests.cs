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
using Xunit;

namespace RehberlikSistemi.Web.Tests.Controllers
{
    public class StudentControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly ApplicationDbContext _context;

        public StudentControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetMyWeeklySchedule_WhenUserNotFound_ReturnsUnauthorized()
        {
            // Arrange
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null!);

            var controller = new StudentController(_context, _userManagerMock.Object);

            // Act
            var result = await controller.GetMyWeeklySchedule("2023-10-25");

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetMyWeeklySchedule_WhenStudentProfileNotFound_ReturnsNotFound()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var controller = new StudentController(_context, _userManagerMock.Object);

            // Act
            var result = await controller.GetMyWeeklySchedule("2023-10-25");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetMyWeeklySchedule_WhenValidData_ReturnsJsonWithExpectedData()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var studentProfile = new StudentProfile
            {
                Id = 1,
                UserId = user.Id,
                Availabilities = new List<Availability>
                {
                    new Availability { Id = 1, DayOfWeek = DayOfWeek.Monday, IsAvailable = true, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(12, 0, 0) }
                }
            };

            var subject1 = new Subject { Id = 1, Name = "Math" };
            var subject2 = new Subject { Id = 2, Name = "Physics" };

            var studyTask = new StudyTask
            {
                Id = 1,
                StudentId = studentProfile.Id,
                SubjectId = subject1.Id,
                Subject = subject1,
                ScheduledDate = new DateTime(2023, 10, 25), // A Wednesday
                StartTime = new TimeSpan(14, 0, 0),
                EndTime = new TimeSpan(16, 0, 0),
                Status = StudyTaskStatus.Pending
            };

            var exam = new Exam
            {
                Id = 1,
                StudentId = studentProfile.Id,
                SubjectId = subject2.Id,
                Subject = subject2,
                ExamDate = new DateTime(2023, 10, 26, 10, 0, 0) // A Thursday
            };

            studentProfile.StudyTasks = new List<StudyTask> { studyTask };
            studentProfile.Exams = new List<Exam> { exam };

            _context.StudentProfiles.Add(studentProfile);
            _context.Subjects.AddRange(subject1, subject2);
            _context.StudyTasks.Add(studyTask);
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            var controller = new StudentController(_context, _userManagerMock.Object);

            // Act
            // 2023-10-25 is a Wednesday. Week starts on Monday (2023-10-23) and ends Sunday (2023-10-29).
            var result = await controller.GetMyWeeklySchedule("2023-10-25");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);

            // To test the anonymous type properly, we can serialize/deserialize or use reflection
            var jsonString = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            var parsed = System.Text.Json.JsonDocument.Parse(jsonString);

            Assert.Equal("2023-10-23", parsed.RootElement.GetProperty("weekStartDate").GetString());
            Assert.Equal("2023-10-29", parsed.RootElement.GetProperty("weekEndDate").GetString());

            var tasks = parsed.RootElement.GetProperty("tasks");
            Assert.Equal(1, tasks.GetArrayLength());
            Assert.Equal(1, tasks[0].GetProperty("id").GetInt32());
            Assert.Equal("Math", tasks[0].GetProperty("subjectName").GetString());
            Assert.Equal("2023-10-25", tasks[0].GetProperty("date").GetString());
            Assert.Equal("14:00", tasks[0].GetProperty("startTime").GetString()); // hh\:mm format
            Assert.Equal("16:00", tasks[0].GetProperty("endTime").GetString()); // hh\:mm format

            var exams = parsed.RootElement.GetProperty("exams");
            Assert.Equal(1, exams.GetArrayLength());
            Assert.Equal(1, exams[0].GetProperty("id").GetInt32());
            Assert.Equal("Physics", exams[0].GetProperty("subjectName").GetString());
            Assert.Equal("2023-10-26", exams[0].GetProperty("date").GetString());

            var availabilities = parsed.RootElement.GetProperty("availabilities");
            Assert.Equal(1, availabilities.GetArrayLength());
            Assert.Equal((int)DayOfWeek.Monday, availabilities[0].GetProperty("dayOfWeek").GetInt32());
            Assert.Equal("09:00", availabilities[0].GetProperty("startTime").GetString()); // hh\:mm format
        }

        [Fact]
        public async Task GetMyWeeklySchedule_WhenInvalidDateString_UsesToday()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user-id" };
            _userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var studentProfile = new StudentProfile { Id = 1, UserId = user.Id };
            _context.StudentProfiles.Add(studentProfile);
            await _context.SaveChangesAsync();

            var controller = new StudentController(_context, _userManagerMock.Object);

            // Act
            var result = await controller.GetMyWeeklySchedule("invalid-date");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonString = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            var parsed = System.Text.Json.JsonDocument.Parse(jsonString);

            var targetDate = DateTime.Today;
            int diff = (7 + (targetDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            var expectedStart = targetDate.AddDays(-1 * diff).Date;
            var expectedEnd = expectedStart.AddDays(6).Date.AddTicks(TimeSpan.TicksPerDay - 1);

            Assert.Equal(expectedStart.ToString("yyyy-MM-dd"), parsed.RootElement.GetProperty("weekStartDate").GetString());
            Assert.Equal(expectedEnd.ToString("yyyy-MM-dd"), parsed.RootElement.GetProperty("weekEndDate").GetString());
        }
    }
}
