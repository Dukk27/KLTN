﻿// <auto-generated />
using System;
using KLTN.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace KLTN.Migrations
{
    [DbContext(typeof(KLTNContext))]
    [Migration("20241115151019_update")]
    partial class update
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.36")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("AmenityHouse", b =>
                {
                    b.Property<int>("IdAmenitiesIdAmenity")
                        .HasColumnType("int");

                    b.Property<int>("IdHousesIdHouse")
                        .HasColumnType("int");

                    b.HasKey("IdAmenitiesIdAmenity", "IdHousesIdHouse");

                    b.HasIndex("IdHousesIdHouse");

                    b.ToTable("AmenityHouse");
                });

            modelBuilder.Entity("G23NHNT.Models.Account", b =>
                {
                    b.Property<int>("IdUser")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdUser"), 1L, 1);

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("IdUser");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("G23NHNT.Models.Amenity", b =>
                {
                    b.Property<int>("IdAmenity")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdAmenity"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("IdAmenity");

                    b.ToTable("Amenities");
                });

            modelBuilder.Entity("G23NHNT.Models.House", b =>
                {
                    b.Property<int>("IdHouse")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdHouse"), 1L, 1);

                    b.Property<int>("HouseTypeId")
                        .HasColumnType("int");

                    b.Property<int>("IdUser")
                        .HasColumnType("int");

                    b.Property<string>("NameHouse")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("IdHouse");

                    b.HasIndex("HouseTypeId");

                    b.HasIndex("IdUser");

                    b.ToTable("Houses");
                });

            modelBuilder.Entity("G23NHNT.Models.HouseDetail", b =>
                {
                    b.Property<int>("IdHouseDetail")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdHouseDetail"), 1L, 1);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Describe")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<double>("DienTich")
                        .HasColumnType("float");

                    b.Property<int>("IdHouse")
                        .HasColumnType("int");

                    b.Property<string>("Image")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TienDien")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("TienDv")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("TienNuoc")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("TimePost")
                        .HasColumnType("datetime2");

                    b.HasKey("IdHouseDetail");

                    b.HasIndex("IdHouse");

                    b.ToTable("HouseDetails");
                });

            modelBuilder.Entity("G23NHNT.Models.HouseType", b =>
                {
                    b.Property<int>("IdHouseType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdHouseType"), 1L, 1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("IdHouseType");

                    b.ToTable("HouseType");
                });

            modelBuilder.Entity("G23NHNT.Models.Review", b =>
                {
                    b.Property<int>("IdReview")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("IdReview"), 1L, 1);

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("IdHouse")
                        .HasColumnType("int");

                    b.Property<int?>("IdRoom")
                        .HasColumnType("int");

                    b.Property<int>("IdUser")
                        .HasColumnType("int");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ReviewDate")
                        .HasColumnType("datetime2");

                    b.HasKey("IdReview");

                    b.HasIndex("IdHouse");

                    b.HasIndex("IdUser");

                    b.ToTable("Reviews");
                });

            modelBuilder.Entity("AmenityHouse", b =>
                {
                    b.HasOne("G23NHNT.Models.Amenity", null)
                        .WithMany()
                        .HasForeignKey("IdAmenitiesIdAmenity")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("G23NHNT.Models.House", null)
                        .WithMany()
                        .HasForeignKey("IdHousesIdHouse")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("G23NHNT.Models.House", b =>
                {
                    b.HasOne("G23NHNT.Models.HouseType", "HouseType")
                        .WithMany()
                        .HasForeignKey("HouseTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("G23NHNT.Models.Account", "IdUserNavigation")
                        .WithMany("Houses")
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HouseType");

                    b.Navigation("IdUserNavigation");
                });

            modelBuilder.Entity("G23NHNT.Models.HouseDetail", b =>
                {
                    b.HasOne("G23NHNT.Models.House", "IdHouseNavigation")
                        .WithMany("HouseDetails")
                        .HasForeignKey("IdHouse")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("IdHouseNavigation");
                });

            modelBuilder.Entity("G23NHNT.Models.Review", b =>
                {
                    b.HasOne("G23NHNT.Models.House", "IdHouseNavigation")
                        .WithMany("Reviews")
                        .HasForeignKey("IdHouse");

                    b.HasOne("G23NHNT.Models.Account", "IdUserNavigation")
                        .WithMany("Reviews")
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("IdHouseNavigation");

                    b.Navigation("IdUserNavigation");
                });

            modelBuilder.Entity("G23NHNT.Models.Account", b =>
                {
                    b.Navigation("Houses");

                    b.Navigation("Reviews");
                });

            modelBuilder.Entity("G23NHNT.Models.House", b =>
                {
                    b.Navigation("HouseDetails");

                    b.Navigation("Reviews");
                });
#pragma warning restore 612, 618
        }
    }
}
