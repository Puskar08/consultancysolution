using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace consultancysolution.Migrations
{
    /// <inheritdoc />
    public partial class addingcolumnsinstudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Students",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Students");
        }
    }
}
