using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow_Pro.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "TaskUserProgresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WorkspaceId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxMembers = table.Column<int>(type: "int", nullable: false),
                    EmailPattern = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkspaceInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleToGrant = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Used = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkspaceInvites_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_WorkspaceId",
                table: "Teams",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskUserProgresses_WorkspaceId",
                table: "TaskUserProgresses",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_WorkspaceId",
                table: "Tasks",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_WorkspaceId",
                table: "Comments",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_WorkspaceId",
                table: "AspNetUsers",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvites_Code",
                table: "WorkspaceInvites",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkspaceInvites_WorkspaceId",
                table: "WorkspaceInvites",
                column: "WorkspaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Workspaces_WorkspaceId",
                table: "AspNetUsers",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Workspaces_WorkspaceId",
                table: "Comments",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Workspaces_WorkspaceId",
                table: "Tasks",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskUserProgresses_Workspaces_WorkspaceId",
                table: "TaskUserProgresses",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Workspaces_WorkspaceId",
                table: "Teams",
                column: "WorkspaceId",
                principalTable: "Workspaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Workspaces_WorkspaceId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Workspaces_WorkspaceId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Workspaces_WorkspaceId",
                table: "Tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskUserProgresses_Workspaces_WorkspaceId",
                table: "TaskUserProgresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Workspaces_WorkspaceId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "WorkspaceInvites");

            migrationBuilder.DropTable(
                name: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Teams_WorkspaceId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_TaskUserProgresses_WorkspaceId",
                table: "TaskUserProgresses");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_WorkspaceId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Comments_WorkspaceId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_WorkspaceId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "TaskUserProgresses");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "WorkspaceId",
                table: "AspNetUsers");
        }
    }
}
