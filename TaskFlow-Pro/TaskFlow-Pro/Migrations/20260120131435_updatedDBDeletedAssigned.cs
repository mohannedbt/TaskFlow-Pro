using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow_Pro.Migrations
{
    /// <inheritdoc />
    public partial class updatedDBDeletedAssigned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tasks_TaskItemId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TaskItemId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TaskItemId",
                table: "AspNetUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaskItemId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TaskItemId",
                table: "AspNetUsers",
                column: "TaskItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tasks_TaskItemId",
                table: "AspNetUsers",
                column: "TaskItemId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
