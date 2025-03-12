using System.ComponentModel.DataAnnotations;

namespace KLTN.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public int HouseId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentStatus { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }

        public virtual Account User { get; set; }
        public virtual House House { get; set; }
    }
}
