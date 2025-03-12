using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KLTN.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    idUser = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userName = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    password = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    Role = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(
                        type: "nvarchar(15)",
                        maxLength: 15,
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Account__3717C98210B6EA83", x => x.idUser);
                }
            );

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    idAmenity = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(
                        type: "varchar(100)",
                        unicode: false,
                        maxLength: 100,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Amenitie__C1B0D589CDE1290A", x => x.idAmenity);
                }
            );

            migrationBuilder.CreateTable(
                name: "House",
                columns: table => new
                {
                    idHouse = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nameHouse = table.Column<string>(
                        type: "nvarchar(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    category = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    idUser = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__House__AF515CBF563343EC", x => x.idHouse);
                    table.ForeignKey(
                        name: "FK_House_Account",
                        column: x => x.idUser,
                        principalTable: "Account",
                        principalColumn: "idUser"
                    );
                }
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

            migrationBuilder.CreateTable(
                name: "HouseDetail",
                columns: table => new
                {
                    idHouseDetail = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idHouse = table.Column<int>(type: "int", nullable: false),
                    address = table.Column<string>(
                        type: "nvarchar(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    dienTich = table.Column<double>(type: "float", nullable: false),
                    tienDien = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    tienNuoc = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    tienDV = table.Column<string>(
                        type: "nvarchar(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    describe = table.Column<string>(
                        type: "nvarchar(500)",
                        maxLength: 500,
                        nullable: true
                    ),
                    status = table.Column<string>(
                        type: "nvarchar(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    image = table.Column<string>(
                        type: "nvarchar(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    timePost = table.Column<DateTime>(type: "datetime", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HouseDet__946F757FD8C7B0A6", x => x.idHouseDetail);
                    table.ForeignKey(
                        name: "FK_HouseDetail_House",
                        column: x => x.idHouse,
                        principalTable: "House",
                        principalColumn: "idHouse"
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "Review",
                columns: table => new
                {
                    idReview = table
                        .Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idUser = table.Column<int>(type: "int", nullable: false),
                    idHouse = table.Column<int>(type: "int", nullable: true),
                    rating = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "text", maxLength: 500, nullable: false),
                    reviewDate = table.Column<DateTime>(
                        type: "datetime",
                        nullable: true,
                        defaultValueSql: "(getdate())"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Review__04F7FE108ECD5FA1", x => x.idReview);
                    table.ForeignKey(
                        name: "FK__Review__idHouse__66603565",
                        column: x => x.idHouse,
                        principalTable: "House",
                        principalColumn: "idHouse"
                    );
                    table.ForeignKey(
                        name: "FK__Review__idUser__6477ECF3",
                        column: x => x.idUser,
                        principalTable: "Account",
                        principalColumn: "idUser"
                    );
                }
            );

            migrationBuilder.CreateIndex(name: "IX_House_idUser", table: "House", column: "idUser");

            migrationBuilder.CreateIndex(
                name: "IX_HouseAmenities_idAmenity",
                table: "HouseAmenities",
                column: "idAmenity"
            );

            migrationBuilder.CreateIndex(
                name: "IX_HouseDetail_idHouse",
                table: "HouseDetail",
                column: "idHouse"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Review_idHouse",
                table: "Review",
                column: "idHouse"
            );

            migrationBuilder.CreateIndex(
                name: "IX_Review_idUser",
                table: "Review",
                column: "idUser"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HouseAmenities");

            migrationBuilder.DropTable(name: "HouseDetail");

            migrationBuilder.DropTable(name: "Review");

            migrationBuilder.DropTable(name: "Amenities");

            migrationBuilder.DropTable(name: "House");

            migrationBuilder.DropTable(name: "Account");
        }
    }
}
