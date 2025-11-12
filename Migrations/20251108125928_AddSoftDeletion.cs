using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace stratoapi.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MetricTypes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "IsDeleted", "PasswordHash", "PasswordSalt" },
                values: new object[] { false, new byte[] { 229, 151, 129, 199, 101, 184, 113, 85, 185, 231, 201, 87, 208, 114, 108, 201, 98, 43, 78, 231, 189, 186, 182, 97, 214, 6, 123, 7, 104, 45, 204, 26, 141, 149, 17, 208, 255, 42, 193, 45, 59, 62, 62, 7, 145, 240, 127, 222, 185, 144, 104, 119, 166, 254, 108, 146, 137, 207, 104, 91, 5, 133, 145, 128 }, new byte[] { 225, 103, 2, 229, 130, 229, 139, 22, 55, 11, 180, 111, 251, 7, 12, 66, 179, 3, 254, 97, 126, 226, 136, 59, 31, 103, 49, 218, 195, 25, 118, 110, 98, 112, 135, 20, 115, 185, 251, 252, 105, 119, 74, 197, 202, 111, 193, 7, 170, 9, 19, 72, 212, 182, 186, 57, 14, 233, 248, 113, 160, 17, 166, 204, 2, 236, 253, 187, 60, 2, 164, 202, 175, 6, 207, 141, 164, 158, 12, 146, 70, 41, 195, 123, 87, 96, 144, 198, 64, 146, 250, 146, 10, 148, 239, 198, 240, 99, 52, 222, 17, 79, 104, 84, 167, 172, 73, 204, 14, 130, 127, 109, 135, 220, 173, 223, 206, 254, 115, 79, 130, 97, 161, 64, 13, 89, 225, 150 } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MetricTypes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 76, 57, 149, 93, 204, 7, 95, 114, 8, 60, 50, 198, 242, 32, 234, 255, 39, 123, 163, 71, 75, 200, 58, 243, 187, 27, 10, 76, 199, 168, 153, 9, 206, 187, 211, 239, 89, 136, 30, 222, 227, 92, 250, 169, 144, 149, 13, 14, 225, 52, 199, 159, 233, 200, 127, 213, 50, 106, 89, 206, 178, 100, 37, 93 }, new byte[] { 232, 75, 84, 149, 207, 196, 244, 59, 80, 209, 202, 181, 118, 247, 160, 18, 217, 251, 123, 227, 46, 198, 11, 59, 199, 58, 249, 67, 152, 112, 194, 113, 30, 20, 73, 187, 99, 136, 178, 115, 109, 199, 203, 16, 151, 123, 234, 168, 176, 5, 127, 97, 5, 83, 52, 148, 78, 54, 1, 52, 105, 23, 247, 3, 151, 139, 2, 156, 33, 5, 96, 85, 80, 206, 38, 162, 140, 218, 110, 130, 104, 98, 188, 129, 23, 27, 54, 4, 63, 254, 188, 159, 180, 173, 205, 75, 53, 215, 211, 248, 201, 10, 27, 2, 76, 240, 212, 17, 31, 212, 6, 44, 78, 152, 219, 115, 81, 120, 58, 104, 208, 209, 98, 54, 32, 164, 169, 169 } });
        }
    }
}
