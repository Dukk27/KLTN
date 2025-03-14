using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLTN.Models
{
    public partial class Account
    {
        public Account()
        {
            Houses = new HashSet<House>();
            Reviews = new HashSet<Review>();
        }

        [Key]
        public int IdUser { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập phải dưới 50 ký tự.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [RegularExpression(
            @"^(?=.*\d)(?=.*[a-zA-Z]).{8,}$",
            ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, chứa ít nhất 1 chữ cái và 1 số."
        )]
        public string Password { get; set; }

        [Range(1, 2, ErrorMessage = "Vai trò không hợp lệ.")]
        public int Role { get; set; }

        [Phone]
        [StringLength(15, ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; }

        public int FreePostsUsed { get; set; }

        // Navigation properties
        public virtual ICollection<House> Houses { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<UserPost> UserPosts { get; set; }
    }
}
