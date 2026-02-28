using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RehberlikSistemi.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExamScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Exams",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "Exams");
        }
    }
}
