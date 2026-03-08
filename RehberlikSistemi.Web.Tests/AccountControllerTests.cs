using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace RehberlikSistemi.Web.Tests
{
    public class AccountControllerTests
    {
        private Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private Mock<SignInManager<ApplicationUser>> MockSignInManager(UserManager<ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(userManager, contextAccessor.Object, claimsFactory.Object, null!, null!, null!, null!);
        }

        [Fact]
        public async Task Logout_CallsSignOutAsyncAndRedirectsToLogin()
        {
            // Arrange
            var userManagerMock = MockUserManager();
            var signInManagerMock = MockSignInManager(userManagerMock.Object);

            var controller = new AccountController(signInManagerMock.Object, userManagerMock.Object);

            // Act
            var result = await controller.Logout();

            // Assert
            signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
            Assert.Equal("Account", redirectResult.ControllerName);
        }
    }
}
