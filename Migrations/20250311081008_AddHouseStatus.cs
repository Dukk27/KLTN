using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropTable(
            //     name: "Payment");

            // migrationBuilder.DropTable(
            //     name: "UserPost");

            // migrationBuilder.AddColumn<int>(
            //     name: "Status",
            //     table: "Houses",
            //     type: "int",
            //     nullable: false,
            //     defaultValue: 0);

            // migrationBuilder.CreateTable(
            //     name: "Payments",
            //     columns: table => new
            //     {
            //         PaymentId = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         UserId = table.Column<int>(type: "int", nullable: false),
            //         HouseId = table.Column<int>(type: "int", nullable: false),
            //         Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
            //         PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Payments", x => x.PaymentId);
            //         table.ForeignKey(
            //             name: "FK_Payments_Accounts_UserId",
            //             column: x => x.UserId,
            //             principalTable: "Accounts",
            //             principalColumn: "IdUser",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "FK_Payments_Houses_HouseId",
            //             column: x => x.HouseId,
            //             principalTable: "Houses",
            //             principalColumn: "IdHouse",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "UserPosts",
            //     columns: table => new
            //     {
            //         UserPostId = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         UserId = table.Column<int>(type: "int", nullable: false),
            //         HouseId = table.Column<int>(type: "int", nullable: false),
            //         IsFree = table.Column<bool>(type: "bit", nullable: false),
            //         PostDate = table.Column<DateTime>(type: "datetime2", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_UserPosts", x => x.UserPostId);
            //         table.ForeignKey(
            //             name: "FK_UserPosts_Accounts_UserId",
            //             column: x => x.UserId,
            //             principalTable: "Accounts",
            //             principalColumn: "IdUser",
            //             onDelete: ReferentialAction.Cascade);
            //         table.ForeignKey(
            //             name: "FK_UserPosts_Houses_HouseId",
            //             column: x => x.HouseId,
            //             principalTable: "Houses",
            //             principalColumn: "IdHouse",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_Payments_HouseId",
            //     table: "Payments",
            //     column: "HouseId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_Payments_UserId",
            //     table: "Payments",
            //     column: "UserId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_UserPosts_HouseId",
            //     table: "UserPosts",
            //     column: "HouseId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_UserPosts_UserId",
            //     table: "UserPosts",
            //     column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        //     migrationBuilder.DropTable(
        //         name: "Payments");

        //     migrationBuilder.DropTable(
        //         name: "UserPosts");

        //     migrationBuilder.DropColumn(
        //         name: "Status",
        //         table: "Houses");

        //     migrationBuilder.CreateTable(
        //         name: "Payment",
        //         columns: table => new
        //         {
        //             PaymentId = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             HouseId = table.Column<int>(type: "int", nullable: false),
        //             UserId = table.Column<int>(type: "int", nullable: false),
        //             Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        //             PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
        //             PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
        //             TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_Payment", x => x.PaymentId);
        //             table.ForeignKey(
        //                 name: "FK_Payment_Accounts_UserId",
        //                 column: x => x.UserId,
        //                 principalTable: "Accounts",
        //                 principalColumn: "IdUser",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_Payment_Houses_HouseId",
        //                 column: x => x.HouseId,
        //                 principalTable: "Houses",
        //                 principalColumn: "IdHouse",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateTable(
        //         name: "UserPost",
        //         columns: table => new
        //         {
        //             UserPostId = table.Column<int>(type: "int", nullable: false)
        //                 .Annotation("SqlServer:Identity", "1, 1"),
        //             HouseId = table.Column<int>(type: "int", nullable: false),
        //             UserId = table.Column<int>(type: "int", nullable: false),
        //             IsFree = table.Column<bool>(type: "bit", nullable: false),
        //             PostDate = table.Column<DateTime>(type: "datetime2", nullable: false)
        //         },
        //         constraints: table =>
        //         {
        //             table.PrimaryKey("PK_UserPost", x => x.UserPostId);
        //             table.ForeignKey(
        //                 name: "FK_UserPost_Accounts_UserId",
        //                 column: x => x.UserId,
        //                 principalTable: "Accounts",
        //                 principalColumn: "IdUser",
        //                 onDelete: ReferentialAction.Cascade);
        //             table.ForeignKey(
        //                 name: "FK_UserPost_Houses_HouseId",
        //                 column: x => x.HouseId,
        //                 principalTable: "Houses",
        //                 principalColumn: "IdHouse",
        //                 onDelete: ReferentialAction.Cascade);
        //         });

        //     migrationBuilder.CreateIndex(
        //         name: "IX_Payment_HouseId",
        //         table: "Payment",
        //         column: "HouseId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_Payment_UserId",
        //         table: "Payment",
        //         column: "UserId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_UserPost_HouseId",
        //         table: "UserPost",
        //         column: "HouseId");

        //     migrationBuilder.CreateIndex(
        //         name: "IX_UserPost_UserId",
        //         table: "UserPost",
        //         column: "UserId");
        }
    }
}
