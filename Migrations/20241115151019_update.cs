using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Migrations
{
    public partial class update : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_House_Account", table: "House");

            migrationBuilder.DropForeignKey(name: "FK_HouseDetail_House", table: "HouseDetail");

            migrationBuilder.DropForeignKey(name: "FK__Review__idHouse__66603565", table: "Review");

            migrationBuilder.DropForeignKey(name: "FK__Review__idUser__6477ECF3", table: "Review");

            migrationBuilder.DropTable(name: "HouseAmenities");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Amenitie__C1B0D589CDE1290A",
                table: "Amenities"
            );

            migrationBuilder.DropPrimaryKey(name: "PK__Review__04F7FE108ECD5FA1", table: "Review");

            migrationBuilder.DropPrimaryKey(
                name: "PK__HouseDet__946F757FD8C7B0A6",
                table: "HouseDetail"
            );

            migrationBuilder.DropPrimaryKey(name: "PK__House__AF515CBF563343EC", table: "House");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Account__3717C98210B6EA83",
                table: "Account"
            );

            migrationBuilder.DropColumn(name: "category", table: "House");

            migrationBuilder.RenameTable(name: "Review", newName: "Reviews");

            migrationBuilder.RenameTable(name: "HouseDetail", newName: "HouseDetails");

            migrationBuilder.RenameTable(name: "House", newName: "Houses");

            migrationBuilder.RenameTable(name: "Account", newName: "Accounts");

            migrationBuilder.RenameColumn(name: "name", table: "Amenities", newName: "Name");

            migrationBuilder.RenameColumn(
                name: "idAmenity",
                table: "Amenities",
                newName: "IdAmenity"
            );

            migrationBuilder.RenameColumn(
                name: "reviewDate",
                table: "Reviews",
                newName: "ReviewDate"
            );

            migrationBuilder.RenameColumn(name: "rating", table: "Reviews", newName: "Rating");

            migrationBuilder.RenameColumn(name: "idUser", table: "Reviews", newName: "IdUser");

            migrationBuilder.RenameColumn(name: "idHouse", table: "Reviews", newName: "IdHouse");

            migrationBuilder.RenameColumn(name: "content", table: "Reviews", newName: "Content");

            migrationBuilder.RenameColumn(name: "idReview", table: "Reviews", newName: "IdReview");

            migrationBuilder.RenameIndex(
                name: "IX_Review_idUser",
                table: "Reviews",
                newName: "IX_Reviews_IdUser"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Review_idHouse",
                table: "Reviews",
                newName: "IX_Reviews_IdHouse"
            );

            migrationBuilder.RenameColumn(
                name: "timePost",
                table: "HouseDetails",
                newName: "TimePost"
            );

            migrationBuilder.RenameColumn(
                name: "tienNuoc",
                table: "HouseDetails",
                newName: "TienNuoc"
            );

            migrationBuilder.RenameColumn(
                name: "tienDien",
                table: "HouseDetails",
                newName: "TienDien"
            );

            migrationBuilder.RenameColumn(name: "tienDV", table: "HouseDetails", newName: "TienDv");

            migrationBuilder.RenameColumn(name: "status", table: "HouseDetails", newName: "Status");

            migrationBuilder.RenameColumn(name: "price", table: "HouseDetails", newName: "Price");

            migrationBuilder.RenameColumn(name: "image", table: "HouseDetails", newName: "Image");

            migrationBuilder.RenameColumn(
                name: "idHouse",
                table: "HouseDetails",
                newName: "IdHouse"
            );

            migrationBuilder.RenameColumn(
                name: "dienTich",
                table: "HouseDetails",
                newName: "DienTich"
            );

            migrationBuilder.RenameColumn(
                name: "describe",
                table: "HouseDetails",
                newName: "Describe"
            );

            migrationBuilder.RenameColumn(
                name: "address",
                table: "HouseDetails",
                newName: "Address"
            );

            migrationBuilder.RenameColumn(
                name: "idHouseDetail",
                table: "HouseDetails",
                newName: "IdHouseDetail"
            );

            migrationBuilder.RenameIndex(
                name: "IX_HouseDetail_idHouse",
                table: "HouseDetails",
                newName: "IX_HouseDetails_IdHouse"
            );

            migrationBuilder.RenameColumn(name: "nameHouse", table: "Houses", newName: "NameHouse");

            migrationBuilder.RenameColumn(name: "idUser", table: "Houses", newName: "IdUser");

            migrationBuilder.RenameColumn(name: "idHouse", table: "Houses", newName: "IdHouse");

            migrationBuilder.RenameIndex(
                name: "IX_House_idUser",
                table: "Houses",
                newName: "IX_Houses_IdUser"
            );

            migrationBuilder.RenameColumn(name: "userName", table: "Accounts", newName: "UserName");

            migrationBuilder.RenameColumn(name: "password", table: "Accounts", newName: "Password");

            migrationBuilder.RenameColumn(name: "idUser", table: "Accounts", newName: "IdUser");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Amenities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldUnicode: false,
                oldMaxLength: 100
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "ReviewDate",
                table: "Reviews",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Reviews",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldMaxLength: 500
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimePost",
                table: "HouseDetails",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime"
            );

            migrationBuilder.AlterColumn<string>(
                name: "TienNuoc",
                table: "HouseDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<string>(
                name: "TienDien",
                table: "HouseDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<string>(
                name: "TienDv",
                table: "HouseDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20
            );

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "HouseDetails",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "HouseDetails",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "HouseDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255
            );

            migrationBuilder.AlterColumn<string>(
                name: "NameHouse",
                table: "Houses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255
            );

            migrationBuilder.AddColumn<int>(
                name: "HouseTypeId",
                table: "Houses",
                type: "int",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "Accounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_Amenities",
                table: "Amenities",
                column: "IdAmenity"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reviews",
                table: "Reviews",
                column: "IdReview"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_HouseDetails",
                table: "HouseDetails",
                column: "IdHouseDetail"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Houses", table: "Houses", column: "IdHouse");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Accounts",
                table: "Accounts",
                column: "IdUser"
            );

            migrationBuilder.CreateTable(
                name: "AmenityHouse",
                columns: table => new
                {
                    IdAmenitiesIdAmenity = table.Column<int>(type: "int", nullable: false),
                    IdHousesIdHouse = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_AmenityHouse",
                        x => new { x.IdAmenitiesIdAmenity, x.IdHousesIdHouse }
                    );
                    table.ForeignKey(
                        name: "FK_AmenityHouse_Amenities_IdAmenitiesIdAmenity",
                        column: x => x.IdAmenitiesIdAmenity,
                        principalTable: "Amenities",
                        principalColumn: "IdAmenity",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_AmenityHouse_Houses_IdHousesIdHouse",
                        column: x => x.IdHousesIdHouse,
                        principalTable: "Houses",
                        principalColumn: "IdHouse",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "HouseType",
                columns: table => new
                {
                    IdHouseType = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(
                        type: "nvarchar(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HouseType", x => x.IdHouseType);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_Houses_HouseTypeId",
                table: "Houses",
                column: "HouseTypeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_AmenityHouse_IdHousesIdHouse",
                table: "AmenityHouse",
                column: "IdHousesIdHouse"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_HouseDetails_Houses_IdHouse",
                table: "HouseDetails",
                column: "IdHouse",
                principalTable: "Houses",
                principalColumn: "IdHouse",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Houses_Accounts_IdUser",
                table: "Houses",
                column: "IdUser",
                principalTable: "Accounts",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Houses_HouseType_HouseTypeId",
                table: "Houses",
                column: "HouseTypeId",
                principalTable: "HouseType",
                principalColumn: "IdHouseType",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Accounts_IdUser",
                table: "Reviews",
                column: "IdUser",
                principalTable: "Accounts",
                principalColumn: "IdUser",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Houses_IdHouse",
                table: "Reviews",
                column: "IdHouse",
                principalTable: "Houses",
                principalColumn: "IdHouse"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HouseDetails_Houses_IdHouse",
                table: "HouseDetails"
            );

            migrationBuilder.DropForeignKey(name: "FK_Houses_Accounts_IdUser", table: "Houses");

            migrationBuilder.DropForeignKey(
                name: "FK_Houses_HouseType_HouseTypeId",
                table: "Houses"
            );

            migrationBuilder.DropForeignKey(name: "FK_Reviews_Accounts_IdUser", table: "Reviews");

            migrationBuilder.DropForeignKey(name: "FK_Reviews_Houses_IdHouse", table: "Reviews");

            migrationBuilder.DropTable(name: "AmenityHouse");

            migrationBuilder.DropTable(name: "HouseType");

            migrationBuilder.DropPrimaryKey(name: "PK_Amenities", table: "Amenities");

            migrationBuilder.DropPrimaryKey(name: "PK_Reviews", table: "Reviews");

            migrationBuilder.DropPrimaryKey(name: "PK_Houses", table: "Houses");

            migrationBuilder.DropIndex(name: "IX_Houses_HouseTypeId", table: "Houses");

            migrationBuilder.DropPrimaryKey(name: "PK_HouseDetails", table: "HouseDetails");

            migrationBuilder.DropPrimaryKey(name: "PK_Accounts", table: "Accounts");

            migrationBuilder.DropColumn(name: "HouseTypeId", table: "Houses");

            migrationBuilder.RenameTable(name: "Reviews", newName: "Review");

            migrationBuilder.RenameTable(name: "Houses", newName: "House");

            migrationBuilder.RenameTable(name: "HouseDetails", newName: "HouseDetail");

            migrationBuilder.RenameTable(name: "Accounts", newName: "Account");

            migrationBuilder.RenameColumn(name: "Name", table: "Amenities", newName: "name");

            migrationBuilder.RenameColumn(
                name: "IdAmenity",
                table: "Amenities",
                newName: "idAmenity"
            );

            migrationBuilder.RenameColumn(
                name: "ReviewDate",
                table: "Review",
                newName: "reviewDate"
            );

            migrationBuilder.RenameColumn(name: "Rating", table: "Review", newName: "rating");

            migrationBuilder.RenameColumn(name: "IdUser", table: "Review", newName: "idUser");

            migrationBuilder.RenameColumn(name: "IdHouse", table: "Review", newName: "idHouse");

            migrationBuilder.RenameColumn(name: "Content", table: "Review", newName: "content");

            migrationBuilder.RenameColumn(name: "IdReview", table: "Review", newName: "idReview");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_IdUser",
                table: "Review",
                newName: "IX_Review_idUser"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_IdHouse",
                table: "Review",
                newName: "IX_Review_idHouse"
            );

            migrationBuilder.RenameColumn(name: "NameHouse", table: "House", newName: "nameHouse");

            migrationBuilder.RenameColumn(name: "IdUser", table: "House", newName: "idUser");

            migrationBuilder.RenameColumn(name: "IdHouse", table: "House", newName: "idHouse");

            migrationBuilder.RenameIndex(
                name: "IX_Houses_IdUser",
                table: "House",
                newName: "IX_House_idUser"
            );

            migrationBuilder.RenameColumn(
                name: "TimePost",
                table: "HouseDetail",
                newName: "timePost"
            );

            migrationBuilder.RenameColumn(
                name: "TienNuoc",
                table: "HouseDetail",
                newName: "tienNuoc"
            );

            migrationBuilder.RenameColumn(name: "TienDv", table: "HouseDetail", newName: "tienDV");

            migrationBuilder.RenameColumn(
                name: "TienDien",
                table: "HouseDetail",
                newName: "tienDien"
            );

            migrationBuilder.RenameColumn(name: "Status", table: "HouseDetail", newName: "status");

            migrationBuilder.RenameColumn(name: "Price", table: "HouseDetail", newName: "price");

            migrationBuilder.RenameColumn(name: "Image", table: "HouseDetail", newName: "image");

            migrationBuilder.RenameColumn(
                name: "IdHouse",
                table: "HouseDetail",
                newName: "idHouse"
            );

            migrationBuilder.RenameColumn(
                name: "DienTich",
                table: "HouseDetail",
                newName: "dienTich"
            );

            migrationBuilder.RenameColumn(
                name: "Describe",
                table: "HouseDetail",
                newName: "describe"
            );

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "HouseDetail",
                newName: "address"
            );

            migrationBuilder.RenameColumn(
                name: "IdHouseDetail",
                table: "HouseDetail",
                newName: "idHouseDetail"
            );

            migrationBuilder.RenameIndex(
                name: "IX_HouseDetails_IdHouse",
                table: "HouseDetail",
                newName: "IX_HouseDetail_idHouse"
            );

            migrationBuilder.RenameColumn(name: "UserName", table: "Account", newName: "userName");

            migrationBuilder.RenameColumn(name: "Password", table: "Account", newName: "password");

            migrationBuilder.RenameColumn(name: "IdUser", table: "Account", newName: "idUser");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Amenities",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "reviewDate",
                table: "Review",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true
            );

            migrationBuilder.AlterColumn<string>(
                name: "content",
                table: "Review",
                type: "text",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500
            );

            migrationBuilder.AlterColumn<string>(
                name: "nameHouse",
                table: "House",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100
            );

            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "House",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.AlterColumn<DateTime>(
                name: "timePost",
                table: "HouseDetail",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2"
            );

            migrationBuilder.AlterColumn<string>(
                name: "tienNuoc",
                table: "HouseDetail",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AlterColumn<string>(
                name: "tienDV",
                table: "HouseDetail",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AlterColumn<string>(
                name: "tienDien",
                table: "HouseDetail",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50
            );

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "HouseDetail",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)"
            );

            migrationBuilder.AlterColumn<decimal>(
                name: "price",
                table: "HouseDetail",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)"
            );

            migrationBuilder.AlterColumn<string>(
                name: "address",
                table: "HouseDetail",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200
            );

            migrationBuilder.AlterColumn<string>(
                name: "password",
                table: "Account",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK__Amenitie__C1B0D589CDE1290A",
                table: "Amenities",
                column: "idAmenity"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK__Review__04F7FE108ECD5FA1",
                table: "Review",
                column: "idReview"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK__House__AF515CBF563343EC",
                table: "House",
                column: "idHouse"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK__HouseDet__946F757FD8C7B0A6",
                table: "HouseDetail",
                column: "idHouseDetail"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK__Account__3717C98210B6EA83",
                table: "Account",
                column: "idUser"
            );

            migrationBuilder.CreateTable(
                name: "HouseAmenities",
                columns: table => new
                {
                    idHouse = table.Column<int>(type: "int", nullable: false),
                    idAmenity = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK__HouseAme__234A51E7ED05E3B2",
                        x => new { x.idHouse, x.idAmenity }
                    );
                    table.ForeignKey(
                        name: "FK__HouseAmen__idAme__5FB337D6",
                        column: x => x.idAmenity,
                        principalTable: "Amenities",
                        principalColumn: "idAmenity"
                    );
                    table.ForeignKey(
                        name: "FK__HouseAmen__idHou__5EBF139D",
                        column: x => x.idHouse,
                        principalTable: "House",
                        principalColumn: "idHouse"
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_HouseAmenities_idAmenity",
                table: "HouseAmenities",
                column: "idAmenity"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_House_Account",
                table: "House",
                column: "idUser",
                principalTable: "Account",
                principalColumn: "idUser"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_HouseDetail_House",
                table: "HouseDetail",
                column: "idHouse",
                principalTable: "House",
                principalColumn: "idHouse"
            );

            migrationBuilder.AddForeignKey(
                name: "FK__Review__idHouse__66603565",
                table: "Review",
                column: "idHouse",
                principalTable: "House",
                principalColumn: "idHouse"
            );

            migrationBuilder.AddForeignKey(
                name: "FK__Review__idUser__6477ECF3",
                table: "Review",
                column: "idUser",
                principalTable: "Account",
                principalColumn: "idUser"
            );
        }
    }
}
