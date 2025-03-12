using System.ComponentModel.DataAnnotations;

namespace KLTN.Models
{
    public class UserPost
    {
        [Key]
        public int UserPostId { get; set; }
        public int UserId { get; set; }
        public int HouseId { get; set; }
        public bool IsFree { get; set; }
        public DateTime PostDate { get; set; }

        public virtual Account User { get; set; }
        public virtual House House { get; set; }
    }
}
