// IHouseRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using KLTN.Models;
using KLTN.ViewModels;

namespace KLTN.Repositories
{
    public interface IHouseRepository
    {
        Task<IEnumerable<House>> GetAllHousesAsync();
        Task<House> GetHouseWithDetailsAsync(int id);
        Task AddAsync(House house);
        Task UpdateAsync(House house);
        Task DeleteAsync(int id);
        Task<List<House>> GetHousesByUserId(int userId);
        Task<IEnumerable<House>> GetHousesByTypeAsync(int houseTypeId);
        Task<IEnumerable<House>> GetFilteredHousesAsync(
            string searchString,
            string priceRange,
            string sortBy,
            string roomType,
            List<string> amenities,
            double? minArea,
            double? maxArea
        );
    }
}
