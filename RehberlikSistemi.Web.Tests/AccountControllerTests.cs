using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
using Xunit;

namespace RehberlikSistemi.Web.Tests
{
    public class AccountControllerTests
    {
        private Mock<SignInManager<ApplicationUser>> GetMockSignInManager()
        {
            var userManager = GetMockUserManager();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

            return new Mock<SignInManager<ApplicationUser>>(
                userManager.Object,
                contextAccessor.Object,
                claimsFactory.Object,
                null, null, null, null);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        private AccountController CreateController(ClaimsPrincipal user)
        {
            var controller = new AccountController(GetMockSignInManager().Object, GetMockUserManager().Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        [Fact]
        public void Login_Get_WhenUserIsNotAuthenticated_ReturnsViewResult()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var controller = CreateController(user);

            // Act
            var result = controller.Login();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Theory]
        [InlineData("Admin", "Admin", "Dashboard")]
        [InlineData("Teacher", "Teacher", "Dashboard")]
        [InlineData("Student", "Student", "Dashboard")]
        [InlineData("Unknown", "Home", "Index")]
        public void Login_Get_WhenUserIsAuthenticated_RedirectsToDashboardBasedOnRole(string role, string expectedController, string expectedAction)
        {
            // Arrange
            var identity = new ClaimsIdentity("TestAuthType");
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
            var user = new ClaimsPrincipal(identity);

            var controller = CreateController(user);

            // Act
            var result = controller.Login() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAction, result.ActionName);
            Assert.Equal(expectedController, result.ControllerName);
        }
    }
}
