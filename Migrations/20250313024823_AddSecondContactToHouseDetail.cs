using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondContactToHouseDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<string>(
            //     name: "ContactName2",
            //     table: "HouseDetails",
            //     type: "nvarchar(max)",
            //     nullable: true
            // );

            // migrationBuilder.AddColumn<string>(
            //     name: "ContactPhone2",
            //     table: "HouseDetails",
            //     type: "nvarchar(max)",
            //     nullable: true
            // );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Accounts_UserId",
                table: "Appointments"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Houses_HouseId",
                table: "Appointments"
            );

            migrationBuilder.DropColumn(name: "ContactName2", table: "HouseDetails");

            migrationBuilder.DropColumn(name: "ContactPhone2", table: "HouseDetails");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Accounts_UserId",
                table: "Appointments",
                column: "UserId",
                principalTable: "Accounts",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Houses_HouseId",
                table: "Appointments",
                column: "HouseId",
                principalTable: "Houses",
                principalColumn: "IdHouse",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
