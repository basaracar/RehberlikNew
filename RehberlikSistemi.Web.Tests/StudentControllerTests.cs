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
using Xunit;

namespace RehberlikSistemi.Web.Tests
{
    public class StudentControllerTests
    {
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
        }
    }
}
