using System.Collections.Generic;
using System.Threading.Tasks;
using KLTN.Models;

namespace KLTN.Repositories
{
    public interface IHouseDetailRepository
    {
        Task<IEnumerable<HouseDetail>> GetAllHouseDetailsAsync();
        Task<HouseDetail> GetHouseDetailByIdAsync(int id);
        Task AddAsync(HouseDetail houseDetail);
        Task UpdateAsync(HouseDetail houseDetail);
        Task DeleteAsync(int id);
    }
}
