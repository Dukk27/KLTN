using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLTN.Models
{
    public partial class Review
    {
        [Key]
        public int IdReview { get; set; } 

        [Required(ErrorMessage = "Bạn phải đăng nhập")]
        public int IdUser { get; set; }
        public int? IdHouse { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải nằm trong khoảng từ 1 đến 5.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Nội dung đánh giá không được để trống.")]
        [StringLength(500, ErrorMessage = "Nội dung đánh giá không được vượt quá 500 ký tự.")]
        public string? Content { get; set; }

        public DateTime? ReviewDate { get; set; }


        [ForeignKey("IdHouse")]
        public virtual House? IdHouseNavigation { get; set; }
        [ForeignKey("IdUser")]
        public virtual Account? IdUserNavigation { get; set; }
    }
}
