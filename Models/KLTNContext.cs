using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KLTN.Models
{
    public partial class KLTNContext : DbContext
    {
        public KLTNContext() { }

        public KLTNContext(DbContextOptions<KLTNContext> options)
            : base(options) { }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<Amenity> Amenities { get; set; } = null!;
        public virtual DbSet<House> Houses { get; set; } = null!;
        public virtual DbSet<HouseDetail> HouseDetails { get; set; } = null!;
        public virtual DbSet<Review> Reviews { get; set; } = null!;
        public virtual DbSet<HouseType> HouseType { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<UserPost> UserPosts { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=DefaultConnection");
            }
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Account>(entity =>
        //    {
        //        entity.HasKey(e => e.IdUser)
        //            .HasName("PK__Account__3717C98210B6EA83");

        //        entity.ToTable("Account");

        //        entity.Property(e => e.IdUser).HasColumnName("idUser");

        //        entity.Property(e => e.Password)
        //            .HasMaxLength(50)
        //            .HasColumnName("password");

        //        entity.Property(e => e.UserName)
        //            .HasMaxLength(50)
        //            .HasColumnName("userName");
        //    });

        //    modelBuilder.Entity<Amenity>(entity =>
        //    {
        //        entity.HasKey(e => e.IdAmenity)
        //            .HasName("PK__Amenitie__C1B0D589CDE1290A");

        //        entity.Property(e => e.IdAmenity).HasColumnName("idAmenity");

        //        entity.Property(e => e.Name)
        //            .HasMaxLength(100)
        //            .IsUnicode(false)
        //            .HasColumnName("name");
        //    });

        //    modelBuilder.Entity<House>(entity =>
        //    {
        //        entity.HasKey(e => e.IdHouse)
        //            .HasName("PK__House__AF515CBF563343EC");

        //        entity.ToTable("House");

        //        entity.Property(e => e.IdHouse).HasColumnName("idHouse");

        //        entity.Property(e => e.Category)
        //            .HasMaxLength(50)
        //            .HasColumnName("category");

        //        entity.Property(e => e.IdUser).HasColumnName("idUser");

        //        entity.Property(e => e.NameHouse)
        //            .HasMaxLength(255)
        //            .HasColumnName("nameHouse");


        //        entity.HasOne(d => d.IdUserNavigation)
        //            .WithMany(p => p.Houses)
        //            .HasForeignKey(d => d.IdUser)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_House_Account");

        //        entity.HasMany(d => d.IdAmenities)
        //            .WithMany(p => p.IdHouses)
        //            .UsingEntity<Dictionary<string, object>>(
        //                "HouseAmenity",
        //                l => l.HasOne<Amenity>().WithMany().HasForeignKey("IdAmenity").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__HouseAmen__idAme__5FB337D6"),
        //                r => r.HasOne<House>().WithMany().HasForeignKey("IdHouse").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK__HouseAmen__idHou__5EBF139D"),
        //                j =>
        //                {
        //                    j.HasKey("IdHouse", "IdAmenity").HasName("PK__HouseAme__234A51E7ED05E3B2");

        //                    j.ToTable("HouseAmenities");

        //                    j.IndexerProperty<int>("IdHouse").HasColumnName("idHouse");

        //                    j.IndexerProperty<int>("IdAmenity").HasColumnName("idAmenity");
        //                });
        //    });

        //    modelBuilder.Entity<HouseDetail>(entity =>
        //    {
        //        entity.HasKey(e => e.IdHouseDetail)
        //            .HasName("PK__HouseDet__946F757FD8C7B0A6");

        //        entity.ToTable("HouseDetail");

        //        entity.Property(e => e.IdHouseDetail).HasColumnName("idHouseDetail");

        //        entity.Property(e => e.Address)
        //            .HasMaxLength(255)
        //            .HasColumnName("address");

        //        entity.Property(e => e.Describe).HasColumnName("describe");

        //        entity.Property(e => e.DienTich).HasColumnName("dienTich");

        //        entity.Property(e => e.IdHouse).HasColumnName("idHouse");

        //        entity.Property(e => e.Image)
        //            .HasMaxLength(255)
        //            .HasColumnName("image");

        //        entity.Property(e => e.Price)
        //            .HasColumnType("decimal(10, 2)")
        //            .HasColumnName("price");

        //        entity.Property(e => e.Status)
        //            .HasMaxLength(50)
        //            .HasColumnName("status");

        //        entity.Property(e => e.TienDien)
        //            .HasMaxLength(20)
        //            .HasColumnName("tienDien");

        //        entity.Property(e => e.TienDv)
        //            .HasMaxLength(20)
        //            .HasColumnName("tienDV");

        //        entity.Property(e => e.TienNuoc)
        //            .HasMaxLength(20)
        //            .HasColumnName("tienNuoc");

        //        entity.Property(e => e.TimePost)
        //            .HasColumnType("datetime")
        //            .HasColumnName("timePost");

        //        entity.HasOne(d => d.IdHouseNavigation)
        //            .WithMany(p => p.HouseDetails)
        //            .HasForeignKey(d => d.IdHouse)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK_HouseDetail_House");
        //    });

        //    modelBuilder.Entity<Review>(entity =>
        //    {
        //        entity.HasKey(e => e.IdReview)
        //            .HasName("PK__Review__04F7FE108ECD5FA1");

        //        entity.ToTable("Review");

        //        entity.Property(e => e.IdReview).HasColumnName("idReview");

        //        entity.Property(e => e.Content)
        //            .HasColumnType("text")
        //            .HasColumnName("content");

        //        entity.Property(e => e.IdHouse).HasColumnName("idHouse");


        //        entity.Property(e => e.IdUser).HasColumnName("idUser");

        //        entity.Property(e => e.Rating).HasColumnName("rating");

        //        entity.Property(e => e.ReviewDate)
        //            .HasColumnType("datetime")
        //            .HasColumnName("reviewDate")
        //            .HasDefaultValueSql("(getdate())");

        //        entity.HasOne(d => d.IdHouseNavigation)
        //            .WithMany(p => p.Reviews)
        //            .HasForeignKey(d => d.IdHouse)
        //            .HasConstraintName("FK__Review__idHouse__66603565");



        //        entity.HasOne(d => d.IdUserNavigation)
        //            .WithMany(p => p.Reviews)
        //            .HasForeignKey(d => d.IdUser)
        //            .OnDelete(DeleteBehavior.ClientSetNull)
        //            .HasConstraintName("FK__Review__idUser__6477ECF3");
        //    });


        //}

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
