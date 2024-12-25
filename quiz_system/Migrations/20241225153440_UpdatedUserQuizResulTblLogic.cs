using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace quiz_system.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserQuizResulTblLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuizAttemptId",
                table: "UserQuizResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "155a39b8-f44d-4e9d-8a60-3af8c3453e1d",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dda0a0e7-e915-483c-aa3a-c4959ae2db5c", "AQAAAAIAAYagAAAAEAjXdAtSN0gwOvRSGkvgIZa+BAHBvPRZMGQ0TWLPZ8jW4h59h349txUntpD6PnP+IA==", "7da5599d-7afa-4048-b7fd-2dd015b35a81" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2e8cb337-0be4-4f90-b4ab-8d447ad168a3",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "91af29a5-051f-447f-a66e-045f84579b60", "AQAAAAIAAYagAAAAEITKBrXqP2SHurb0NcI0DlIVBrGbi80TcSws4QRtZuzXadNbguRjM2VpcjwWU1hNQQ==", "423f940f-0e07-423f-b3a6-77dbf6b63a53" });

            migrationBuilder.CreateIndex(
                name: "IX_UserQuizResults_QuizAttemptId",
                table: "UserQuizResults",
                column: "QuizAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_SectionId",
                table: "QuizAttempts",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserQuizResults_QuizAttempts_QuizAttemptId",
                table: "UserQuizResults",
                column: "QuizAttemptId",
                principalTable: "QuizAttempts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserQuizResults_QuizAttempts_QuizAttemptId",
                table: "UserQuizResults");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropIndex(
                name: "IX_UserQuizResults_QuizAttemptId",
                table: "UserQuizResults");

            migrationBuilder.DropColumn(
                name: "QuizAttemptId",
                table: "UserQuizResults");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "155a39b8-f44d-4e9d-8a60-3af8c3453e1d",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dd964fc1-b136-4e78-9af2-f9bc07cdd355", "AQAAAAIAAYagAAAAEDjZVNO4azRVszo/PtbCCa+Gw6QzPqSVMdKc/ok9sS/sligzoQxLtXqF2bKyqG4O3g==", "6f050aab-d3b4-451c-8fff-6996c9d8afbe" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2e8cb337-0be4-4f90-b4ab-8d447ad168a3",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ad3b5ab8-90f4-420c-9044-520856de466d", "AQAAAAIAAYagAAAAEDPsNgJrXK5ihw8xOjlcS527NozWHTD8Fo1V968o/oj4mXyM6tjTTSWXUIIfxby2ag==", "1e81a234-67cc-456d-9851-7469fccb2ce2" });
        }
    }
}
