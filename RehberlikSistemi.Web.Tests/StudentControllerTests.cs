using System;
using System.Collections.Generic;
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
using RehberlikSistemi.Web.Models.Student;
using Xunit;

namespace RehberlikSistemi.Web.Tests
{
    public class StudentControllerTests
    {
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
        }
    }
}
