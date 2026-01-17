using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow_Pro.Migrations
{
    /// <inheritdoc />
    public partial class migration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Tasks_ToTaskId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "ToTaskId",
                table: "Comments",
                newName: "ToTaskItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ToTaskId",
                table: "Comments",
                newName: "IX_Comments_ToTaskItemId");

            migrationBuilder.AddColumn<string>(
                name: "AssignedToId",
                table: "Tasks",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tasks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_AssignedToId",
                table: "Tasks",
                column: "AssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Tasks_ToTaskItemId",
                table: "Comments",
                column: "ToTaskItemId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_AssignedToId",
                table: "Tasks",
                column: "AssignedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Tasks_ToTaskItemId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_AssignedToId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_AssignedToId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tasks");

            migrationBuilder.RenameColumn(
                name: "ToTaskItemId",
                table: "Comments",
                newName: "ToTaskId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_ToTaskItemId",
                table: "Comments",
                newName: "IX_Comments_ToTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Tasks_ToTaskId",
                table: "Comments",
                column: "ToTaskId",
                principalTable: "Tasks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
