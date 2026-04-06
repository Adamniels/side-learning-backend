using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SideLearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Goal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    EstimatedDurationInMinutes = table.Column<int>(type: "integer", nullable: true),
                    ContextExplanation = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ContextWhyItMatters = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ContextYoutubeUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    ContextAdditionalResources = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    HandsOnInstructions = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    HandsOnExpectedOutput = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ReflectionSolution = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ReflectionText = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ReflectionNotes = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ReflectionDifficultyFeedback = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sessions_domain_users_UserId",
                        column: x => x.UserId,
                        principalTable: "domain_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session_topics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Topic = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_session_topics_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_session_topics_SessionId",
                table: "session_topics",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_UserId",
                table: "sessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "session_topics");

            migrationBuilder.DropTable(
                name: "sessions");
        }
    }
}
