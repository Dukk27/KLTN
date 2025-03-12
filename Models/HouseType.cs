using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLTN.Models
{
    // public enum HouseTypeEnum
    // {
    //     [Display(Name = "Nhà trọ & phòng trọ")]
    //     HouseAndRoom = 1,

    //     [Display(Name = "Nhà nguyên căn")]
    //     EntireHouse = 2,

    //     [Display(Name = "Nhà tập thể")]
    //     CollectiveHouse = 3,

    //     [Display(Name = "Kí túc xá")]
    //     Dormitory = 4,

    //     [Display(Name = "Chung cư mi ni")]
    //     MiniApartment = 5
    // }

    public partial class HouseType
    {
        [Key]
        public int IdHouseType { get; set; }
        [Required(ErrorMessage = "Tên loại nhà không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên loại nhà không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = null!;
    }
}
