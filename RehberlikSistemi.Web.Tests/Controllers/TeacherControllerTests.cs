<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
=======
>>>>>>> cd5e533 (Add tests for TeacherController.CreateStudyTask)
=======
>>>>>>> cf8150e309825dd1bffceffc1f0b10a4d14eb370
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
<<<<<<< HEAD
<<<<<<< HEAD
>>>>>>> 9a81442 (🧪 Add test for TeacherController.Dashboard)
=======
>>>>>>> cd5e533 (Add tests for TeacherController.CreateStudyTask)
=======
>>>>>>> tests/teacher-controller-plangenerator-3108397973037746152
=======
>>>>>>> cf8150e309825dd1bffceffc1f0b10a4d14eb370
=======
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
>>>>>>> origin/fix/teacher-controller-assign-student-tests-7726696823679279214
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
<<<<<<< HEAD
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> tests/teacher-controller-plangenerator-3108397973037746152
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Teacher;
<<<<<<< HEAD
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
<<<<<<< HEAD
=======
using RehberlikSistemi.Web.Core.Enums;
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Teacher;
>>>>>>> 9a81442 (🧪 Add test for TeacherController.Dashboard)
=======
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Teacher;
>>>>>>> cd5e533 (Add tests for TeacherController.CreateStudyTask)
=======
>>>>>>> tests/teacher-controller-plangenerator-3108397973037746152
=======
>>>>>>> cf8150e309825dd1bffceffc1f0b10a4d14eb370
using Xunit;

namespace RehberlikSistemi.Web.Tests.Controllers
{
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> tests/teacher-controller-plangenerator-3108397973037746152
    public class TeacherControllerTests
=======
    public class TeacherControllerTests : IDisposable
>>>>>>> cf8150e309825dd1bffceffc1f0b10a4d14eb370
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly TeacherController _controller;
        private readonly ApplicationUser _teacherUser;
        private readonly StudentProfile _student;

        public TeacherControllerTests()
=======
using Xunit;
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Data;

namespace RehberlikSistemi.Web.Tests.Controllers
{
    public class TeacherControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
>>>>>>> origin/fix/teacher-controller-assign-student-tests-7726696823679279214
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

<<<<<<< HEAD
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
<<<<<<< HEAD
            var result = await controller.PlanGenerator(1);
<<<<<<< HEAD
=======
=======
>>>>>>> cd5e533 (Add tests for TeacherController.CreateStudyTask)
    public class TeacherControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly TeacherController _controller;
<<<<<<< HEAD
=======
        private readonly ApplicationUser _teacherUser;
        private readonly StudentProfile _student;
>>>>>>> cd5e533 (Add tests for TeacherController.CreateStudyTask)

        public TeacherControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

<<<<<<< HEAD
            _controller = new TeacherController(_context, _mockUserManager.Object);
=======
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
>>>>>>> cd5e533 (Add tests for TeacherController.CreateStudyTask)
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
<<<<<<< HEAD
        public async Task Dashboard_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Dashboard();
>>>>>>> 9a81442 (🧪 Add test for TeacherController.Dashboard)
=======
>>>>>>> tests/teacher-controller-plangenerator-3108397973037746152
=======
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
>>>>>>> cf8150e309825dd1bffceffc1f0b10a4d14eb370
=======
            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            return mockUserManager;
        }

        [Fact]
        public async Task AssignStudent_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync((ApplicationUser)null!);

            var controller = new TeacherController(dbContext, mockUserManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.AssignStudent(1);
>>>>>>> origin/fix/teacher-controller-assign-student-tests-7726696823679279214

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> tests/teacher-controller-plangenerator-3108397973037746152
        public async Task PlanGenerator_StudentNotFound_ReturnsNotFound()
=======
        public async Task CreateStudyTask_StudentNotFoundOrNotOwned_ReturnsNotFound()
>>>>>>> cf8150e309825dd1bffceffc1f0b10a4d14eb370
        {
            // Arrange
            var model = new StudentDetailViewModel { ProfileId = 999 }; // Non-existent student

            // Act
            var result = await _controller.CreateStudyTask(model);
=======
        public async Task AssignStudent_StudentNotFound_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var mockUserManager = GetMockUserManager();
            var user = new ApplicationUser { Id = "teacher1" };

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(user);

            var controller = new TeacherController(dbContext, mockUserManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.AssignStudent(999);
>>>>>>> origin/fix/teacher-controller-assign-student-tests-7726696823679279214

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
<<<<<<< HEAD
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
<<<<<<< HEAD
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
<<<<<<< HEAD
=======
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
>>>>>>> 9a81442 (🧪 Add test for TeacherController.Dashboard)
=======
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
=======
>>>>>>> cf8150e309825dd1bffceffc1f0b10a4d14eb370
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("StudentDetail", redirectResult.ActionName);
            Assert.Equal(_student.Id, redirectResult.RouteValues?["id"]);
            Assert.Contains("Öğrenci seçilen saat aralığında müsait değil", redirectResult.RouteValues?["msg"]?.ToString());
            Assert.False(_controller.ModelState.IsValid);
<<<<<<< HEAD
>>>>>>> cd5e533 (Add tests for TeacherController.CreateStudyTask)
=======
>>>>>>> tests/teacher-controller-plangenerator-3108397973037746152
=======
>>>>>>> cf8150e309825dd1bffceffc1f0b10a4d14eb370
=======
        public async Task AssignStudent_Success_AssignsTeacherIdAndRedirects()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();

            var user1 = new ApplicationUser { Id = "student1" };
            dbContext.Users.Add(user1);
            var student = new StudentProfile { Id = 1, UserId = "student1", TeacherId = null };
            dbContext.StudentProfiles.Add(student);
            await dbContext.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var teacherUser = new ApplicationUser { Id = "teacher1" };

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(teacherUser);

            var controller = new TeacherController(dbContext, mockUserManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.AssignStudent(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Students", redirectResult.ActionName);

            // Verify db context
            var updatedStudent = await dbContext.StudentProfiles.FindAsync(1);
            Assert.NotNull(updatedStudent);
            Assert.Equal("teacher1", updatedStudent.TeacherId);
        }

        [Fact]
        public async Task AssignStudent_TeacherAlreadyAssigned_DoesNotChangeTeacherIdAndRedirects()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();

            var user1 = new ApplicationUser { Id = "student1" };
            dbContext.Users.Add(user1);
            var student = new StudentProfile { Id = 1, UserId = "student1", TeacherId = "otherTeacherId" };
            dbContext.StudentProfiles.Add(student);
            await dbContext.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            var teacherUser = new ApplicationUser { Id = "teacher1" };

            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(teacherUser);

            var controller = new TeacherController(dbContext, mockUserManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await controller.AssignStudent(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Students", redirectResult.ActionName);

            // Verify db context
            var updatedStudent = await dbContext.StudentProfiles.FindAsync(1);
            Assert.NotNull(updatedStudent);
            Assert.Equal("otherTeacherId", updatedStudent.TeacherId); // Teacher ID shouldn't be overridden
>>>>>>> origin/fix/teacher-controller-assign-student-tests-7726696823679279214
        }
    }
}
