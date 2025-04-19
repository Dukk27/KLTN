using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Migrations
{
    /// <inheritdoc />
    public partial class AddIsNotifiedToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNotified",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNotified",
                table: "Appointments");
        }
    }
}
