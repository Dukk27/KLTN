using System.Collections.Generic;
using System.Threading.Tasks;
using KLTN.Models;

namespace KLTN.Repositories
{
    public interface IAmenityRepository
    {
        Task<IEnumerable<Amenity>> GetAllAmenitiesAsync();
        Task<Amenity> GetAmenityByIdAsync(int id);
        Task AddAsync(Amenity amenity);
        Task UpdateAsync(Amenity amenity);
        Task DeleteAsync(int id);
    }
}
