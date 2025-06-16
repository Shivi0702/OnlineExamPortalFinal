using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineExamPortalFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToExamAndIsPassedToResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "Responses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Exams",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "Responses");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Exams");
        }
    }
}
