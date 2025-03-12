using KLTN.Models;

namespace KLTN.ViewModels
{
    public class HouseDetailViewModel
    {
        public House House { get; set; }
        public List<HouseDetail> HouseDetails { get; set; }
        public List<Amenity> IdAmenities { get; set; }
        public IEnumerable<Review> Reviews { get; set; }
        public IEnumerable<HouseType> HouseTypes { get; set; }
    }
}
