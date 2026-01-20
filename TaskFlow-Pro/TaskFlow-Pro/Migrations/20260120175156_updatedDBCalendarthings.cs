using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow_Pro.Migrations
{
    /// <inheritdoc />
    public partial class updatedDBCalendarthings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CompactCalendarMode",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DefaultCalendarView",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "DueDateReminders",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailNotifications",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TaskAssignmentAlerts",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "AspNetUsers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompactCalendarMode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DefaultCalendarView",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DueDateReminders",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailNotifications",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TaskAssignmentAlerts",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Theme",
                table: "AspNetUsers");
        }
    }
}
