using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace consultancysolution.Migrations
{
    /// <inheritdoc />
    public partial class addedphotoandpassporturlinstudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassportUrl",
                table: "Students",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePhotoUrl",
                table: "Students",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassportUrl",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoUrl",
                table: "Students");
        }
    }
}
