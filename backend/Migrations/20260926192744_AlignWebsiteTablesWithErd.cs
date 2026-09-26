using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AlignWebsiteTablesWithErd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_website_sys_user_sys_user_id",
                schema: "public",
                table: "user_website");

            migrationBuilder.Sql("ALTER TABLE public.sys_user ALTER COLUMN sys_user_uuid TYPE varchar USING sys_user_uuid::text;");
            migrationBuilder.Sql("ALTER TABLE public.user_website ALTER COLUMN user_website_uuid TYPE varchar USING user_website_uuid::text;");

            migrationBuilder.AlterColumn<long>(
                name: "sys_user_id",
                schema: "public",
                table: "user_website",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "user_website_id",
                schema: "public",
                table: "user_website",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "sys_user_uuid",
                schema: "public",
                table: "sys_user",
                type: "varchar",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "sys_user_id",
                schema: "public",
                table: "sys_user",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("ALTER SEQUENCE public.sys_user_sys_user_id_seq AS bigint;");
            migrationBuilder.Sql("ALTER SEQUENCE public.user_website_user_website_id_seq AS bigint;");

            migrationBuilder.AddForeignKey(
                name: "FK_user_website_sys_user_sys_user_id",
                schema: "public",
                table: "user_website",
                column: "sys_user_id",
                principalSchema: "public",
                principalTable: "sys_user",
                principalColumn: "sys_user_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateTable(
                name: "user_website_credential",
                schema: "public",
                columns: table => new
                {
                    user_website_credential_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_website_credential_uuid = table.Column<string>(type: "varchar", nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    user_website_credential_username_fer = table.Column<string>(type: "varchar", nullable: false),
                    user_website_credential_username_sha = table.Column<string>(type: "varchar(64)", nullable: false),
                    user_website_credential_password_fer = table.Column<string>(type: "varchar", nullable: false),
                    user_website_credential_password_sha = table.Column<string>(type: "varchar(64)", nullable: false),
                    user_website_id = table.Column<long>(type: "bigint", nullable: false),
                    is_secure = table.Column<bool>(type: "boolean", nullable: true),
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
                    user_website_link_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_website_link_uuid = table.Column<string>(type: "varchar", nullable: false, defaultValueSql: "gen_random_uuid()::text"),
                    user_website_link_url = table.Column<string>(type: "varchar", nullable: false),
                    user_website_id = table.Column<long>(type: "bigint", nullable: false),
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

            migrationBuilder.DropForeignKey(
                name: "FK_user_website_sys_user_sys_user_id",
                schema: "public",
                table: "user_website");

            migrationBuilder.Sql("ALTER TABLE public.sys_user ALTER COLUMN sys_user_uuid TYPE uuid USING sys_user_uuid::uuid;");
            migrationBuilder.Sql("ALTER TABLE public.user_website ALTER COLUMN user_website_uuid TYPE uuid USING user_website_uuid::uuid;");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_website_uuid",
                schema: "public",
                table: "user_website",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AlterColumn<int>(
                name: "sys_user_id",
                schema: "public",
                table: "user_website",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "user_website_id",
                schema: "public",
                table: "user_website",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "sys_user_uuid",
                schema: "public",
                table: "sys_user",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar");

            migrationBuilder.AlterColumn<int>(
                name: "sys_user_id",
                schema: "public",
                table: "sys_user",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("ALTER SEQUENCE public.sys_user_sys_user_id_seq AS integer;");
            migrationBuilder.Sql("ALTER SEQUENCE public.user_website_user_website_id_seq AS integer;");

            migrationBuilder.AddForeignKey(
                name: "FK_user_website_sys_user_sys_user_id",
                schema: "public",
                table: "user_website",
                column: "sys_user_id",
                principalSchema: "public",
                principalTable: "sys_user",
                principalColumn: "sys_user_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
