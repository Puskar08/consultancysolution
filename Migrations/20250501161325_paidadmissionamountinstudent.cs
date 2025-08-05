using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace consultancysolution.Migrations
{
    /// <inheritdoc />
    public partial class paidadmissionamountinstudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAdmissionAmount",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Grade",
                table: "StudentCourses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAdmissionAmount",
                table: "Students");

            migrationBuilder.AlterColumn<string>(
                name: "Grade",
                table: "StudentCourses",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
