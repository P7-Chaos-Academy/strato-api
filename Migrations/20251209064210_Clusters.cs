using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace stratoapi.Migrations
{
    /// <inheritdoc />
    public partial class Clusters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApiEndpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrometheusEndpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clusters_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Clusters_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 178, 154, 20, 230, 163, 109, 208, 248, 3, 131, 3, 249, 215, 248, 224, 202, 186, 204, 182, 249, 72, 104, 94, 20, 206, 228, 53, 19, 142, 37, 70, 0, 193, 32, 166, 190, 55, 13, 246, 1, 156, 152, 1, 164, 224, 31, 181, 82, 101, 110, 166, 112, 218, 101, 132, 220, 141, 243, 16, 147, 11, 86, 67, 29 }, new byte[] { 231, 132, 126, 230, 33, 194, 32, 243, 184, 33, 38, 195, 64, 213, 10, 67, 216, 36, 22, 182, 57, 232, 33, 231, 232, 41, 183, 62, 150, 193, 184, 165, 196, 224, 146, 238, 234, 55, 88, 50, 210, 42, 166, 134, 48, 21, 246, 183, 73, 227, 106, 22, 98, 125, 201, 214, 214, 47, 231, 91, 165, 81, 158, 249, 231, 243, 39, 196, 33, 199, 98, 206, 47, 85, 67, 18, 240, 87, 237, 90, 13, 143, 60, 151, 160, 20, 142, 157, 195, 251, 78, 176, 29, 35, 189, 36, 149, 80, 157, 132, 185, 85, 21, 122, 29, 168, 12, 249, 164, 150, 246, 208, 0, 122, 72, 166, 172, 190, 99, 195, 185, 210, 1, 67, 208, 74, 209, 203 } });

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_CreatedBy",
                table: "Clusters",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Clusters_UpdatedBy",
                table: "Clusters",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clusters");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 183, 86, 28, 85, 10, 113, 41, 249, 255, 49, 122, 165, 242, 44, 131, 127, 219, 155, 12, 159, 175, 218, 231, 41, 179, 152, 117, 97, 134, 18, 206, 248, 124, 91, 64, 7, 190, 119, 56, 183, 129, 30, 67, 204, 220, 80, 57, 66, 230, 228, 93, 165, 10, 142, 72, 48, 87, 245, 221, 64, 8, 68, 228, 76 }, new byte[] { 116, 25, 200, 148, 4, 9, 109, 205, 95, 14, 67, 61, 122, 229, 132, 92, 117, 153, 30, 145, 114, 253, 171, 43, 233, 79, 16, 198, 66, 198, 149, 144, 189, 174, 181, 41, 221, 177, 11, 26, 135, 144, 240, 162, 133, 61, 64, 38, 221, 238, 109, 147, 79, 227, 33, 141, 252, 238, 75, 165, 141, 131, 66, 114, 202, 160, 218, 79, 161, 56, 122, 201, 237, 1, 46, 255, 195, 200, 211, 207, 185, 241, 72, 82, 122, 46, 90, 41, 204, 204, 75, 178, 72, 120, 213, 40, 152, 85, 212, 8, 232, 205, 229, 241, 78, 164, 247, 238, 160, 218, 74, 76, 142, 95, 226, 196, 199, 17, 179, 70, 16, 209, 107, 39, 43, 24, 6, 156 } });
        }
    }
}
