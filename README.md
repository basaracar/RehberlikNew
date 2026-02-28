# Rehberlik Sistemi (Counseling System)

## Overview
Rehberlik Sistemi is a web-based counseling and guidance management system built with ASP.NET Core 9.0. It provides a platform for managing student counseling activities, facilitating communication between administrators, teachers, and students.

## Key Features
* **Role-Based Access Control:** Secure authentication and authorization using ASP.NET Core Identity with predefined roles:
  * **Admin:** Full system access, user management, and configuration.
  * **Teacher (Counselor):** Access to student records, ability to manage counseling sessions and notes.
  * **Student:** Access to personal counseling history and communication with counselors.
* **Database Management:** Utilizes Entity Framework Core 9.0 with SQL Server for robust data storage and retrieval.
* **Modern Web Interface:** Built with ASP.NET Core MVC, providing a responsive and user-friendly experience.

## Technologies Used
* **Backend:** ASP.NET Core 9.0 (MVC)
* **ORM:** Entity Framework Core 9.0
* **Database:** Microsoft SQL Server
* **Authentication/Authorization:** ASP.NET Core Identity
* **Frontend:** HTML, CSS, JavaScript (Razor Views)

## Project Structure
* `Controllers/`: Contains the MVC controllers handling user requests (e.g., `AdminController`, `TeacherController`, `StudentController`, `AccountController`).
* `Core/`: Includes core business logic, entities, and interfaces.
* `Data/`: Contains the Entity Framework `ApplicationDbContext` and database seeding logic.
* `Models/`: Data transfer objects (DTOs) and view models used across the application.
* `Views/`: Razor views for rendering the UI.
* `wwwroot/`: Static assets such as CSS, JavaScript, and images.

## Getting Started

### Prerequisites
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* SQL Server (LocalDB or a dedicated instance)

### Installation & Setup
1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd <repository-directory>
   ```

2. **Configure the Database Connection:**
   Open `RehberlikSistemi.Web/appsettings.Development.json` (or `appsettings.json`) and update the `DefaultConnection` string to point to your SQL Server instance.

3. **Apply Database Migrations:**
   Navigate to the project directory and run the Entity Framework Core migrations to create the database schema:
   ```bash
   cd RehberlikSistemi.Web
   dotnet ef database update
   ```
   *Note: If you don't have the EF Core CLI tools installed, you can install them globally using `dotnet tool install --global dotnet-ef`.*

4. **Run the Application:**
   Start the application using the .NET CLI:
   ```bash
   dotnet run
   ```
   The application will start, and you can access it via the URL provided in the console output (typically `https://localhost:5001` or `http://localhost:5000`).

### Default Users (Seeded)
The application may seed default users and roles upon the first run. Please check `DbSeeder.SeedRolesAndUsersAsync` in `Program.cs` for the specific credentials created during initialization.

## License
[Add License Information Here]
