using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace stratoapi.Migrations
{
    /// <inheritdoc />
    public partial class AddMetricTypeUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MetricTypes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MetricTypes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 76, 57, 149, 93, 204, 7, 95, 114, 8, 60, 50, 198, 242, 32, 234, 255, 39, 123, 163, 71, 75, 200, 58, 243, 187, 27, 10, 76, 199, 168, 153, 9, 206, 187, 211, 239, 89, 136, 30, 222, 227, 92, 250, 169, 144, 149, 13, 14, 225, 52, 199, 159, 233, 200, 127, 213, 50, 106, 89, 206, 178, 100, 37, 93 }, new byte[] { 232, 75, 84, 149, 207, 196, 244, 59, 80, 209, 202, 181, 118, 247, 160, 18, 217, 251, 123, 227, 46, 198, 11, 59, 199, 58, 249, 67, 152, 112, 194, 113, 30, 20, 73, 187, 99, 136, 178, 115, 109, 199, 203, 16, 151, 123, 234, 168, 176, 5, 127, 97, 5, 83, 52, 148, 78, 54, 1, 52, 105, 23, 247, 3, 151, 139, 2, 156, 33, 5, 96, 85, 80, 206, 38, 162, 140, 218, 110, 130, 104, 98, 188, 129, 23, 27, 54, 4, 63, 254, 188, 159, 180, 173, 205, 75, 53, 215, 211, 248, 201, 10, 27, 2, 76, 240, 212, 17, 31, 212, 6, 44, 78, 152, 219, 115, 81, 120, 58, 104, 208, 209, 98, 54, 32, 164, 169, 169 } });

            migrationBuilder.CreateIndex(
                name: "IX_MetricTypes_CreatedBy",
                table: "MetricTypes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MetricTypes_UpdatedBy",
                table: "MetricTypes",
                column: "UpdatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_MetricTypes_Users_CreatedBy",
                table: "MetricTypes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MetricTypes_Users_UpdatedBy",
                table: "MetricTypes",
                column: "UpdatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MetricTypes_Users_CreatedBy",
                table: "MetricTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_MetricTypes_Users_UpdatedBy",
                table: "MetricTypes");

            migrationBuilder.DropIndex(
                name: "IX_MetricTypes_CreatedBy",
                table: "MetricTypes");

            migrationBuilder.DropIndex(
                name: "IX_MetricTypes_UpdatedBy",
                table: "MetricTypes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MetricTypes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MetricTypes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "PasswordHash", "PasswordSalt" },
                values: new object[] { new byte[] { 1, 245, 150, 104, 218, 18, 191, 109, 215, 131, 79, 254, 147, 19, 237, 82, 42, 170, 174, 161, 108, 35, 233, 35, 9, 159, 45, 37, 130, 136, 54, 221, 207, 69, 59, 37, 204, 38, 164, 247, 153, 174, 134, 135, 53, 220, 214, 19, 92, 52, 46, 82, 76, 163, 220, 91, 134, 75, 213, 27, 44, 174, 250, 51 }, new byte[] { 241, 164, 169, 122, 220, 136, 201, 206, 242, 107, 168, 34, 191, 37, 52, 246, 34, 20, 183, 147, 105, 190, 95, 99, 107, 198, 189, 93, 105, 184, 170, 246, 81, 32, 168, 65, 148, 51, 154, 233, 83, 127, 105, 168, 154, 173, 33, 147, 5, 116, 93, 121, 15, 90, 98, 179, 63, 230, 140, 29, 6, 46, 102, 145, 100, 214, 239, 8, 34, 62, 225, 198, 194, 48, 211, 146, 104, 153, 128, 237, 8, 212, 132, 226, 86, 92, 41, 253, 34, 141, 87, 178, 245, 179, 78, 81, 154, 150, 134, 177, 202, 239, 9, 88, 142, 224, 160, 193, 86, 212, 45, 47, 132, 60, 35, 9, 44, 253, 211, 152, 53, 243, 231, 44, 156, 232, 243, 79 } });
        }
    }
}
