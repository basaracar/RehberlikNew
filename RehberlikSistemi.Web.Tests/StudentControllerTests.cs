using System;
using System.Collections.Generic;
<<<<<<< HEAD
=======
using System.Linq;
>>>>>>> origin/testing-improvement-student-controller-2888079067300223416
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
<<<<<<< HEAD
using RehberlikSistemi.Web.Models.Student;
=======
>>>>>>> origin/testing-improvement-student-controller-2888079067300223416
using Xunit;

namespace RehberlikSistemi.Web.Tests
{
    public class StudentControllerTests
    {
<<<<<<< HEAD
        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mgr = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            mgr.Object.UserValidators.Add(new UserValidator<ApplicationUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<ApplicationUser>());
            return mgr;
        }

        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            // Ensure created to trigger OnModelCreating properly, preventing the NullButNotEmpty exception
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task AddAvailability_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetDbContext();
            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null!);

            var controller = new StudentController(dbContext, mockUserManager.Object);

            var model = new StudentAvailabilityViewModel();

            // Act
            var result = await controller.AddAvailability(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddAvailability_StudentProfileNotFound_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetDbContext();
            var user = new ApplicationUser { Id = "test-user-id" };

            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new StudentController(dbContext, mockUserManager.Object);
            var model = new StudentAvailabilityViewModel();

            // Act
            var result = await controller.AddAvailability(model);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddAvailability_ValidModel_AddsAvailabilityAndRedirects()
        {
            // Arrange
            var dbContext = GetDbContext();
            var user = new ApplicationUser { Id = "test-user-id" };
            var studentProfile = new StudentProfile { Id = 1, UserId = user.Id };

            dbContext.Users.Add(user);
            dbContext.StudentProfiles.Add(studentProfile);
            await dbContext.SaveChangesAsync();

            var mockUserManager = GetMockUserManager();
            mockUserManager.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            var controller = new StudentController(dbContext, mockUserManager.Object);

            var model = new StudentAvailabilityViewModel
            {
                NewDayOfWeek = DayOfWeek.Monday,
                NewStartTime = new TimeSpan(9, 0, 0),
                NewEndTime = new TimeSpan(11, 0, 0)
            };

            // Act
            var result = await controller.AddAvailability(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(StudentController.Availability), redirectResult.ActionName);

            var availability = await dbContext.Availabilities.FirstOrDefaultAsync(a => a.StudentId == studentProfile.Id);
            Assert.NotNull(availability);
            Assert.Equal(DayOfWeek.Monday, availability.DayOfWeek);
            Assert.Equal(new TimeSpan(9, 0, 0), availability.StartTime);
            Assert.Equal(new TimeSpan(11, 0, 0), availability.EndTime);
            Assert.True(availability.IsAvailable);
=======
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public StudentControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task MarkTaskComplete_WithValidTask_UpdatesStatusAndRedirects()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1" };
            var student = new StudentProfile { Id = 1, UserId = "user1" };
            var task = new StudyTask { Id = 10, StudentId = 1, Status = Core.Enums.StudyTaskStatus.Pending, Student = student };

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                context.Users.Add(user);
                context.StudentProfiles.Add(student);
                context.StudyTasks.Add(task);
                context.SaveChanges();
            }

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                var controller = new StudentController(context, _mockUserManager.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "user1") })) }
                    }
                };

                // Act
                var result = await controller.MarkTaskComplete(10);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Dashboard", redirectResult.ActionName);

                var updatedTask = await context.StudyTasks.FindAsync(10);
                Assert.Equal(Core.Enums.StudyTaskStatus.Completed, updatedTask!.Status);
            }
        }

        [Fact]
        public async Task MarkTaskComplete_WithValidTaskAjax_UpdatesStatusAndReturnsJson()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user2" };
            var student = new StudentProfile { Id = 2, UserId = "user2" };
            var task = new StudyTask { Id = 20, StudentId = 2, Status = Core.Enums.StudyTaskStatus.Pending, Student = student };

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                context.Users.Add(user);
                context.StudentProfiles.Add(student);
                context.StudyTasks.Add(task);
                context.SaveChanges();
            }

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                var controller = new StudentController(context, _mockUserManager.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "user2") })) }
                    }
                };

                // Act
                var result = await controller.MarkTaskComplete(20, ajax: true);

                // Assert
                var jsonResult = Assert.IsType<JsonResult>(result);
                var value = jsonResult.Value;
                Assert.NotNull(value);
                var successProperty = value.GetType().GetProperty("success");
                Assert.NotNull(successProperty);
                var successValue = (bool)successProperty.GetValue(value)!;
                Assert.True(successValue);

                var updatedTask = await context.StudyTasks.FindAsync(20);
                Assert.Equal(Core.Enums.StudyTaskStatus.Completed, updatedTask!.Status);
            }
        }

        [Fact]
        public async Task MarkTaskComplete_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((ApplicationUser)null!);

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                var controller = new StudentController(context, _mockUserManager.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext()
                    }
                };

                // Act
                var result = await controller.MarkTaskComplete(10);

                // Assert
                Assert.IsType<UnauthorizedResult>(result);
            }
        }

        [Fact]
        public async Task MarkTaskComplete_TaskNotFound_ReturnsRedirect()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user3" };

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                var controller = new StudentController(context, _mockUserManager.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "user3") })) }
                    }
                };

                // Act
                var result = await controller.MarkTaskComplete(999);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Dashboard", redirectResult.ActionName);
            }
        }

        [Fact]
        public async Task MarkTaskComplete_TaskNotFoundAjax_ReturnsJsonFalse()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user4" };

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                var controller = new StudentController(context, _mockUserManager.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "user4") })) }
                    }
                };

                // Act
                var result = await controller.MarkTaskComplete(999, ajax: true);

                // Assert
                var jsonResult = Assert.IsType<JsonResult>(result);
                var value = jsonResult.Value;
                Assert.NotNull(value);

                var successProperty = value.GetType().GetProperty("success");
                Assert.NotNull(successProperty);
                var successValue = (bool)successProperty.GetValue(value)!;
                Assert.False(successValue);

                var messageProperty = value.GetType().GetProperty("message");
                Assert.NotNull(messageProperty);
                var messageValue = (string)messageProperty.GetValue(value)!;
                Assert.Equal("Görev bulunamadı veya yetkiniz yok.", messageValue);
            }
        }

        [Fact]
        public async Task MarkTaskComplete_TaskBelongsToAnotherStudent_ReturnsRedirectAndDoesNotUpdateStatus()
        {
            // Arrange
            var user1 = new ApplicationUser { Id = "user1" };
            var student1 = new StudentProfile { Id = 1, UserId = "user1" };

            var user2 = new ApplicationUser { Id = "user2" };
            var student2 = new StudentProfile { Id = 2, UserId = "user2" };

            var task = new StudyTask { Id = 30, StudentId = 2, Status = Core.Enums.StudyTaskStatus.Pending, Student = student2 };

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                context.Users.Add(user1);
                context.Users.Add(user2);
                context.StudentProfiles.Add(student1);
                context.StudentProfiles.Add(student2);
                context.StudyTasks.Add(task);
                context.SaveChanges();
            }

            // Current user is user1
            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user1);

            using (var context = new ApplicationDbContext(_dbOptions))
            {
                var controller = new StudentController(context, _mockUserManager.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "user1") })) }
                    }
                };

                // Act
                var result = await controller.MarkTaskComplete(30);

                // Assert
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Dashboard", redirectResult.ActionName);

                var unchangedTask = await context.StudyTasks.FindAsync(30);
                Assert.Equal(Core.Enums.StudyTaskStatus.Pending, unchangedTask!.Status);
            }
>>>>>>> origin/testing-improvement-student-controller-2888079067300223416
        }
    }
}
