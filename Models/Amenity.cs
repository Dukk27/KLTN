using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KLTN.Models
{
    public partial class Amenity
    {
        public Amenity()
        {
            IdHouses = new HashSet<House>();
        }

        [Key]
        public int IdAmenity { get; set; }

        [Required(ErrorMessage = "Tên tiện nghi là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên tiện nghi phải dưới 100 ký tự.")]
        public string Name { get; set; } = null!;

        // Navigation property
        public virtual ICollection<House> IdHouses { get; set; }
    }
}
