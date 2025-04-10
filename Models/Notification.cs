using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLTN.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Message { get; set; } 

        [Required]
        public int UserId { get; set; } 

        public bool IsRead { get; set; } = false; 

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Thiết lập khóa ngoại đến bảng Account
        [ForeignKey("UserId")]
        public virtual Account? User { get; set; }
    }
}
