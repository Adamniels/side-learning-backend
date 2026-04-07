using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SideLearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameSessionSubjectAreasAndExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "session_topics",
                newName: "session_subject_areas");

            migrationBuilder.RenameColumn(
                name: "Topic",
                table: "session_subject_areas",
                newName: "SubjectArea");

            migrationBuilder.RenameIndex(
                name: "IX_session_topics_SessionId",
                table: "session_subject_areas",
                newName: "IX_session_subject_areas_SessionId");

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "sessions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropPrimaryKey(
                name: "PK_session_topics",
                table: "session_subject_areas");

            migrationBuilder.AddPrimaryKey(
                name: "PK_session_subject_areas",
                table: "session_subject_areas",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_session_subject_areas",
                table: "session_subject_areas");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "sessions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_session_topics",
                table: "session_subject_areas",
                column: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_session_subject_areas_SessionId",
                table: "session_subject_areas",
                newName: "IX_session_topics_SessionId");

            migrationBuilder.RenameColumn(
                name: "SubjectArea",
                table: "session_subject_areas",
                newName: "Topic");

            migrationBuilder.RenameTable(
                name: "session_subject_areas",
                newName: "session_topics");
        }
    }
}
