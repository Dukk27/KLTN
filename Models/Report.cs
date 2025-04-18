using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KLTN.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int HouseId { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;

        [ForeignKey("UserId")]
        public virtual Account? User { get; set; }

        [ForeignKey("HouseId")]
        public virtual House? House { get; set; }
        public int ReportVersion { get; set; }
    }
}
