using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskPilot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addcolumncolorCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorCode",
                table: "Statuses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorCode",
                table: "Statuses");
        }
    }
}
