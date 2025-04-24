using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLTN.Models
{
    public partial class HouseDetail
    {
        [Key]
        public int IdHouseDetail { get; set; }

        [Required(ErrorMessage = "IdHouse không được để trống.")]
        public int IdHouse { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc.")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được quá 200 ký tự.")]
        public string Address { get; set; } = null!;

        [Required(ErrorMessage = "Tiền thuê là bắt buộc.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Diện tích là bắt buộc.")]
        public double DienTich { get; set; }

        public string TienDien { get; set; } = null!;

        public string TienNuoc { get; set; } = null!;

        [Required(ErrorMessage = "Tiền dịch vụ là bắt buộc.")]
        public string TienDv { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự.")]
        public string? Describe { get; set; }

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        public string Status { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Tên hình ảnh không được quá 255 ký tự.")]
        public string? Image { get; set; }

        public DateTime TimePost { get; set; }
        public DateTime? TimeUpdate { get; set; }

        public string? ContactName2 { get; set; }
        public string? ContactPhone2 { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Foreign Key for House
        [ForeignKey("IdHouse")]
        public virtual House? IdHouseNavigation { get; set; }
    }
}
