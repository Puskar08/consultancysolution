using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace consultancysolution.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCourseChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ModifiedCoursePrice",
                table: "StudentCourses",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedCoursePrice",
                table: "StudentCourses");
        }
    }
}
