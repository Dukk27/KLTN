// HomeViewModel.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KLTN.Models
{
    public class HomeViewModel
    {
        public IEnumerable<HouseType>? HouseTypes { get; set; }
        public HouseType? SelectedHouseType { get; set; }
        public IEnumerable<SelectListItem>? HouseType { get; set; } = new List<SelectListItem>();
        public List<Amenity>? Amenities { get; set; } = new List<Amenity>();
        public IEnumerable<House> Houses { get; set; }
        public bool IsChuTro { get; set; }
        public bool IsAdmin { get; set; }
        public int UserRole { get; set; }
    }
}
