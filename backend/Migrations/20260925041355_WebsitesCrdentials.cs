using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class WebsitesCrdentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_website_credential",
                schema: "public",
                columns: table => new
                {
                    user_website_credential_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_website_credential_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_website_credential_username_fer = table.Column<string>(type: "varchar", nullable: false),
                    user_website_credential_username_sha = table.Column<string>(type: "varchar", nullable: false),
                    user_website_credential_password_fer = table.Column<string>(type: "varchar", nullable: false),
                    user_website_credential_password_sha = table.Column<string>(type: "varchar", nullable: false),
                    user_website_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_website_credential", x => x.user_website_credential_id);
                    table.ForeignKey(
                        name: "FK_user_website_credential_user_website_user_website_id",
                        column: x => x.user_website_id,
                        principalSchema: "public",
                        principalTable: "user_website",
                        principalColumn: "user_website_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_website_link",
                schema: "public",
                columns: table => new
                {
                    user_website_link_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_website_link_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_website_link_url = table.Column<string>(type: "varchar(240)", nullable: false),
                    user_website_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_website_link", x => x.user_website_link_id);
                    table.ForeignKey(
                        name: "FK_user_website_link_user_website_user_website_id",
                        column: x => x.user_website_id,
                        principalSchema: "public",
                        principalTable: "user_website",
                        principalColumn: "user_website_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_website_credential_user_website_id",
                schema: "public",
                table: "user_website_credential",
                column: "user_website_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_website_link_user_website_id",
                schema: "public",
                table: "user_website_link",
                column: "user_website_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_website_credential",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_website_link",
                schema: "public");
        }
    }
}
