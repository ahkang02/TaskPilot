using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskPilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addcolumncolorCodepriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                table: "Priorities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorCode",
                table: "Priorities");
        }
    }
}
