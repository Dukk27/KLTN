using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLTN.Models
{
    public partial class HouseType
    {
        [Key]
        public int IdHouseType { get; set; }
        [Required(ErrorMessage = "Tên loại nhà không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên loại nhà không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = null!;
    }
}
