using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLTN.Models
{
    public enum AppointmentStatus
    {
        Pending = 0, // Chờ xác nhận
        Confirmed = 1, // Đã xác nhận
        Cancelled =
            2 // Đã hủy
        ,
    }

    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int HouseId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public bool IsNotified { get; set; } = false; // Kiểm tra gửi thông báo

        [Required]
        [Column(TypeName = "int")]
        public AppointmentStatus Status { get; set; }

        public bool IsReminderSent { get; set; } = false; // Gửi nhắc nhở

        public virtual Account? User { get; set; }

        public virtual House? House { get; set; }
    }
}
