using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Migrations
{
    /// <inheritdoc />
    public partial class FixReportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "Reports",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         UserId = table.Column<int>(type: "int", nullable: false),
            //         HouseId = table.Column<int>(type: "int", nullable: false),
            //         Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
            //         IsApproved = table.Column<bool>(type: "bit", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Reports", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_Reports_Accounts_UserId",
            //             column: x => x.UserId,
            //             principalTable: "Accounts",
            //             principalColumn: "IdUser",
            //             onDelete: ReferentialAction.NoAction);
            //         table.ForeignKey(
            //             name: "FK_Reports_Houses_HouseId",
            //             column: x => x.HouseId,
            //             principalTable: "Houses",
            //             principalColumn: "IdHouse",
            //             onDelete: ReferentialAction.NoAction);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_Reports_HouseId",
            //     table: "Reports",
            //     column: "HouseId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Reports_UserId",
            //     table: "Reports",
            //     column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
