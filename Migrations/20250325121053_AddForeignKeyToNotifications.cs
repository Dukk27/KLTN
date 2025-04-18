using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyToNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AlterColumn<int>(
            //     name: "UserId",
            //     table: "Notifications",
            //     type: "int",
            //     nullable: false,
            //     oldClrType: typeof(string),
            //     oldType: "nvarchar(max)");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Notifications_UserId",
            //     table: "Notifications",
            //     column: "UserId");

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Notifications_Accounts_UserId",
            //     table: "Notifications",
            //     column: "UserId",
            //     principalTable: "Accounts",
            //     principalColumn: "IdUser",
            //     onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Accounts_UserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
