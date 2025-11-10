using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace stratoapi.Migrations
{
    /// <inheritdoc />
    public partial class Metrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MetricTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PrometheusIdentifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedBy = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<int>(type: "integer", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricTypes", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 1, 245, 150, 104, 218, 18, 191, 109, 215, 131, 79, 254, 147, 19, 237, 82, 42, 170, 174, 161, 108, 35, 233, 35, 9, 159, 45, 37, 130, 136, 54, 221, 207, 69, 59, 37, 204, 38, 164, 247, 153, 174, 134, 135, 53, 220, 214, 19, 92, 52, 46, 82, 76, 163, 220, 91, 134, 75, 213, 27, 44, 174, 250, 51 }, new byte[] { 241, 164, 169, 122, 220, 136, 201, 206, 242, 107, 168, 34, 191, 37, 52, 246, 34, 20, 183, 147, 105, 190, 95, 99, 107, 198, 189, 93, 105, 184, 170, 246, 81, 32, 168, 65, 148, 51, 154, 233, 83, 127, 105, 168, 154, 173, 33, 147, 5, 116, 93, 121, 15, 90, 98, 179, 63, 230, 140, 29, 6, 46, 102, 145, 100, 214, 239, 8, 34, 62, 225, 198, 194, 48, 211, 146, 104, 153, 128, 237, 8, 212, 132, 226, 86, 92, 41, 253, 34, 141, 87, 178, 245, 179, 78, 81, 154, 150, 134, 177, 202, 239, 9, 88, 142, 224, 160, 193, 86, 212, 45, 47, 132, 60, 35, 9, 44, 253, 211, 152, 53, 243, 231, 44, 156, 232, 243, 79 } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetricTypes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 217, 242, 131, 147, 14, 138, 55, 19, 236, 25, 116, 175, 118, 49, 101, 111, 69, 35, 217, 141, 2, 232, 4, 103, 227, 19, 64, 221, 28, 183, 21, 126, 51, 179, 38, 45, 219, 230, 177, 214, 204, 96, 96, 13, 121, 192, 237, 63, 134, 232, 220, 210, 145, 61, 145, 101, 159, 95, 224, 192, 211, 6, 17, 133 }, new byte[] { 90, 225, 51, 222, 186, 204, 140, 229, 222, 216, 2, 42, 23, 219, 44, 195, 153, 226, 172, 187, 200, 18, 82, 161, 46, 83, 117, 242, 77, 232, 120, 215, 1, 171, 227, 195, 70, 11, 34, 13, 114, 81, 144, 150, 82, 170, 74, 224, 120, 9, 255, 174, 199, 215, 71, 3, 217, 30, 255, 198, 172, 62, 127, 69, 41, 36, 40, 14, 173, 239, 240, 84, 131, 218, 120, 8, 172, 108, 134, 80, 123, 161, 188, 94, 179, 207, 112, 244, 190, 181, 227, 55, 185, 13, 152, 217, 120, 227, 42, 63, 70, 224, 181, 88, 83, 59, 141, 16, 137, 102, 237, 78, 143, 9, 38, 204, 129, 196, 111, 139, 127, 16, 184, 74, 140, 2, 119, 133 } });
        }
    }
}
