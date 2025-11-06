using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace stratoapi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedUserPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "PasswordHash", "PasswordSalt" },
                values: new object[] { "seed@example.com", new byte[] { 217, 242, 131, 147, 14, 138, 55, 19, 236, 25, 116, 175, 118, 49, 101, 111, 69, 35, 217, 141, 2, 232, 4, 103, 227, 19, 64, 221, 28, 183, 21, 126, 51, 179, 38, 45, 219, 230, 177, 214, 204, 96, 96, 13, 121, 192, 237, 63, 134, 232, 220, 210, 145, 61, 145, 101, 159, 95, 224, 192, 211, 6, 17, 133 }, new byte[] { 90, 225, 51, 222, 186, 204, 140, 229, 222, 216, 2, 42, 23, 219, 44, 195, 153, 226, 172, 187, 200, 18, 82, 161, 46, 83, 117, 242, 77, 232, 120, 215, 1, 171, 227, 195, 70, 11, 34, 13, 114, 81, 144, 150, 82, 170, 74, 224, 120, 9, 255, 174, 199, 215, 71, 3, 217, 30, 255, 198, 172, 62, 127, 69, 41, 36, 40, 14, 173, 239, 240, 84, 131, 218, 120, 8, 172, 108, 134, 80, 123, 161, 188, 94, 179, 207, 112, 244, 190, 181, 227, 55, 185, 13, 152, 217, 120, 227, 42, 63, 70, 224, 181, 88, 83, 59, 141, 16, 137, 102, 237, 78, 143, 9, 38, 204, 129, 196, 111, 139, 127, 16, 184, 74, 140, 2, 119, 133 } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "PasswordHash", "PasswordSalt" },
                values: new object[] { "", new byte[] { 0, 31, 164, 182, 134, 17, 250, 14, 156, 249, 149, 193, 69, 80, 248, 26, 198, 50, 233, 38, 217, 69, 33, 13, 96, 63, 43, 234, 77, 83, 144, 10, 175, 6, 39, 23, 241, 125, 124, 37, 107, 228, 48, 128, 240, 62, 197, 80, 164, 181, 100, 69, 135, 10, 212, 48, 49, 4, 205, 65, 227, 105, 230, 204 }, new byte[] { 0 } });
        }
    }
}
