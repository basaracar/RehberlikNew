using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using RehberlikSistemi.Web.Controllers;
using RehberlikSistemi.Web.Core.Entities;
using RehberlikSistemi.Web.Core.Enums;
using RehberlikSistemi.Web.Data;
using RehberlikSistemi.Web.Models.Admin;
using Xunit;

namespace RehberlikSistemi.Web.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly ApplicationDbContext _dbContext;

        public AdminControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task CreateStudent_Post_ValidModel_CreatesUserAndProfile_RedirectsToStudents()
        {
            // Arrange
            var controller = new AdminController(_dbContext, _mockUserManager.Object);
            var model = new StudentViewModel
            {
                Email = "test@student.com",
                FirstName = "Test",
                LastName = "Student",
                Password = "Password123!",
                TeacherId = "teacher1",
                GradeLevel = "10",
                TargetUniversity = "Test University",
                ProfileId = 0,
                Id = "test"
            };

            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Student"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await controller.CreateStudent(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Students", redirectResult.ActionName);

            var profile = await _dbContext.StudentProfiles.FirstOrDefaultAsync();
            Assert.NotNull(profile);
            Assert.Equal(model.TeacherId, profile.TeacherId);
            Assert.Equal(model.GradeLevel, profile.GradeLevel);
            Assert.Equal(model.TargetUniversity, profile.TargetUniversity);
        }

        [Fact]
        public async Task CreateStudent_Post_InvalidModel_ReturnsViewWithModelAndTeachers()
        {
            // Arrange
            var controller = new AdminController(_dbContext, _mockUserManager.Object);
            controller.ModelState.AddModelError("Email", "Email is required");
            var model = new StudentViewModel();

            var teachers = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "teacher1", FirstName = "Teacher 1" }
            };
            _mockUserManager.Setup(x => x.GetUsersInRoleAsync("Teacher"))
                .ReturnsAsync(teachers);

            // Act
            var result = await controller.CreateStudent(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.NotNull(controller.ViewBag.Teachers);
        }

        [Fact]
        public async Task CreateStudent_Post_CreateUserFails_ReturnsViewWithErrorsAndTeachers()
        {
            // Arrange
            var controller = new AdminController(_dbContext, _mockUserManager.Object);
            var model = new StudentViewModel
            {
                Email = "test@student.com",
                FirstName = "Test",
                LastName = "Student"
            };

            var errors = new[] { new IdentityError { Description = "Password too weak" } };
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var teachers = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "teacher1", FirstName = "Teacher 1" }
            };
            _mockUserManager.Setup(x => x.GetUsersInRoleAsync("Teacher"))
                .ReturnsAsync(teachers);

            // Act
            var result = await controller.CreateStudent(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, viewResult.Model);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState.Values.SelectMany(v => v.Errors), e => e.ErrorMessage == "Password too weak");
            Assert.NotNull(controller.ViewBag.Teachers);
        }

        [Fact]
        public async Task EditSubject_Get_ValidId_ReturnsViewWithSubject()
        {
            // Arrange
            var subject = new Subject { Name = "Matematik", SubjectType = SubjectType.Sayisal };
            _dbContext.Subjects.Add(subject);
            await _dbContext.SaveChangesAsync();

            var controller = new AdminController(_dbContext, _mockUserManager.Object);

            // Act
            var result = await controller.EditSubject(subject.Id);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Subject>(viewResult.Model);
            Assert.Equal("Matematik", model.Name);
            Assert.Equal(SubjectType.Sayisal, model.SubjectType);
        }

        [Fact]
        public async Task EditSubject_Post_ValidModel_UpdatesAndRedirects()
        {
            // Arrange
            var subject = new Subject { Name = "Matematik", SubjectType = SubjectType.Sayisal };
            _dbContext.Subjects.Add(subject);
            await _dbContext.SaveChangesAsync();

            var controller = new AdminController(_dbContext, _mockUserManager.Object);
            var model = new Subject { Id = subject.Id, Name = "Fizik", SubjectType = SubjectType.Sayisal };

            // Act
            var result = await controller.EditSubject(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Subjects", redirectResult.ActionName);

            var updated = await _dbContext.Subjects.FindAsync(subject.Id);
            Assert.Equal("Fizik", updated!.Name);
        }

        [Fact]
        public async Task DeleteSubject_NoAssignment_DeletesAndRedirects()
        {
            // Arrange
            var subject = new Subject { Name = "Matematik", SubjectType = SubjectType.Sayisal };
            _dbContext.Subjects.Add(subject);
            await _dbContext.SaveChangesAsync();

            var controller = new AdminController(_dbContext, _mockUserManager.Object);

            // Act
            var result = await controller.DeleteSubject(subject.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Subjects", redirectResult.ActionName);
            Assert.Null(await _dbContext.Subjects.FindAsync(subject.Id));
        }

        [Fact]
        public async Task DeleteSubject_HasStudyTask_ReturnsErrorAndDoesNotDelete()
        {
            // Arrange
            var subject = new Subject { Name = "Matematik", SubjectType = SubjectType.Sayisal };
            _dbContext.Subjects.Add(subject);

            var profile = new StudentProfile
            {
                UserId = "student1",
                GradeLevel = "10",
                TargetUniversity = "Test"
            };
            _dbContext.StudentProfiles.Add(profile);
            await _dbContext.SaveChangesAsync();

            var task = new StudyTask
            {
                SubjectId = subject.Id,
                StudentId = profile.Id,
                ScheduledDate = System.DateTime.Today,
                StartTime = System.TimeSpan.FromHours(10),
                EndTime = System.TimeSpan.FromHours(11)
            };
            _dbContext.StudyTasks.Add(task);
            await _dbContext.SaveChangesAsync();

            var controller = new AdminController(_dbContext, _mockUserManager.Object);
            controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());

            // Act
            var result = await controller.DeleteSubject(subject.Id);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Subjects", redirectResult.ActionName);
            Assert.NotNull(await _dbContext.Subjects.FindAsync(subject.Id)); // Subject should NOT be deleted
            Assert.Equal("Bu ders öğrencilere atanmış olduğu için silinemez.", controller.TempData["Error"]);
        }
    }
}
