using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineExamPortalFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddPercentageToReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Percentage",
                table: "Reports",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "Reports");
        }
    }
}
