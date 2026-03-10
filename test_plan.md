1. **Setup Test Environment**: I will keep the created `RehberlikSistemi.Web.Tests` xUnit project which has dependencies on Moq and Microsoft.EntityFrameworkCore.InMemory. I will add it to the solution using `dotnet sln add`.
2. **Review Dependencies**:
   - `UserManager<ApplicationUser>`: I need to mock this using Moq.
   - `ApplicationDbContext`: I will use the In-Memory Entity Framework provider to instantiate this and verify database changes (since testing `MarkTaskComplete` requires fetching and updating relationships `t => t.Student`).
   - `ClaimsPrincipal`: I need to mock the current user to pass the `[Authorize]` attribute requirement and the `_userManager.GetUserAsync(User)` method call.
3. **Write Tests**:
   I will write test methods inside `StudentControllerTests.cs` to cover `StudentController.MarkTaskComplete(int taskId, bool ajax)` method. The tests will cover:
   - **Success (Redirect)**: Updating task status from pending to completed, then verifying redirection to Dashboard and actual status update in db.
   - **Success (Ajax)**: Updating task status using ajax and verifying the JSON response (`new { success = true }`) and the database state.
   - **User Unauthenticated**: Expecting `UnauthorizedResult` when `GetUserAsync` returns null.
   - **Task Not Found / Not Belonging To Student**: Trying to complete a non-existent task, or a task that belongs to another student, and ensuring it does not throw, and properly handles redirect/json false (if ajax).
4. **Build & Verify**: Ensure `dotnet build RehberlikSistemi.Web.Tests` succeeds without compilation errors.
