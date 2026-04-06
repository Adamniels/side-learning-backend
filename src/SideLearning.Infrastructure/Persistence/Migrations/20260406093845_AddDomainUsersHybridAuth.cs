using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SideLearning.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainUsersHybridAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "domain_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSuspended = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SuspendedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_domain_users_AspNetUsers_Id",
                        column: x => x.Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_domain_users_NormalizedEmail",
                table: "domain_users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO domain_users ("Id", "Email", "NormalizedEmail", "DisplayName", "IsActive", "IsSuspended", "CreatedAtUtc", "UpdatedAtUtc", "SuspendedAtUtc")
                SELECT u."Id",
                       COALESCE(u."Email", ''),
                       COALESCE(u."NormalizedEmail", UPPER(COALESCE(u."Email", ''))),
                       COALESCE(u."DisplayName", ''),
                       TRUE,
                       FALSE,
                       NOW(),
                       NULL,
                       NULL
                FROM "AspNetUsers" u
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM domain_users d
                    WHERE d."Id" = u."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_users");
        }
    }
}
