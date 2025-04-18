using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Migrations
{
    /// <inheritdoc />
    public partial class AddLatLngToHouseDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AddColumn<double>(
            //     name: "Latitude",
            //     table: "HouseDetails",
            //     type: "float",
            //     nullable: true);

            // migrationBuilder.AddColumn<double>(
            //     name: "Longitude",
            //     table: "HouseDetails",
            //     type: "float",
            //     nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "HouseDetails");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "HouseDetails");
        }
    }
}
