using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Admin;
using Xunit;

namespace RehberlikSistemi.Web.Tests.Controllers
{
    public class AdminControllerTests
    {
        [Fact]
        public async Task CreateTeacher_ReturnsViewWithError_WhenCreationFails()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_AdminController")
                .Options;

            using var context = new ApplicationDbContext(options);

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError { Description = "Password too short." },
                    new IdentityError { Description = "Email already exists." }
                ));

            var controller = new AdminController(context, userManagerMock.Object);
            var model = new TeacherViewModel
            {
                Email = "test@teacher.com",
                FirstName = "Test",
                LastName = "Teacher",
                Password = "123"
            };

            // Act
            var result = await controller.CreateTeacher(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var returnedModel = Assert.IsType<TeacherViewModel>(viewResult.Model);
            Assert.Equal(model.Email, returnedModel.Email);
            Assert.False(controller.ModelState.IsValid);
            Assert.Equal(2, controller.ModelState.ErrorCount);

            // Checking specific errors if needed
            var errors = controller.ModelState[string.Empty]?.Errors;
            Assert.NotNull(errors);
            Assert.Contains(errors, e => e.ErrorMessage == "Password too short.");
            Assert.Contains(errors, e => e.ErrorMessage == "Email already exists.");
        }
    }
}
