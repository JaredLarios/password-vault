using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "sys_user",
                schema: "public",
                columns: table => new
                {
                    sys_user_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sys_user_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    sys_user_username_fer = table.Column<string>(type: "varchar(420)", nullable: false),
                    sys_user_username_sha = table.Column<string>(type: "varchar(64)", nullable: false),
                    sys_user_name_fer = table.Column<string>(type: "varchar(420)", nullable: true),
                    sys_user_last_name_fer = table.Column<string>(type: "varchar(420)", nullable: true),
                    sys_user_password = table.Column<string>(type: "varchar(128)", nullable: false),
                    is_temporal = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sys_user", x => x.sys_user_id);
                });

            migrationBuilder.CreateTable(
                name: "user_website",
                schema: "public",
                columns: table => new
                {
                    user_website_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_website_uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_website_name = table.Column<string>(type: "varchar", nullable: false),
                    sys_user_id = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_website", x => x.user_website_id);
                    table.ForeignKey(
                        name: "FK_user_website_sys_user_sys_user_id",
                        column: x => x.sys_user_id,
                        principalSchema: "public",
                        principalTable: "sys_user",
                        principalColumn: "sys_user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_website_sys_user_id",
                schema: "public",
                table: "user_website",
                column: "sys_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_website",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sys_user",
                schema: "public");
        }
    }
}
