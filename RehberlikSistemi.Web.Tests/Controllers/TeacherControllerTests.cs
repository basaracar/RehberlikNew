using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Data;

namespace RehberlikSistemi.Web.Tests.Controllers
{
    public class TeacherControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

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

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
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

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
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
        }
    }
}
